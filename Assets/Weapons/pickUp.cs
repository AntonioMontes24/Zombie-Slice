using UnityEngine;

public class pickup : PickupBase
{
    [SerializeField] GunStats gun;

    protected override void Start()
    {
        base.Start();
        gun.ammoCur = gun.ammoMax;
    }

    private void OnTriggerEnter(Collider other)//---Handles weapon pick up
    {
        if (other.CompareTag("Player"))
        {
            PlayerWeaponManager weaponManager = other.GetComponent<PlayerWeaponManager>();
            if (weaponManager != null)
            {
                weaponManager.GetGunStats(gun);
            }
            Destroy(gameObject);
        }
    }
}
