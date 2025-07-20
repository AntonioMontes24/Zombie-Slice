using UnityEngine;

public class MeatyZombieFist : MonoBehaviour
{
    [SerializeField] public int fist_damage;

    private void OnTriggerEnter(Collider other)
    {
        // the other object in range is Player
        if (other.CompareTag("Player"))
        {
            // assign damage to the player
            IDamage player_dmg = GameManager.instance.player.GetComponent<IDamage>();
            player_dmg.takeDamage(fist_damage);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // the other object leaving range is Player
        if (other.CompareTag("Player"))
        {
            
        }
    }
}
