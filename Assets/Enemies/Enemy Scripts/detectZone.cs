using UnityEngine;

public class detectZone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public flyingEnemyAI parentscript;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            parentscript.playerInRange(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            parentscript.playerInRange(false);
        }
    }
}
