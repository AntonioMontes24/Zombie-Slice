using UnityEngine;

public class RectangleSpawnComponent : MonoBehaviour, ISpawnComponent
{
    [SerializeField]
    Transform center;
    [SerializeField]
    float x, z;

    public Vector3 GetSpawnPosition()
    {
        var x2 = Random.Range(-(x / 2), (x / 2));
        var z2 = Random.Range(-(z / 2), (z / 2));
        return new Vector3(x2, 0, z2) + center.position;
    }

    private void OnDrawGizmos() 
    {
        if (center == null)
            return;
        Gizmos.DrawWireCube(center.position, new Vector3(x, 1, z));
    }

    public float GetSize() => (x + z) / 2;
}
