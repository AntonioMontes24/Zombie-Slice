using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeStats", menuName = "Weapon/Melee Stats")]
public class MeleeWeaponStats : WeaponStats
{
    [Header("Melee")]
    [SerializeField]public int damage;
    [SerializeField] public float attackRate;
    [SerializeField] public AudioClip swingSound;
    [SerializeField] public AudioClip hitSound;
    [SerializeField] public ParticleSystem hitEffect;}
