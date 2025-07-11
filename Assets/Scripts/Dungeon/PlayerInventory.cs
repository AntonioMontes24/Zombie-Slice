using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;
    private HashSet<string> keys = new HashSet<string>();
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool HasKey(string keyName)
    {
        return keys.Contains(keyName);
    }

    public void AddKey(string keyName)
    {
        keys.Add(keyName);
    }
}
