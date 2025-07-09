using UnityEngine;

public class RespawnReset : MonoBehaviour
{
    Vector3 pos;
    void Start()
    {
        pos = transform.position;
        GameManager.instance.respawnHook += ResetPosition;
    }

    private void OnDestroy()
    {
        GameManager.instance.respawnHook -= ResetPosition;
    }

    void ResetPosition()
    {
        transform.position = pos;
    }
}
