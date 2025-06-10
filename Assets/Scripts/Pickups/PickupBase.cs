using UnityEngine;

public abstract class PickupBase : MonoBehaviour
{
    const float wiggleSpeed = 1.75f;
    const float wiggleIntensity = .1f;
    Vector3 startPos;

    protected virtual void Start()
    {
        startPos = transform.localPosition;
    }

    protected virtual void Update()
    {
        transform.localPosition = startPos + new Vector3(0, Mathf.Sin(Time.realtimeSinceStartup * wiggleSpeed) * wiggleIntensity, 0);
    }
}
