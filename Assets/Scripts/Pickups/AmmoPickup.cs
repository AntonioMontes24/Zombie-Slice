using UnityEngine;

public class AmmoPickup : PickupBase
{
    [SerializeField] int bullets;
    [SerializeField] AudioSource sourceAudio;
    [SerializeField] AudioClip pickUpSoundAmmo;
    [SerializeField] float pickUpSoundVol;
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || !other.CompareTag("Player"))
            return;
        var weapon = other.GetComponent<PlayerWeaponManager>();
        if (weapon == null)
            return;

        if(weapon.CurrentGun.ammoReserve >= weapon.CurrentGun.maxAmmoReserve)
            return;
        weapon.AddAmmoToReserve(weapon.CurrentGun.weaponNameId,bullets);
        sourceAudio.PlayOneShot(pickUpSoundAmmo, pickUpSoundVol);
        Destroy(transform.gameObject, pickUpSoundAmmo.length);
    }
}
