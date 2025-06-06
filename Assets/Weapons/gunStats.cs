using UnityEngine;

[CreateAssetMenu(fileName = "NewGunStats", menuName = "Weapon/Gun Stats")]
public class GunStats : ScriptableObject
{
    [Header("Weapons Stats")]
    public GameObject gunModel;
    public int shootDamage;
    public float shootRate;
    public int shootRange;
    public int ammoCur, ammoMax;
    public float reloadTime;

    [Header("Weapon Fire Type")]
    public bool canSwitchFireMode;
    public bool isAutomaticDefault;
    public float autoFireRate;
    public float semiFireRate;

    [Header("VFX")]
    public GameObject bulletHolePrefab;
    public ParticleSystem hitEffect;

    [Header("SFX")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    [Range(0, 1)] public float shootVol = 1f;
    public AudioClip fireModeSwitchSound;
}
