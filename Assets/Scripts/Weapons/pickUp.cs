using UnityEngine;

public class pickup : PickupBase
{
    [SerializeField] GunStats gun;
    [SerializeField] int startingAmmo = -1;
    [SerializeField] private int reserveAmmo = -1;

    protected override void Start()
    {
        base.Start();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerWeaponManager weaponManager = other.GetComponent<PlayerWeaponManager>();
            if (weaponManager != null)
            {
                weaponManager.GetGunStats(gun, startingAmmo, reserveAmmo);// Gives us the ability to modify starting ammo of a weapon 
            }
            Destroy(gameObject);
        }
    }
}
