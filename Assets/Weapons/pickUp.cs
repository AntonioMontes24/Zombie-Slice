using System.Collections;
using UnityEngine;

public class pickup : MonoBehaviour
{
    [SerializeField] GunStats gun;

    private void Start()
    {
        gun.ammoCur = gun.ammoMax;
    }

    private void OnTriggerEnter(Collider other)
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
