using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerWeaponManager weaponManager;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] public GameObject armsModel;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip freakingZombie;

    [SerializeField] private float leanAngle;
    [SerializeField] private float leanSpeed = 5f;
    [SerializeField] private Transform cameraHolder; //used to tilt camera with lean

    private float currentLean = 0;
    private float targetLean = 0f;

    private bool hasPlayedPickup = false;

    // Update is called once per frame
    void Update()
    {
        playerMovement.HandleMove();//Updates Movement Handling 
        playerMovement.HandleSprint();// Updates Sprint handling
        playerMovement.HandleLanding();// updates landing handling
        weaponManager.HandleShooting();// updates shooting

        if (weaponManager.HasGun() && !hasPlayedPickup)
        {
            StartCoroutine(PlayPickupAndEnableArms());
        }

        if (Input.GetButtonDown("FireMode"))//Handles Firemode switch
        {
            weaponManager.ToggleFireMode();
        }
        weaponManager.HandleADS();// handles ads
        weaponManager.SetAiming(Input.GetButtonDown("Fire2"));// handles aiming

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

        Quaternion leanRotation = Quaternion.Euler(0f, 0f, currentLean);

        if (armsModel != null)
            armsModel.transform.localRotation = leanRotation;

        if (cameraHolder != null)
            cameraHolder.localRotation = leanRotation;
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
}
