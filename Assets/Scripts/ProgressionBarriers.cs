using UnityEngine;

public class ProgressionBarriers : MonoBehaviour
{
    [SerializeField] private int killsNeeded;

    private bool barrierRemoved = false;

    private Renderer barrierRenderer;
    private Collider barrierCollider;

    private void Awake()
    {
        barrierRenderer = GetComponent<Renderer>();
        barrierCollider = GetComponent<Collider>();

        if (barrierRenderer == null || barrierCollider == null)
        {
            Debug.LogWarning("Renderer or Collider component missing on barrier: " + gameObject.name);
        }
    }

    void Update()
    {
        if (!barrierRemoved && KillManager.instance != null && KillManager.instance.KillCount >= killsNeeded)
        {
            RemoveBarrier();
        }
    }

    private void RemoveBarrier()
    {
        barrierRemoved = true;
        if (barrierRenderer != null) barrierRenderer.enabled = false;
        if (barrierCollider != null) barrierCollider.enabled = false;

        Debug.Log("Barrier removed (disabled) after " + killsNeeded + " kills.");
    }

    public void ResetBarrier()
    {
        barrierRemoved = false;
        if (barrierRenderer != null) barrierRenderer.enabled = true;
        if (barrierCollider != null) barrierCollider.enabled = true;

        Debug.Log("Barrier reset (enabled).");
    }
}
