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
    [SerializeField] private AudioClip freakingZombie;
    [SerializeField] private Transform leftArm;

    [SerializeField] private float leanAngle;
    [SerializeField] private float leanSpeed = 5f;
    [SerializeField] private Transform cameraHolder; //used to tilt camera with lean
    [SerializeField] private Transform leanRoot;

    private float currentLean = 0f;
    private float targetLean = 0f;

    private bool hasPlayedPickup = false;


    void Start() => SpawnPlayer();

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
        if (!GameManager.instance.isPaused)
        {
        weaponManager.HandleShooting();// updates shooting

        }
        HandleWeaponSwitch();

        if (weaponManager.HasGun())
        {
            var currentGun = weaponManager.CurrentGun;

            bool isPistol = currentGun.isPistol;
            Debug.Log("Is Pistol: " + isPistol);

            if (leftArm != null)
                leftArm.localScale = isPistol ? Vector3.zero : Vector3.one;

            if (!hasPlayedPickup)
            {
                StartCoroutine(PlayPickupAndEnableArms());
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

    private IEnumerator PlayPickupAndEnableArms()
    {
        hasPlayedPickup = true;
        if (animator != null && animator.runtimeAnimatorController != null && animator.gameObject.activeSelf)
            animator.SetBool("HasGun", true);
        Debug.Log("Setting Has Gun = true");

        if (freakingZombie != null && audioSource != null)
            audioSource.PlayOneShot(freakingZombie, 0.8f);

        yield return new WaitForSeconds(0.1f);
        if (armsModel != null)
        {
            armsModel.SetActive(true);
        }
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
