using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float mouseSensitivity = 3.5f;
    public float controllerSensitivity = 200f;
    public float lockVertMin = -90f;
    public float lockVertMax = 90f;
    public bool invertY = false;
    public Transform playerBody;

    [SerializeField] private Transform pitchTarget;

    //rotate on X axis looks up and down on Y axis, weird thing but REMEMBER THIS!!!
    float rotX = 0f;
    float freeLookYaw = 0f;
    private float freeLookClamp = 12.5f;

    private bool isFreeLooking;

    private Vector2 lookInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 3.5f);
        controllerSensitivity = PlayerPrefs.GetFloat("ControllerSensitivity", 200f);
        MouseSensController.OnSensChanged += UpdateSensitivity;
    }

    // Update is called once per frame
    void Update()
    {
        HandleFreeLookInput();

        float mouseX = 0f, mouseY = 0f;

        // Check device type
        var lastDevice = PlayerController.inputActions.Input.Look.activeControl?.device;

        if (lastDevice is Mouse)
        {
            Vector2 look = PlayerController.inputActions.Input.Look.ReadValue<Vector2>();
            float scaledSensitivity = mouseSensitivity * 0.2f;
            mouseX = look.x * scaledSensitivity * Time.deltaTime;
            mouseY = look.y * scaledSensitivity * Time.deltaTime;
        }
        else if (lastDevice is Gamepad)
        {
            Vector2 look = PlayerController.inputActions.Input.Look.ReadValue<Vector2>();
            float controllerSens = controllerSensitivity * 0.2f;
            mouseX = look.x * controllerSens * Time.deltaTime;
            mouseY = look.y * controllerSens * Time.deltaTime;
        }

        rotX += invertY ? mouseY : -mouseY;
        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);// clamp camera on the x axis

        if (pitchTarget != null)
            pitchTarget.localRotation = Quaternion.Euler(rotX, 0f, 0f);

        if (isFreeLooking)
        {
            // Rotate camera side-to-side, independent of player body
            freeLookYaw += mouseX;
            freeLookYaw = Mathf.Clamp(freeLookYaw, -freeLookClamp, freeLookClamp);
            transform.localRotation = Quaternion.Euler(0f, freeLookYaw, 0f);
        }
        else
        {
            // Rotate the player body as normal
            playerBody.Rotate(Vector3.up * mouseX);
            // Reset freelook rotation
            freeLookYaw = 0f;
            transform.localRotation = Quaternion.identity;
        }
    }

    void HandleFreeLookInput()
    {
        isFreeLooking = PlayerController.inputActions.Input.FreeLook.IsPressed();
    }

    private void UpdateSensitivity(float newSens)
    {
        mouseSensitivity = newSens;
        Debug.Log($"new sens: {mouseSensitivity}");
    }
}