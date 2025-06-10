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
        weapon.AddAmmoToReserve(bullets);
        Destroy(transform.parent.gameObject);
    }
}
