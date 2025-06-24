using UnityEngine;

public class CheckpointVisuals : MonoBehaviour
{
    [SerializeField] Renderer[] x, z;

    public void FixScale()
    {
        foreach (Renderer renderer in x)
            if (renderer != null)
                renderer.material.mainTextureScale = new Vector2(transform.localScale.x, 1);
        foreach (Renderer renderer in z)
            if (renderer != null)
                renderer.material.mainTextureScale = new Vector2(transform.localScale.z, 1);
    }

    void Start() => FixScale();
}
