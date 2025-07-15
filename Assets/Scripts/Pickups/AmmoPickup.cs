using System.Collections;
using UnityEngine;

public class AmmoPickup : PickupBase
{
    [SerializeField] int bullets;
    [SerializeField] AmmoTypes ammoType;
    [SerializeField] AudioSource sourceAudio;
    [SerializeField] AudioClip pickUpSoundAmmo;
    [SerializeField] float pickUpSoundVol;

    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || !other.CompareTag("Player"))
            return;

        var weapon = other.GetComponent<PlayerWeaponManager>();
        if (weapon == null) return;

        if (!weapon.HasWeaponWithAmmoType(ammoType))
        {
            Debug.Log($"[AmmoPickup] Player has no gun using {ammoType}, skipping.");
            return;
        }

        bool success = weapon.AddAmmoToReserve(ammoType, bullets);
        if (!success) return;
        isCollected = true;
        if (sourceAudio && pickUpSoundAmmo)
            StartCoroutine(AmmoPickUpSFX());
        else
            Destroy(gameObject);
    }

    IEnumerator AmmoPickUpSFX()
    {
        sourceAudio.PlayOneShot(pickUpSoundAmmo, pickUpSoundVol);
        yield return new WaitForSeconds(pickUpSoundAmmo.length);
        Destroy(gameObject);
    }
}
