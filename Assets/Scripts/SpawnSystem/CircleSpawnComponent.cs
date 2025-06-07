using UnityEngine;

public class CircleSpawnComponent : MonoBehaviour, ISpawnComponent
{
    [SerializeField]
    Transform center;
    [SerializeField]
    float radius;
    public Vector3 GetSpawnPosition()
    {
        while (true)
        {
            var x = Random.Range(-radius, radius);
            var z = Random.Range(-radius, radius);
            if (Vector2.Distance(new Vector2(x, z), Vector2.zero) < radius)
                return new Vector3(x, 0, z) + center.position;
        }
    }
    private void OnDrawGizmos() 
    {
        if (center == null) 
            return;
        Gizmos.DrawWireSphere(center.position, radius);
    }
    public float GetSize() => radius * 2f;
}
