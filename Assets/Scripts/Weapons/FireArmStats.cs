using UnityEngine;

[CreateAssetMenu(fileName = "NewFirearmStats", menuName = "Weapon/Firearm Stats")]
public class FireArmStats : WeaponStats
{

    [Header("Shooting")]
    [SerializeField] public int shootDamage;
    [SerializeField] public float shootRate;
    [SerializeField] public int shootRange;

    [Header("Ammo")]
    [SerializeField] public int ammoCur, ammoMax;
    [SerializeField] public float reloadTime;
    [SerializeField] public int ammoReserve;
    [SerializeField] public int maxAmmoReserve;
    [SerializeField] public AmmoTypes ammoType;

    [Header("Fire modes")]
    [SerializeField] public bool canSwitchFireMode;
    [SerializeField] public bool isAutomaticDefault;
    [SerializeField] public float autoFireRate;
    [SerializeField] public float semiFireRate;

    [Header("VFX")]
    [SerializeField] public GameObject bulletHolePrefab;
    [SerializeField] public ParticleSystem hitEffect;
    [SerializeField] public GameObject zombieBloodHit;

    [Header("SFX")]
    [SerializeField] public AudioClip shootSound;
    [SerializeField] public AudioClip reloadSound;
    [SerializeField] public AudioClip emptySound;
    [SerializeField] public AudioClip fireModeSwitchSound;
    [SerializeField] public AudioClip reloadFreakingZombie;

    public FireArmStats Clone()
    {
        return Instantiate(this);
    }
}
