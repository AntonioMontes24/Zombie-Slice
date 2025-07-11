using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyRoom : MonoBehaviour
{
    [SerializeField] EnemySpawner spawner;
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
//     public void TriggerRoom()
// {
//     if (!roomTriggered)
//     {
//         roomTriggered = true;

//         if (spawner != null)
//         {
//             List<GameObject> spawnedEnemies = spawner.SpawnEnemies();
//             enemies.AddRange(spawnedEnemies);
//         }

//         LockDoors(); // Optional, only if you want the doors to close after spawning
//     }
// }


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
        for (int i = 0; i < spawnCount; i++)
        {
            var max = spawner.GetTotalSpawnArea();
            var pick = Random.Range(0, max);
            float current = 0f;

            foreach (var component in spawner.GetSpawnComponents())
            {
                current += component.GetSize();
                if (current > pick)
                {
                    var enemy = Instantiate(spawner.PickEnemy(), component.GetSpawnPosition(), Quaternion.identity);
                    activeEnemies.Add(enemy);
                    break;
                }

            }
        }
    }
    public bool AreAllEnemiesDefeated()
    {
        return activeEnemies.TrueForAll(e => e == null || !e.activeInHierarchy);
    }

   
}
