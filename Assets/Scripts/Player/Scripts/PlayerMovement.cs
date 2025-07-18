using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] public CharacterController controller;
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintMultiplier;
    [SerializeField] int jumpMax;
    [SerializeField] float jumpForce;
    [SerializeField] float gravity;
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audioSteps;
    //[SerializeField] public float audioStepsVol;
    [SerializeField] AudioClip[] audioJump;
    //[SerializeField] public float audioJumpVol;
    [SerializeField] AudioClip[] audioLand;
    //[SerializeField] public float audioLandVol;
    [SerializeField] public Animator animator;

    [Header("Stamina Settings")]
    [SerializeField] public float maxStamina;
    [SerializeField] private float staminaDrainRate;
    [SerializeField] private float staminaRegenRate;
    [SerializeField] private float staminaRegenDelay;
    [SerializeField] private UnityEngine.UI.Slider staminaSlider;

    [Header("Crouch Settings")]
    [SerializeField] float crouchHeight = 1f;
    [SerializeField] float standHeight = 2f;
    [SerializeField] float crouchSpeedMultiplier = 0.5f;
    [SerializeField] Transform cameraTransform;
    [SerializeField] float crouchCameraY = 0.8f;
    [SerializeField] float standCameraY = 1.6f;

    private bool isCrouching = false;
    private float originalWalkSpeed;

    public float currentStamina;
    private float regenTimer;
    public bool canSprint => currentStamina > 0;

    private int currentJumpCount;
    private float currentAnimSpeed = 0f;
    Vector3 moveDir;
    Vector3 playerVel;
    bool isSprinting;
    bool isJumped;
    bool isPlayingStep;
    bool wasGrounded;

    private Vector2 inputMove;
    private bool jumpPressed;
    private bool sprintHeld;

    void Start()
    {
        originalWalkSpeed = walkSpeed;
    }

    public void HandleMove()//Movement
    {
        HandleJump();

        inputMove = PlayerController.inputActions.KBM.Move.ReadValue<Vector2>();
        moveDir = (inputMove.x * transform.right) +
                  (inputMove.y * transform.forward);

        float currentSpeed = isSprinting ? walkSpeed * sprintMultiplier : walkSpeed;
        controller.Move(moveDir * currentSpeed * Time.deltaTime);

        SetAnimations();

        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;

        if (controller.isGrounded && moveDir.magnitude > 0.3f && !isPlayingStep)
            StartCoroutine(PlaySteps());

        // NEW: Proper jump reset on landing
        if (controller.isGrounded && !wasGrounded)
        {
            currentJumpCount = 0;
            isJumped = false;

            if (audioLand != null && audioLand.Length > 0)
                aud.PlayOneShot(audioLand[Random.Range(0, audioLand.Length)]/*, audioLandVol*/);
        }

        wasGrounded = controller.isGrounded;
    }

    void SetAnimations()
    {
        if (animator != null && animator.runtimeAnimatorController != null && animator.gameObject.activeSelf)
        {
            var weaponManager = GetComponent<PlayerWeaponManager>();
            bool isKnifeEquipped = weaponManager != null &&
                                   weaponManager.CurrentGun == null &&
                                   weaponManager.HasGun(); // Check if holding  melee, plays a different animation

            animator.SetBool("IsKnife", isKnifeEquipped);

            float targetSpeed = 0f;

            if (moveDir.magnitude > 0.1f)
                targetSpeed = isSprinting ? 1f : 0.5f;

            // Smoothly transition to the target speed
            currentAnimSpeed = Mathf.MoveTowards(currentAnimSpeed, targetSpeed, Time.deltaTime * 1f);

            animator.SetFloat("Speed", currentAnimSpeed);

            animator.SetFloat("AnimFreeze", controller.isGrounded ? 1f : 0f);
        }
    }

    public void HandleSprint()//Sprint
    {
        if (isCrouching)// This flag checks if the player is crouching// If crouching forces isprinting to false; this will allow the player to hold sprint  button and still fire
        {
            isSprinting = false;
            return;
        }

        sprintHeld = PlayerController.inputActions.KBM.Sprint.ReadValue<float>() > 0.1f;
        bool sprintInput = sprintHeld && moveDir.magnitude > 0.1f;

        if (sprintInput && canSprint)
        {
            isSprinting = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            regenTimer = 0f;
            currentStamina = Mathf.Max(currentStamina, 0f); // Clamp to 0
        }
        else
        {
            isSprinting = false;
            regenTimer += Time.deltaTime;

            if (regenTimer >= staminaRegenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina); // Clamp to max
            }
        }

        SetAnimations();

        if (staminaSlider != null)
            staminaSlider.value = currentStamina;
    }

    public void HandleJump()//Jump
    {
        jumpPressed = PlayerController.inputActions.KBM.Jump.triggered;

        if (jumpPressed && currentJumpCount < jumpMax)
        {
            playerVel.y = jumpForce;
            isJumped = true;
            currentJumpCount++;

            if (audioJump != null && audioJump.Length > 0)
                aud.PlayOneShot(audioJump[Random.Range(0, audioJump.Length)]/*, audioJumpVol*/);
        }
    }

    public void HandleLanding()//Landing
    {
        if (isJumped && controller.isGrounded && audioLand.Length > 0)
            aud.PlayOneShot(audioLand[Random.Range(0, audioLand.Length)]/*, audioLandVol*/);

        if (controller.isGrounded)
            isJumped = false;
    }

    IEnumerator PlaySteps()//Steps sfx
    {
        isPlayingStep = true;
        aud.PlayOneShot(audioSteps[Random.Range(0, audioSteps.Length)]/*, audioStepsVol*/);
        if (!isSprinting)
            yield return new WaitForSeconds(0.5f);
        else
            yield return new WaitForSeconds(0.3f);
        isPlayingStep = false;
    }

    public void HandleCrouch()//Toggle crouch on key press
    {
        if (PlayerController.inputActions.KBM.Crouch.triggered)
        {
            isCrouching = !isCrouching;

            controller.height = isCrouching ? crouchHeight : standHeight;

            if (cameraTransform != null)
            {
                Vector3 camPos = cameraTransform.localPosition;
                camPos.y = isCrouching ? crouchCameraY : standCameraY;
                cameraTransform.localPosition = camPos;
            }

            walkSpeed = isCrouching ? originalWalkSpeed * crouchSpeedMultiplier : originalWalkSpeed;
        }
    }

    public bool IsActuallySprinting()
    {
        return isSprinting && !isCrouching;
    }
}
