using UnityEngine;
using System.Collections.Generic;

public class PlayerPersistentData : MonoBehaviour
{
    public static PlayerPersistentData instance;

    public int savedHealth;
    public float currentHealth;
    public List<WeaponSaveData> savedWeapons = new List<WeaponSaveData>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SavePlayerData(float health, List<WeaponSaveData> weapons)
    {
        currentHealth = health;
        savedWeapons = new List<WeaponSaveData>(weapons); // Deep copy
    }
}