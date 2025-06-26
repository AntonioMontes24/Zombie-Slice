using UnityEngine;

public class HealthPickup : PickupBase
{
    [SerializeField] int health;
    [SerializeField] AudioSource sourceAudio;
    [SerializeField] AudioClip pickUpSoundHealth;
    [SerializeField] float pickUpSoundVol;
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || !other.CompareTag("Player"))
            return;
        var healthComponent = other.GetComponent<PlayerHealth>();
        if (!healthComponent.CanHeal())
            return;
        healthComponent.Heal(health);
        sourceAudio.PlayOneShot(pickUpSoundHealth,pickUpSoundVol);
        Destroy(transform.gameObject,pickUpSoundHealth.length);
    }
}
