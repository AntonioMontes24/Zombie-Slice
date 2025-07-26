using UnityEngine;
using UnityEngine.AI;

public class PickupSpawner : MonoBehaviour
{

    public static PickupSpawner instance;

    [System.Serializable]
    public class PickupItem
    {
        public GameObject pickupPrefab;
        [Range(0f, 1f)]
        public float spawnChance;
    }

    [SerializeField] PickupItem[] pickups;
    [SerializeField] int maxSpawn, minSpawn;
    [SerializeField] AnimationCurve spawnProbabilityFalloff;
    [SerializeField] float maxSpawnArea, minSpawnArea;
    [SerializeField] float spawnForce = 5f; // for pop effect


    [Header("NavMesh Spawn Settings")]
    [SerializeField] float navMeshSampleRadius = 2.0f; // how far to search for a valid nav position
    [SerializeField] int navMeshAreaMask = NavMesh.AllAreas;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }


    [ContextMenu("Spawn Test")]
    public void SpawnPickupsAtLocation(Vector3 deathPosition)
    {
        float count;
        while (true)
        {
            count = Random.Range(minSpawn, maxSpawn + 1);
            if (spawnProbabilityFalloff.Evaluate((float)count / maxSpawn) < Random.Range(0f, 1f))
                continue;
            break;
        }

        // Randomly pick items to spawn
        for (int i = 0; i < count; i++)
        {
            // Determine which pickup to spawn based on spawn chances
            GameObject pickupToSpawn = GetRandomPickup();

            if(pickupToSpawn == null)
            {
                continue;
            }

            Vector3 randomOffset = new Vector3(Random.Range(minSpawnArea, maxSpawnArea) * (Random.value > 0.5f ? 1 : -1), 0, Random.Range(minSpawnArea, maxSpawnArea) * (Random.value > 0.5f ? 1 : -1));

            Vector3 potentialSpawnPos = deathPosition + randomOffset;

            NavMeshHit hit;
            if(NavMesh.SamplePosition(potentialSpawnPos, out hit, navMeshSampleRadius, navMeshAreaMask))
            {
                Vector3 finalSpawnPos = hit.position + Vector3.up * 0.5f;
                GameObject spawnedItem = Instantiate(pickupToSpawn, finalSpawnPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
                Rigidbody rb = spawnedItem.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = spawnedItem.AddComponent<Rigidbody>();
                }
                Vector3 popDirection = (Vector3.up + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized);
                rb.AddForce(popDirection * spawnForce, ForceMode.Impulse);
            }
        }
    }

    private GameObject GetRandomPickup()
    {
        float totalChance = 0f;
        foreach (var pickup in pickups)
        {
            totalChance += pickup.spawnChance;
        }

        if(totalChance == 0f)
        {
            return null;
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

        if(pickups.Length > 0)
        {
            return pickups[0].pickupPrefab;
        }

        return null;
    }
}
