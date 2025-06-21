using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] CharacterController controller;
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintMultiplier;
    [SerializeField] int jumpMax;
    [SerializeField] float jumpForce;
    [SerializeField] float gravity;
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audioSteps;
    [SerializeField] public float audioStepsVol;
    [SerializeField] AudioClip[] audioJump;
    [SerializeField] public float audioJumpVol;
    [SerializeField] AudioClip[] audioLand;
    [SerializeField] public float audioLandVol;
    [SerializeField] public Animator animator;

    [Header("Stamina Settings")]
    [SerializeField] public float maxStamina;
    [SerializeField] private float staminaDrainRate;
    [SerializeField] private float staminaRegenRate;
    [SerializeField] private float staminaRegenDelay;
    [SerializeField] private UnityEngine.UI.Slider staminaSlider;

    public float currentStamina;
    private float regenTimer;
    public bool canSprint => currentStamina > 0;


    Vector3 moveDir;
    Vector3 playerVel;
    bool isSprinting;
    bool isJumped;
    bool isPlayingStep;
    bool wasGrounded;

    public void HandleMove()//Movement
    {
        HandleJump();
        wasGrounded = controller.isGrounded;

        if (controller.isGrounded)
        {
            isJumped = false;
            playerVel = Vector3.zero;
        }

        moveDir = (Input.GetAxis("Horizontal") * transform.right) +
                  (Input.GetAxis("Vertical") * transform.forward);
        float currentSpeed = isSprinting ? walkSpeed * sprintMultiplier : walkSpeed;
        controller.Move(moveDir * currentSpeed * Time.deltaTime);

        if (animator != null && animator.runtimeAnimatorController != null && animator.gameObject.activeSelf)
            animator.SetBool("isWalking", moveDir.magnitude > 0.1f);


        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;

        if (controller.isGrounded && moveDir.magnitude > 0.3f && !isPlayingStep)
            StartCoroutine(PlaySteps());
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

        if (animator != null && animator.runtimeAnimatorController != null && animator.gameObject.activeSelf)
            animator.SetBool("isRunning", isSprinting && moveDir.magnitude > 0.1f);

        if (staminaSlider != null)
            staminaSlider.value = currentStamina;
    }


    public void HandleJump()//Jump
    {
        if (Input.GetButtonDown("Jump") && wasGrounded)
        {
            playerVel.y = jumpForce;
            isJumped = true;

            if (audioJump != null && audioJump.Length > 0)
                aud.PlayOneShot(audioJump[Random.Range(0, audioJump.Length)], audioJumpVol);
        }
    }

    public void HandleLanding()//Landing
    {
        if (isJumped && controller.isGrounded && audioLand.Length > 0)
            aud.PlayOneShot(audioLand[Random.Range(0, audioLand.Length)], audioLandVol);

        if (controller.isGrounded)
            isJumped = false;
    }

    IEnumerator PlaySteps()//Steps sfx
    {
        isPlayingStep = true;
        aud.PlayOneShot(audioSteps[Random.Range(0, audioSteps.Length)], audioStepsVol);
        if (!isSprinting)
            yield return new WaitForSeconds(0.5f);
        else
            yield return new WaitForSeconds(0.3f);
        isPlayingStep = false;
    }
}
