using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PickupItem
    {
        public GameObject pickupPrefab;
        public float spawnChance;
    }

    [SerializeField] PickupItem[] pickups;
    [SerializeField] int maxSpawn, minSpawn;
    [SerializeField] AnimationCurve spawnProbabilityFalloff;
    [SerializeField] float maxSpawnArea, minSpawnArea;

    [ContextMenu("Spawn Test")]
    public void Spawn()
    {
        float count;
        while (true)
        {
            count = Random.Range(minSpawn, maxSpawn);
            if (spawnProbabilityFalloff.Evaluate((float)count / maxSpawn) < Random.Range(0f, 1f))
                continue;
            break;
        }

        // Randomly pick items to spawn
        for (int i = 0; i < count; i++)
        {
            // Determine which pickup to spawn based on spawn chances
            GameObject pickupToSpawn = GetRandomPickup();

            var t = Instantiate(pickupToSpawn, transform.position, Quaternion.identity).GetComponent<Transform>();
            var pos = transform.position + new Vector3(Random.Range(minSpawnArea, maxSpawnArea), 0, Random.Range(minSpawnArea, maxSpawnArea));
            t.SetPositionAndRotation(pos, Quaternion.Euler(0, Random.Range(0, 360), 0));
        }
    }

    private GameObject GetRandomPickup()
    {
        float totalChance = 0f;
        foreach (var pickup in pickups)
        {
            totalChance += pickup.spawnChance;
        }

        float randomValue = Random.Range(0f, totalChance);
        float currentChance = 0f;

        foreach (var pickup in pickups)
        {
            currentChance += pickup.spawnChance;
            if (randomValue <= currentChance)
            {
                return pickup.pickupPrefab;
            }
        }

        return pickups[0].pickupPrefab;
    }
}
