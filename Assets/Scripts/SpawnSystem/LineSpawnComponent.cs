using UnityEngine;

public class LineSpawnComponent : MonoBehaviour, ISpawnComponent
{
    [SerializeField]
    Transform start, end;
    public Vector3 GetSpawnPosition() => Vector3.Lerp(start.position, end.position, Random.Range(0f, 1f));
    private void OnDrawGizmos() 
    {
        if (start == null || end == null)
            return;
        Gizmos.DrawLine(start.position, end.position);
    }
    public float GetSize() => (start.position - end.position).magnitude;
}
