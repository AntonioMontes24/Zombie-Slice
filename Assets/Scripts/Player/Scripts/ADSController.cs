using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class AimDownSights : MonoBehaviour
{
    [Header("FOV (field‑of‑view) settings")]
    [SerializeField] float hipFov = 60f;
    [SerializeField] float adsFov = 35f;
    [SerializeField] float zoomSpeed = 8f;

    [Header("Controls")]


    Camera cam;
    float targetFov;
    PlayerWeaponManager weaponManager;
    private PlayerInputActions inputActions;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.fieldOfView = hipFov;
        targetFov = hipFov;

        weaponManager = Object.FindFirstObjectByType<PlayerWeaponManager>();
        inputActions = new PlayerInputActions();
        inputActions.KBM.Enable();
    }

    void Update()
    {
        bool isAiming = PlayerController.inputActions.KBM.Aim.ReadValue<float>() > 0.1f;
        targetFov = isAiming ? adsFov : hipFov;

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);

        weaponManager?.SetAiming(isAiming);
    }
}