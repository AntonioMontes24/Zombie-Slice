using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float mouseSensitivity = 3.5f;
    public float lockVertMin = -90f;
    public float lockVertMax = 90f;
    public bool invertY = false;
    public Transform playerBody;

    [SerializeField] private Transform pitchTarget;

    //rotate on X axis looks up and down on Y axis, weird thing but REMEMBER THIS!!!
    float rotX = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 3.5f);
        MouseSensController.OnSensChanged += UpdateSensitivity;

    }

    // Update is called once per frame
    void Update()
    {
        // get input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // give option to invert mouse look up and down
        if (invertY)
            rotX += mouseY;
        else
            rotX -= mouseY;

        // clamp camera on the x-axis 
        //rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);

        rotX = Mathf.Clamp(rotX, -90f, 90f);

        // rotate camera on x-axis to look up and down
        // we now rotate pitchTarget (CameraHolder) instead of this object directly
        if (pitchTarget != null)
            pitchTarget.localRotation = Quaternion.Euler(rotX, 0, 0);

        transform.localRotation = Quaternion.Euler(rotX, 0f, 0f);
        // rotate player on y-axis to look left and right
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void UpdateSensitivity(float newSens)
    {
        mouseSensitivity = newSens;
        Debug.Log($"new sens: {mouseSensitivity}");
    }

}