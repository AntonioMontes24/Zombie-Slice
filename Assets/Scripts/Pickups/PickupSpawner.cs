using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] pickup;
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
        for (int i = 0; i < count; i++)
        {
            var t = Instantiate(pickup[Random.Range(0, pickup.Length - 1)], transform.position, Quaternion.identity).GetComponent<Transform>();
            var pos = transform.position + new Vector3(Random.Range(minSpawnArea, maxSpawnArea), 0, Random.Range(minSpawnArea, maxSpawnArea));
            t.SetPositionAndRotation(pos, Quaternion.Euler(0, Random.Range(0, 360), 0));
        }
    }
}
