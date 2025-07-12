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

        //give option to invert mouse look up and down
        rotX += invertY ? mouseY : -mouseY;
        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);// clamp camera on the x axis 

        if (pitchTarget != null)
            pitchTarget.localRotation = Quaternion.Euler(rotX, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void UpdateSensitivity(float newSens)
    {
        mouseSensitivity = newSens;
        Debug.Log($"new sens: {mouseSensitivity}");
    }

}