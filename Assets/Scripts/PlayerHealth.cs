using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float playerHealth=100f;
    public bool isAlive=true;
    public Explosion explosion;
    public void Damage(float min,float max)
    {
        if (isAlive)
        {
            float randomFloat = Random.Range(min, max);

            playerHealth=playerHealth-randomFloat;
            Debug.Log("Player Health : "+playerHealth);
            if (playerHealth < 0f)
            {
                isAlive=false;
                Invoke("destruct",1);
            }
        }
        else
        {
            Debug.Log("Player Dead");
        }
    }
    void destruct()
    {
        explosion.Explode();
    }
}
