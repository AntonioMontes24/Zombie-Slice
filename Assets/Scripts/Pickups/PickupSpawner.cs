using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [SerializeField] GameObject pickup;
    [SerializeField] int maxSpawn, minSpawn;
    [SerializeField] AnimationCurve spawnProbabilityFalloff;
    [SerializeField] float maxMoveForce, minMoveForce;
    [SerializeField] float maxJumpForce, minJumpForce;

    private void OnDestroy()
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
            var g = Instantiate(pickup, transform.position, Quaternion.identity);
            var forceY = Random.Range(minJumpForce, maxJumpForce);
            var forceX = Random.Range(minMoveForce, maxMoveForce);
            var forceZ = Random.Range(minMoveForce, maxMoveForce);
            Debug.Log((forceZ, forceX, forceY));
            g.GetComponent<Rigidbody>().AddForce(new Vector3(forceX, forceY, forceZ), ForceMode.Impulse);
        }
    }
}
