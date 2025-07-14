using UnityEngine;

public class pickup : PickupBase
{
    [SerializeField] WeaponStats gun;
    [SerializeField] int startingAmmo = -1;
    [SerializeField] private int reserveAmmo = -1;

    [Header("Ammo Randomizer")]
    [SerializeField] private int minReserveAmmo = 5; // Minimum reserve ammo on pickup
    [SerializeField] private int maxReserveAmmo = 30; // Maximum reserve ammo on pickup

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
                // If player already owns this weapon, just give reserve ammo
                if (weaponManager.HasGun(gun.weaponNameId))
                {
                    // Only applies to firearms
                    if (gun is FireArmStats fireArm)
                    {
                        int ammoToGive = Random.Range(minReserveAmmo, maxReserveAmmo + 1);
                        weaponManager.AddAmmoToReserve(fireArm.ammoType, ammoToGive);
                    }
                }
                else
                {
                    // If player does not have the weapon, give it with optional randomized reserve ammo
                    int randomReserve = (reserveAmmo >= 0) ? reserveAmmo : Random.Range(minReserveAmmo, maxReserveAmmo + 1);
                    weaponManager.GetGunStats(gun, startingAmmo, randomReserve);// Gives us the ability to modify starting ammo of a weapon 
                }
            }

            Destroy(gameObject);// Destroy pickup
        }
    }
}
