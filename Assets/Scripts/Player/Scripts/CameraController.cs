using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float mouseSensitivity = 1.0f;
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
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", .5f);
        MouseSensController.OnSensChanged += UpdateSensitivity;
    }

    // Update is called once per frame
    void Update()
    {
        HandleFreeLookInput();
        lookInput = PlayerController.inputActions.Input.Look.ReadValue<Vector2>();

        bool isGamepad = Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame;

        float mouseX, mouseY;

        if (isGamepad)
        {
            float sens = PlayerPrefs.GetFloat("ControllerSensitivity", controllerSensitivity);
            mouseX = lookInput.x * sens * Time.deltaTime;
            mouseY = lookInput.y * sens * Time.deltaTime;
        }
        else
        {
            Vector2 rawMouse = Mouse.current.delta.ReadValue();
            float sens = PlayerPrefs.GetFloat("MouseSensitivity", mouseSensitivity);

            const float mouseScale = 0.01f;

            mouseX = rawMouse.x * sens * mouseScale;
            mouseY = rawMouse.y * sens * mouseScale;

            Debug.Log($"Raw Mouse Input: {rawMouse}, Scaled: ({mouseX}, {mouseY}), Sens: {sens}");
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
