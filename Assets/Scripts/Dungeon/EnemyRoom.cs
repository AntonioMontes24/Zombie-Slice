using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyRoom : MonoBehaviour
{
    [SerializeField] List<GameObject> enemyPrefabs;
    [SerializeField] List<Transform> spawnPoints;
    [SerializeField] Collider triggerzone;
    [SerializeField] int spawnCount;
    [SerializeField] List<doorScript> doorsToLock = new List<doorScript>();
    

    bool roomTriggered;
    bool doorUnlocked = false;
    private List<GameObject> activeEnemies = new();

    public bool RoomTriggered => roomTriggered;

    void OnTriggerEnter(Collider other)
    {
        if (!roomTriggered && other.CompareTag("Player"))
        {
            roomTriggered = true;
            LockDoor();

            SpawnEnemies();
        }
    }
    void Update()
    {
        if (roomTriggered && !doorUnlocked && AreAllEnemiesDefeated())
        {
            doorUnlocked = true;
            UnlockDoor();
        }
    }



    void LockDoor()
    {
        foreach (var door in doorsToLock)
        {
            if (door != null)
            {
                door.LockDoorByEnemies();
            }
        }

    }

    void UnlockDoor()
    {
        foreach (var door in doorsToLock)
        {
            if (door != null)
            {
                door.UnlockDoor();
            }
        }
    }

    void SpawnEnemies()
{
    activeEnemies.Clear();

    // Safety: Clamp the count to how many spawn points you actually have
    int count = Mathf.Min(spawnCount, spawnPoints.Count);

    List<int> usedIndices = new List<int>();

    for (int i = 0; i < count; i++)
    {
        // Choose a random unused spawn point
        int spawnIndex;
        do
        {
            spawnIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
        }
        while (usedIndices.Contains(spawnIndex));
        usedIndices.Add(spawnIndex);

        Transform spawnPoint = spawnPoints[spawnIndex];

        // Choose a random enemy prefab
        GameObject enemyPrefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Count)];

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        activeEnemies.Add(enemy);
    }
}


    public bool AreAllEnemiesDefeated()
    {
        return activeEnemies.TrueForAll(e => e == null || !e.activeInHierarchy);
    }

   
}
