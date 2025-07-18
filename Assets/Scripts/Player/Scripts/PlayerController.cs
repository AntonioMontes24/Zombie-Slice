using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IOpen//Added open interface for crypt door
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerWeaponManager weaponManager;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] public GameObject armsModel;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip weaponSwap;
    [SerializeField] private Transform leftArm;

    [SerializeField] private float leanAngle;
    [SerializeField] private float leanSpeed = 5f;
    [SerializeField] private Transform cameraHolder; //used to tilt camera with lean
    [SerializeField] private Transform leanRoot;

    private float currentLean = 0f;
    private float targetLean = 0f;

    private bool hasPlayedPickup = false;

    // In PlayerController.cs
    public static PlayerInputActions inputActions { get; private set; }
    private float lastCycleInput;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Input.Enable();
    }

    private void OnDisable()
    {
        inputActions.Input.Disable();
    }



    private void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        playerMovement.controller.transform.position = GameManager.instance.playerSpawnPoint;
        playerHealth.ResetHealth();
    }

    // Update is called once per frame
    void Update()
    {
        playerMovement.HandleMove();//Updates Movement Handling 
        playerMovement.HandleSprint();// Updates Sprint handling
        playerMovement.HandleLanding();// updates landing handling
        playerMovement.HandleJump();// Updates Jump handling
        weaponManager.HandleMeleeAttack();// Updates melee attacks
        playerMovement.HandleCrouch();

        if (!GameManager.instance.isPaused)// If game is not paused player can shoot else player cannot shoot 
        {
            weaponManager.HandleShooting();// updates shooting
        }

        HandleWeaponSwitch();

        if (!hasPlayedPickup && weaponManager.HasGun())
        {
            bool isOneHanded = weaponManager.CurrentGun?.isOneHanded ?? true;
            StartCoroutine(PlayPickupAnimation(isOneHanded));
        }

        if (inputActions.Input.FireMode.triggered)//Handles Firemode switch
        {
            weaponManager.ToggleFireMode();
        }
        weaponManager.SetAiming(inputActions.Input.ADS.ReadValue<float>() > 0.1f);// handles aiming
        weaponManager.HandleADS();// handles ads

        HandleLean();
    }

    private void HandleLean()// Handles L & R leans
    {
        if (inputActions.Input.LeanLeft.ReadValue<float>() > 0.1f)
            targetLean = leanAngle;
        else if (inputActions.Input.LeanRight.ReadValue<float>() > 0.1f)
            targetLean = -leanAngle;
        else
            targetLean = 0f;

        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);

        Quaternion leanRot = Quaternion.Euler(0f, 0f, currentLean);
        if (leanRoot != null)
            leanRoot.localRotation = leanRot;
    }

    public IEnumerator PlayPickupAnimation(bool isOneHanded) // Handles Weapon Pick up animation
    {
        hasPlayedPickup = true;
        PlayGunTakeOutAnimation();

        if (weaponSwap != null && audioSource != null)
            audioSource.PlayOneShot(weaponSwap/*, 0.8f*/);

        yield return new WaitForSeconds(0.1f);
        EnableHands();

        yield return new WaitForSeconds(0.05f);
        UpdateOneHandedWeaponArms(isOneHanded);
    }
    private void EnableHands()// Enables hands if a weapon is equipped
    {
        if (armsModel != null)
        {
            armsModel.SetActive(true);

            if (weaponManager != null && weaponManager.HasGun())
            {
                var gun = weaponManager.CurrentGun;
                if (gun != null)
                {
                    Debug.Log("EnableHands: Checking for one-handed weapon");
                    UpdateOneHandedWeaponArms(gun.isOneHanded);
                }
            }
        }
    }

    public void UpdateOneHandedWeaponArms(bool isOneHanded)// Disables one arm for one handed guns and melee weapons
    {
        Debug.Log("One Handed Function called" + isOneHanded);
        if (leftArm != null)
        {
            leftArm.localScale = isOneHanded ? Vector3.zero : Vector3.one;
        }
    }

    public void PlayGunTakeOutAnimation()//Plays switch animation everytime a weapon is picked up or switched
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.ResetTrigger("GunTakeOut");
            animator.SetTrigger("GunTakeOut");
        }
        StartCoroutine(BlockFireDuringGunSwitch(0.7f));
    }

    public IEnumerator BlockFireDuringGunSwitch(float duration) // Disables the ability to fire during weapon change
    {
        weaponManager.SetCanFire(false);
        yield return new WaitForSeconds(duration);
        weaponManager.SetCanFire(true);
    }

    void HandleWeaponSwitch()// Handles weapon selection using mouse scroll and numbers
    {
        // Number key selection
        if (inputActions.Input.SwitchWeapon.triggered)
            weaponManager.TryEquipWeapon(0);

        if (inputActions.Input.SwitchWeapon2.triggered)
            weaponManager.TryEquipWeapon(1);

        if (inputActions.Input.SwitchWeapon3.triggered)
            weaponManager.TryEquipWeapon(2);
        if(inputActions.Input.SwitchWeapon4.triggered)
            weaponManager.TryEquipWeapon(3);

        if (inputActions.Input.SwitchWeaponLeft.triggered)
            weaponManager.ScrollWeapon(-1);

        if (inputActions.Input.SwitchWeaponRight.triggered)
            weaponManager.ScrollWeapon(1);

        // Mouse scroll selection
        float scroll = inputActions.Input.ScrollWheel.ReadValue<Vector2>().y;

        if (scroll > 0f)
            weaponManager.ScrollWeapon(1); // scroll up
        else if (scroll < 0f)
            weaponManager.ScrollWeapon(-1); // scroll down
    }
}
