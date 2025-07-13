using UnityEngine;

public abstract class WeaponStats : ScriptableObject
{
    [Header("General")]
    [SerializeField] public GameObject weaponModel;
    [SerializeField] public Sprite weaponIcon;
    public bool isOneHanded;

    [Header("Weapon ID")]
    public string weaponNameId;
}