using UnityEngine;

public class headHit : MonoBehaviour
{
    // private newEnemyAI_V2 enemyAI;
    // int damageDouble;
    // int amount;
    // void Start()
    // {
    //     enemyAI = GetComponentInParent<newEnemyAI_V2>();
    // }
    // public void takeDamage(int amount)
    // {
    //     enemyAI.takeDamage(damageDouble);
    // }
    public bool hit;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            // damageDouble = amount * 2;
            hit = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            hit = false;
        }
    }

}
