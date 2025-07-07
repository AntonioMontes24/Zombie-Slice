using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AimDownSights : MonoBehaviour
{
    [Header("FOV (field‑of‑view) settings")]
    [SerializeField] float hipFov = 60f;
    [SerializeField] float adsFov = 35f;
    [SerializeField] float zoomSpeed = 8f;

    [Header("Controls")]
    [SerializeField] KeyCode aimButton = KeyCode.Mouse1;

    Camera cam;
    float targetFov;
    PlayerWeaponManager weaponManager;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.fieldOfView = hipFov;
        targetFov = hipFov;

        weaponManager = FindObjectOfType<PlayerWeaponManager>();
    }

    void Update()
    {
        bool isAiming = Input.GetKey(aimButton);
        targetFov = isAiming ? adsFov : hipFov;

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);

        weaponManager?.SetAiming(isAiming);
    }
}
