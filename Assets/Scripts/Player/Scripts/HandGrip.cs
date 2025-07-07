using UnityEngine;

public class FingerCurl : MonoBehaviour
{
    [Header("Right Hand Finger Bones")]
    public Transform[] indexFingers;
    public Transform[] middleFingers;
    public Transform[] ringFingers;
    public Transform[] pinkyFingers;
    public Transform[] thumbFingers;

    [Range(0f, 90f)]
    public float curlAngle = 50f;

    public bool curl = false;

    void Update()
    {
        if (curl)
            CurlHand();
    }

    public void CurlHand()
    {
        RotateFingers(indexFingers);
        RotateFingers(middleFingers);
        RotateFingers(ringFingers);
        RotateFingers(pinkyFingers);
        RotateFingers(thumbFingers);
    }

    void RotateFingers(Transform[] bones)
    {
        foreach (var bone in bones)
        {
            if (bone != null)
                bone.localRotation = Quaternion.Euler(curlAngle, 90, 90);
        }
    }
}

