using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public CharacterController controller;
    public float walkSpeed;
    public float sprintMultiplier;
    public int jumpMax;
    public float jumpForce;
    public float gravity;
    public AudioSource aud;
    public AudioClip[] audioSteps;
    [Range(0, 1)] public float audioStepsVol = 0.9f;
    public AudioClip[] audioJump;
    [Range(0, 1)] public float audioJumpVol = 0.9f;
    public AudioClip[] audioLand;
    [Range(0, 1)] public float audioLandVol = 0.9f;

    Vector3 moveDir;
    Vector3 playerVel;
    bool isSprinting;
    bool isJumped;
    bool isPlayingStep;
    bool wasGrounded;

    public void HandleMove()
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


        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;

        if (controller.isGrounded && moveDir.magnitude > 0.3f && !isPlayingStep)
            StartCoroutine(PlaySteps());
    }

    public void HandleSprint()
    {
        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;
        else if (Input.GetButtonUp("Sprint"))
            isSprinting = false;
    }

    public void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && wasGrounded)
        {
            playerVel.y = jumpForce;
            isJumped = true;

            if (audioJump != null && audioJump.Length > 0)
                aud.PlayOneShot(audioJump[Random.Range(0, audioJump.Length)], audioJumpVol);
        }
    }

    public void HandleLanding()
    {
        if (isJumped && controller.isGrounded && audioLand.Length > 0)
            aud.PlayOneShot(audioLand[Random.Range(0, audioLand.Length)], audioLandVol);

        if (controller.isGrounded)
            isJumped = false;
    }

    IEnumerator PlaySteps()
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
