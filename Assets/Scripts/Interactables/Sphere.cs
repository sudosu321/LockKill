using UnityEngine;
using UnityEngine.AI;

public class Sphere : Interactable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private GameObject door;
    public NavMeshObstacle obstacle;
    private bool doorOpen;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    protected override void Interact()
    {
        doorOpen=!doorOpen;
        Debug.Log("DOOR STATE" +doorOpen);
        door.GetComponent<Animator>().SetBool("isOpen",doorOpen);
        obstacle.enabled=doorOpen; 

        Debug.Log("OBSTACLE STATE" +doorOpen);
    }
}
