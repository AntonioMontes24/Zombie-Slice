using UnityEngine;
using UnityEngine.AI;

public class zombieSpawnTrap : MonoBehaviour
{

    [Header("Spawner Settings")]
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private int numOfZombies;
    [SerializeField] private float minSpawnRadius;
    [SerializeField] private float maxSpawnRadius;
    [SerializeField] private LayerMask groundLayer;

    [Header("Trigger")]
    [SerializeField] private bool destroyAfterTrigger = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            SpawnZombiesAround(other.transform.position);
            hasTriggered = true;

            if (destroyAfterTrigger)
            {
                if(ObjectiveManager.instance != null)
                {
                    ObjectiveManager.instance.updateSpawnerCount(-1);
                }
                Destroy(gameObject);
            }
        }
    }

    private void SpawnZombiesAround(Vector3 center)
    {
        for (int i = 0; i < numOfZombies; i++) {
            
            Vector3 spawnPosition = GetRandomSpawn(center);
            if(spawnPosition != Vector3.zero)
            {
                Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
                if (ObjectiveManager.instance != null)
                {
                    ObjectiveManager.instance.updateZombieCount(1);
                }
            }
        }
    }

    private Vector3 GetRandomSpawn(Vector3 center)
    {

        for(int i = 0; i < 10; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized * Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector3 randomPoint = center + new Vector3(randomDir.x, 0, randomDir.y);

            if(Physics.Raycast(randomPoint + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f, groundLayer))
            {
                Vector3 groundPosition = hit.point;
                NavMeshHit navHt;
                if(NavMesh.SamplePosition(groundPosition, out navHt, 5.0f, NavMesh.AllAreas))
                {
                    return navHt.position;
                }
            }
        }
        return Vector3.zero;
    }

}
