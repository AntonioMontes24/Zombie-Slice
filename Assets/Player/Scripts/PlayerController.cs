using System.Collections;
//using Unity.VisualScripting;
using UnityEngine;

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

    private void Start()
    {
        SpawnPlayer();
    }
    //void Start() => SpawnPlayer();

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
        playerMovement.HandleJump();
        if (!GameManager.instance.isPaused)
        {
        weaponManager.HandleShooting();// updates shooting

        }
        HandleWeaponSwitch();

        if (weaponManager.HasGun())
        {
            var currentGun = weaponManager.CurrentGun;

            if (!hasPlayedPickup)
            {
                bool isOneHanded = currentGun.isOneHanded;
                StartCoroutine(PlayPickupAnimation(isOneHanded));
            }
        }

        if (Input.GetButtonDown("FireMode"))//Handles Firemode switch
        {
            weaponManager.ToggleFireMode();
        }
        weaponManager.HandleADS();// handles ads
        weaponManager.SetAiming(Input.GetButton("Fire2"));// handles aiming

        HandleLean();
    }

    private void HandleLean()
    {
        if (Input.GetKey(KeyCode.Q))
            targetLean = leanAngle;
        else if (Input.GetKey(KeyCode.E))
            targetLean = -leanAngle;
        else
            targetLean = 0f;

        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);

        Quaternion leanRot = Quaternion.Euler(0f, 0f, currentLean);
        if (leanRoot != null)
            leanRoot.localRotation = leanRot;
    }

    public IEnumerator PlayPickupAnimation(bool isOneHanded)
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
    private void EnableHands()
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


    public void UpdateOneHandedWeaponArms(bool isOneHanded)
    {
        Debug.Log("One Handed Function called" + isOneHanded);
        if (leftArm != null)
        {
            Debug.LogWarning("Left Arm Found. Resizing");
            leftArm.localScale = isOneHanded ? Vector3.zero : Vector3.one;

        }
        else
        {
            Debug.LogWarning("L Arm Reference is NULL");
        }
    }

    public void PlayGunTakeOutAnimation()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.ResetTrigger("GunTakeOut");
            animator.SetTrigger("GunTakeOut");
        }
        StartCoroutine(BlockFireDuringGunSwitch(0.7f));
    }

    public IEnumerator BlockFireDuringGunSwitch(float duration )
    {
        weaponManager.SetCanFire(false);
        yield return new WaitForSeconds(duration);
        weaponManager.SetCanFire(true);
    }

    void HandleWeaponSwitch()
    {
        // Number key selection
        if (Input.GetKeyDown(KeyCode.Alpha1)) weaponManager.TryEquipWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) weaponManager.TryEquipWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) weaponManager.TryEquipWeapon(2);

        // Mouse scroll selection
        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0f)
            weaponManager.ScrollWeapon(1); // scroll up
        else if (scroll < 0f)
            weaponManager.ScrollWeapon(-1); // scroll down
    }

}
