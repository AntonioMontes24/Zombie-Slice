using UnityEngine;

public abstract class PickupBase : MonoBehaviour
{
    const float wiggleSpeed = 1.75f;
    const float wiggleIntensity = .1f;
    Vector3 startPos;
    float offset;
    protected virtual void Start()
    {
        startPos = transform.localPosition;
        offset = Random.Range(0, 2 * Mathf.PI);
    }

    protected virtual void Update()
    {
        transform.localPosition = startPos + new Vector3(0, Mathf.Sin(Time.realtimeSinceStartup * wiggleSpeed + offset) * wiggleIntensity, 0);
    }
}
