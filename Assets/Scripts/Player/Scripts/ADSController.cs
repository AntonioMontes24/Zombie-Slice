using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class AimDownSights : MonoBehaviour
{
    [Header("FOV (field‑of‑view) settings")]
    [SerializeField] float hipFov = 60f;
    [SerializeField] float adsFov = 35f;
    [SerializeField] float zoomSpeed = 8f;

    Camera cam;
    float targetFov;
    PlayerWeaponManager weaponManager;
    private PlayerInputActions inputActions => PlayerController.inputActions;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.fieldOfView = hipFov;
        targetFov = hipFov;

        weaponManager = Object.FindFirstObjectByType<PlayerWeaponManager>();
    }

    void Update()
    {
        if (weaponManager == null) return;

        bool isGun = weaponManager.CurrentGun is FireArmStats;
        bool adsPressed = inputActions.Input.ADS.IsPressed();

        bool isAiming = isGun && adsPressed;
        targetFov = isAiming ? adsFov : hipFov;

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);

        weaponManager.SetAiming(isAiming);
    }
}
