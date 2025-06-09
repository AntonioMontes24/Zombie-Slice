using UnityEngine;

[CreateAssetMenu(fileName = "NewGunStats", menuName = "Weapon/Gun Stats")]
public class GunStats : ScriptableObject
{

    [Header("Weapons Stats")]
    [SerializeField] public GameObject gunModel;
    [SerializeField] public int shootDamage;
    [SerializeField] public float shootRate;
    [SerializeField] public int shootRange;
    [SerializeField] public int ammoCur, ammoMax;
    [SerializeField] public float reloadTime;
    [SerializeField] public int ammoReserve;

    [Header("Weapon Fire Type")]
    [SerializeField] public bool canSwitchFireMode;
    [SerializeField] public bool isAutomaticDefault;
    [SerializeField] public float autoFireRate;
    [SerializeField] public float semiFireRate;

    [Header("VFX")]
    [SerializeField] public GameObject bulletHolePrefab;
    [SerializeField] public ParticleSystem hitEffect;

    [Header("SFX")]
    [SerializeField] public AudioClip shootSound;
    [SerializeField] public AudioClip reloadSound;
    [SerializeField] public AudioClip emptySound;
    [SerializeField] public float shootVol;
    [SerializeField] public AudioClip fireModeSwitchSound;
}
