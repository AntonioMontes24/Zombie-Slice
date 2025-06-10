using UnityEngine;

public class HealthPickup : PickupBase
{
    [SerializeField] int health;
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || !other.CompareTag("Player"))
            return;
        var healthComponent = other.GetComponent<PlayerHealth>();
        if (!healthComponent.CanHeal())
            return;
        healthComponent.Heal(health);
        Destroy(transform.parent.gameObject);
    }
}
