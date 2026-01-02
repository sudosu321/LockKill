using System;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float waitTime = 1f;

    [Header("Detection")]
    public float detectionRange = 20f;
    public float attackRange = 10f;

    private NavMeshAgent agent;
    private Transform player;

    private int currentPoint;
    private float waitCounter;
    private bool playerDetected;
    private bool attack;
    [Header("Shooting")]
    public float shootCooldown = 1f;
    private float nextShootTime;
    public ParticleSystem muzzleFlash;
    public GunRecoil gun; // your existing gun / recoil script

    public Material normalEyes;
    public Material chaseEyes;
    public ChangeEyes change1;
    public ChangeEyes change2;
    public GameObject arm;
    public Vector3 rotationSpeed = new Vector3(90f, 90f, 90f); // degrees per second
    private Quaternion armStartRot;
    public float gunUpAngle = -90f;   // X axis (adjust sign if needed)
    public float armSpeed = 6f;
    public float angleTolerance = 5f; // degrees
    bool gunIsRaised=false;
    public int bulletShottedAfterAttackStart=0;
    public PlayerHealth playerHealth;
    public PlayerHealth enemy;


    [Header("Line of Sight")]
    public Transform eyes;                 // empty object at head
    public LayerMask obstacleMask;         // walls
    public LayerMask playerMask;           // player
    bool patrolInitialized;
    void Start()
    {
        nextShootTime = 4f;
        armStartRot = arm.transform.localRotation;

        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[0].position);
    }

    void Update()
    {
        if (agent.isStopped == true)
        {
            if (attack == false && playerDetected==true)
            {
                agent.isStopped=false;
            }
        }
        if (enemy.isAlive)
        {
            
            if (playerHealth.isAlive)
            {
                DetectPlayer();

                if (attack)
                {
                    AttackPlayer();
            
                }
                else if (playerDetected)
                {
                    ChasePlayer();
                }
                else
                {
                    Patrol();
                }
            }
            else
            {
                attack=false;
                playerDetected=false;
                Patrol();
            }
        }
    }

    void TryShoot()
    {
        if (Time.time < nextShootTime) return;

        nextShootTime = Time.time + shootCooldown;

        if (gun != null)
        {
            //gun.(); // this should already do recoil + muzzle flash
            if (gunIsRaised)
            {
                gun.Recoil();
                
            }
            if (muzzleFlash != null)
                muzzleFlash.Play();
            
        }   
        if(playerHealth!=null)
            playerHealth.Damage(10,20);
    }

    void AttackPlayer()
    {
        agent.isStopped = true;
        FacePlayer();
        AngryEyes();
        raiseHand();
        TryShoot();
    }
    void AngryEyes()
    {
        change1.changeMat(chaseEyes);
        change2.changeMat(chaseEyes);
    }
   
    void raiseHand()
    {
        Quaternion targetRot = armStartRot * Quaternion.Euler(gunUpAngle, 0f, 0f);

        arm.transform.localRotation = Quaternion.Lerp(
            arm.transform.localRotation,
            targetRot,
            Time.deltaTime * armSpeed
        );
        gunIsRaised = Quaternion.Angle(
                arm.transform.localRotation,
                targetRot
            ) <= angleTolerance;
    }
    void LowerHand()
    {
        arm.transform.localRotation = Quaternion.Lerp(
            arm.transform.localRotation,
            armStartRot,
            Time.deltaTime * armSpeed
        );
        gunIsRaised=false;
    }

    void DetectPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > detectionRange)
        {
            playerDetected = false;
            attack = false;
            return;
        }

        Vector3 origin = eyes.position;
        Vector3 targetPoint = player.position + Vector3.up * 1.2f;
        Vector3 direction = (targetPoint - origin).normalized;

        Debug.DrawRay(origin, direction * detectionRange, Color.red);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, detectionRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                if (distance <= attackRange)
                {
                    attack = true;
                    playerDetected = false;
                }
                else
                {
                    playerDetected = true;
                    attack = false;
                }
            }
            else
            {
                // Hit something else first (wall, door, etc)
                playerDetected = true;
                attack = false;
            }
        }
    }

    void ChasePlayer()
    {
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            Debug.Log("❌ INVALID PATH");
        }
        else if (agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            Debug.Log("⚠ PARTIAL PATH");
        }
        FacePlayer();
        AngryEyes();
        agent.isStopped = false;
        LowerHand();

        //if (agent.remainingDistance > agent.stoppingDistance)
            agent.SetDestination(player.position);
    }
    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);
    }
    void Patrol()
    {
        agent.isStopped = false;    
        if(!patrolInitialized && !playerHealth.isAlive)
            if (agent.hasPath)
            {
                agent.ResetPath();   // ✅ FORCE NEW PAT
                patrolInitialized=true;
            }
        bulletShottedAfterAttackStart=0;
        LowerHand();
        change1.changeMat(normalEyes);
        change2.changeMat(normalEyes);
        if (patrolPoints.Length == 0)
            return;

        //agent.isStopped = false;

        // 🔴 THIS IS THE FIX
        agent.SetDestination(patrolPoints[currentPoint].position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitCounter += Time.deltaTime;

            if (waitCounter >= waitTime)
            {
                currentPoint = (currentPoint + 1) % patrolPoints.Length;
                waitCounter = 0f;
            }
        }
    }
}
