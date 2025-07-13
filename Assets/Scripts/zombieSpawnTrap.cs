using UnityEngine;

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
                Destroy(gameObject);
            }
        }
    }

    private object SpawnZombiesAround()
    {
        throw new System.NotImplementedException();
    }

    private void SpawnZombiesAround(Vector3 center)
    {
        for (int i = 0; i < numOfZombies; i++) {

            Vector3 spawnPosition = GetRandomSpawn(center);
            Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
        }
    }

    private Vector3 GetRandomSpawn(Vector3 center)
    {

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float distance = Random.Range(minSpawnRadius, maxSpawnRadius);

        Vector3 randomPoint = center + new Vector3(randomDir.x, 0, randomDir.y) * distance;

        if (Physics.Raycast(randomPoint + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {

            return hit.point;

        }

        return randomPoint;
    }

}
