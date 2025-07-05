using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] public CharacterController controller;
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintMultiplier;

    [Header("Audio and Animations")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audioSteps;
    //[SerializeField] public float audioStepsVol;
    [SerializeField] AudioClip[] audioJump;
    //[SerializeField] public float audioJumpVol;
    [SerializeField] AudioClip[] audioLand;
    //[SerializeField] public float audioLandVol;
    [SerializeField] public Animator animator;


    [Header("JumpSettings")]
    [SerializeField] int jumpMax;
    [SerializeField] float jumpForce;
    [SerializeField] float gravity;
    [SerializeField] float fallMultiplier;
    [SerializeField] float lowMultiplier;

    [Header("Stamina Settings")]
    [SerializeField] public float maxStamina;
    [SerializeField] private float staminaDrainRate;
    [SerializeField] private float staminaRegenRate;
    [SerializeField] private float staminaRegenDelay;
    [SerializeField] private UnityEngine.UI.Slider staminaSlider;

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

    public void HandleMove()//Movement
    {
        HandleJump();


        if (controller.isGrounded && playerVel.y < 0)
        {
            isJumped = false;
            playerVel.y = 0f;
            currentJumpCount = 0;
        }


        moveDir = (Input.GetAxis("Horizontal") * transform.right) +
                  (Input.GetAxis("Vertical") * transform.forward);
        float currentSpeed = isSprinting ? walkSpeed * sprintMultiplier : walkSpeed;
        controller.Move(moveDir * currentSpeed * Time.deltaTime);

        SetAnimations();

        if (playerVel.y < 0)
        {
            playerVel.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else if (playerVel.y > 0 && !Input.GetButton("Jump"))
        {
            playerVel.y += gravity * lowMultiplier * Time.deltaTime;
        }
        else
        {
            playerVel.y += gravity * Time.deltaTime;
        }
        controller.Move(playerVel * Time.deltaTime);
        if (controller.isGrounded && moveDir.magnitude > 0.3f && !isPlayingStep)
            StartCoroutine(PlaySteps());
    }

    void SetAnimations()
    {
        if (animator != null && animator.runtimeAnimatorController != null && animator.gameObject.activeSelf)
        {
            // Freeze all animation playback if not grounded
            if (!controller.isGrounded)
            {
                animator.speed = 0f;
                return; // Exit early — no need to update speed or parameters
            }

            animator.speed = 1f; // Resume animations if grounded

            float targetSpeed = 0f;
            if (moveDir.magnitude > 0.1f)
                targetSpeed = isSprinting ? 1f : 0.5f;

            // Smooth speed blending
            currentAnimSpeed = Mathf.MoveTowards(currentAnimSpeed, targetSpeed, Time.deltaTime * 5f);
            animator.SetFloat("Speed", currentAnimSpeed);
        }
    }

    public void HandleSprint()//Sprint
    {
        bool sprintInput = Input.GetButton("Sprint") && moveDir.magnitude > 0.1f;

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
        if (Input.GetButtonDown("Jump") && currentJumpCount < jumpMax)
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
}
