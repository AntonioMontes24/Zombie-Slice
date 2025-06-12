using UnityEngine;

public class AmmoPickup : PickupBase
{
    [SerializeField] int bullets;
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || !other.CompareTag("Player"))
            return;
        var weapon = other.GetComponent<PlayerWeaponManager>();
        if (weapon == null)
            return;

        if(weapon.CurrentGun.ammoReserve >= weapon.CurrentGun.maxAmmoReserve)
            return;
        weapon.AddAmmoToReserve(bullets);
        Destroy(transform.gameObject);
    }
}
