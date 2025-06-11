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
        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;

        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;

        if (animator != null && animator.runtimeAnimatorController != null && animator.gameObject.activeSelf)
            animator.SetBool("isRunning", isSprinting && moveDir.magnitude > 0.1f);
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
