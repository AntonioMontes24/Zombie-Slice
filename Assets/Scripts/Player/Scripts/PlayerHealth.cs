using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamage
{
    [Header("Player Health")]
    [SerializeField] int maxHealth;
    [SerializeField] int currentHealth;

    [Header("VFX")]
    public GameObject deathEffect;//If we want like a bloody screen or something
    public AudioClip hurtSound;//hurt sfx   
    public AudioClip deathSound;// death sfx

    [Header("UI Low Health Flashing")]
    [SerializeField] Animator lowHealthFlashAnimator;
    [Range(0f, 1f)]
    [SerializeField] float lowHealthThreashHold = 0.25f; // 0.25 for 25% threshold


    [Header("Death Camera Follow")]
    [SerializeField] private Transform headBone;
    [SerializeField] private Camera deathCamera;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 0.2f, 0);
    [SerializeField] private float followSpeed = 5f;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Bleed Effect")]
    private Coroutine bleedCoroutine;
    private int activeBleedDamagePerTick;
    private float activeBleedTickInterval;

    private bool isFlashingLowHealth = false;

    Coroutine damageSoundRoutine;

    bool playedHurtSound;
    bool isTakingDotDamage;
    public bool hasDied;


    private AudioSource audioSource;

    private void Start()
    {
        currentHealth = maxHealth;
        updatePlayerUI();

        audioSource = GetComponent<AudioSource>();

        if(lowHealthFlashAnimator != null)
        {
            lowHealthFlashAnimator.SetBool("IsLowHealth", false);
            isFlashingLowHealth = false;
        }
    }

    public void takeDamage(int amount)// handles damage take
    {
        if (hasDied) return;

        // flash the damage on the screen
        StartCoroutine(damageFlash());

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0); // ensure health doesnt go below 0
        updatePlayerUI();

        if (currentHealth <= 0)
        {
            if (!hasDied)
            {
                hasDied = true;
                if (deathSound && audioSource)
                    audioSource.PlayOneShot(deathSound);
                Die();
                StartCoroutine(HandleDeathSequence());
                RemoveBleed();
                if (damageSoundRoutine != null)
                    StopCoroutine(damageSoundRoutine);

                if(lowHealthFlashAnimator != null)
                {
                    lowHealthFlashAnimator.SetBool("IsLowHealth", false);
                    isFlashingLowHealth = false;
                }
            }
        }
        else
        {
            if (hurtSound && audioSource)
            {
                audioSource.PlayOneShot(hurtSound);
                // isTakingDotDamage = true;
                // damageSoundRoutine = StartCoroutine(LoopHurtSound());
            }
        }

        Debug.Log("PlayerHealth: " + currentHealth);
    }

    public void Heal(int amount)//Handles healing waiting on health pick up to test
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        updatePlayerUI();
        StartCoroutine(HealFlash());

        // stop low health flashing if healed above threshold
        if(((float)currentHealth / maxHealth) > lowHealthThreashHold && isFlashingLowHealth)
        {
            if(lowHealthFlashAnimator != null)
            {
                lowHealthFlashAnimator.SetBool("IsLowHealth", false);
                isFlashingLowHealth = false;
            }
        }

    }

    public int CurrentHealth // saving health for player state
    {
        get => currentHealth;
        set
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
            updatePlayerUI();
        }
    }

    public void SetHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        updatePlayerUI();
    }


    // only call this when resetting gamestate
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        updatePlayerUI();
        hasDied = false;
        RemoveBleed();

        if(lowHealthFlashAnimator != null)
        {
            lowHealthFlashAnimator.SetBool("IsLowHealth", false);
            isFlashingLowHealth = false;
        }

        CancelHurtLoop();

    }

    public bool CanHeal()
    {
        return maxHealth != currentHealth;
    }

    void Die()//Handles death/VFX/SFX//Disables attacks and shooting
    {
        if (deathEffect)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3f))
            {
                Vector3 floorPos = hit.point + Vector3.up * 0.01f;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                GameObject effect = Instantiate(deathEffect, floorPos, rotation);
                effect.transform.localScale = Vector3.one * 5f;
            }
        }

        if (animator != null)
            animator.SetTrigger("IsDead");

        PlayerWeaponManager weaponManager = GetComponent<PlayerWeaponManager>();
        if (weaponManager != null)
            weaponManager.SetCanFire(false);

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
            controller.enabled = false;

        CameraController cameraController = GetComponentInChildren<CameraController>();
        if (cameraController != null)
            cameraController.enabled = false;

        if (GameManager.instance.flashDamageScreen != null)
        {
            GameManager.instance.flashDamageScreen.SetActive(true);
        }
        EnableDeathCamera();
    }


    IEnumerator LoopHurtSound()//Hurt sound flag to avoid span
    {
        while (isTakingDotDamage && !hasDied)
        {
            if (hurtSound && audioSource)
                audioSource.PlayOneShot(hurtSound);

            yield return new WaitForSeconds(.8f);
        }
    }

    public void CancelHurtLoop()//Cancels the hurt loop sound
    {
        isTakingDotDamage = false;
        if(damageSoundRoutine != null)
        {
            StopCoroutine(damageSoundRoutine);
            damageSoundRoutine = null;
        }
    }

    void updatePlayerUI()
    {
        float healthPercent = (float)currentHealth / maxHealth;

        // Update fill amount
        GameManager.instance.playerHPBar.fillAmount = healthPercent;
        GameManager.instance.playerHPText.text = currentHealth.ToString();

        // Update color
        if (healthPercent >= 0.5f)
        {
            GameManager.instance.playerHPBar.color = Color.green;
        }
        else if (healthPercent >= 0.25f)
        {
            GameManager.instance.playerHPBar.color = Color.orange;
        }
        else
        {
            GameManager.instance.playerHPBar.color = Color.red;
        }

        if(lowHealthFlashAnimator != null)
        {
            if(healthPercent <= lowHealthThreashHold && !hasDied)
            {
                if(!isFlashingLowHealth)
                {
                    lowHealthFlashAnimator.SetBool("IsLowHealth", true);
                    isFlashingLowHealth = true;
                    Debug.Log("Low Health: Flashing!");
                }
            }
            else
            {
                if (isFlashingLowHealth)
                {
                    lowHealthFlashAnimator.SetBool("IsLowHealth", false);
                    isFlashingLowHealth= false;
                    Debug.Log("Low Health flashing stopped.");
                }
            }
        }

        
    }

    void EnableDeathCamera()//Enable second camera on death to follow death animation view
    {
        if (deathCamera != null && headBone != null)
        {
            deathCamera.gameObject.SetActive(true);
            deathCamera.transform.position = headBone.position + cameraOffset;
            deathCamera.transform.rotation = headBone.rotation;
            deathCamera.transform.SetParent(null);
            StartCoroutine(FollowHeadAfterDeath());
        }
    }

    IEnumerator damageFlash()
    {
        GameManager.instance.flashDamageScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.flashDamageScreen.SetActive(false);
    }


    IEnumerator HealFlash()
    {
        GameManager.instance.flashHealScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.flashHealScreen.SetActive(false);
    }

    IEnumerator HandleDeathSequence()
    {
        yield return new WaitForSeconds(5f);
        GameManager.instance.youLose();
        Debug.Log("Player died!");
    }

    IEnumerator FollowHeadAfterDeath()
    {
        while (true)
        {
            if (headBone == null) yield break;

            deathCamera.transform.position = Vector3.Lerp(
                deathCamera.transform.position,
                headBone.position + cameraOffset,
                Time.deltaTime * followSpeed
            );

            deathCamera.transform.rotation = Quaternion.Lerp(
                deathCamera.transform.rotation,
                headBone.rotation,
                Time.deltaTime * followSpeed
            );

            yield return null;
        }
    }


    public void ApplyBleed(int damagePerTick, float tickInterval, float duration)
    {
        if(bleedCoroutine != null)
        {
            StopCoroutine(bleedCoroutine);
        }

        activeBleedDamagePerTick = damagePerTick;
        activeBleedTickInterval = tickInterval;

        // start bleed coroutine
        bleedCoroutine = StartCoroutine(BleedRoutine(duration));
    }

    public void RemoveBleed()
    {
        if(bleedCoroutine != null)
        {
            StopCoroutine(bleedCoroutine);
            bleedCoroutine = null;
        }
    }

    private IEnumerator BleedRoutine(float duration)
    {
        float timer = 0f;
        while(timer < duration)
        {
            yield return new WaitForSeconds(activeBleedTickInterval);

            if (hasDied)
            {
                RemoveBleed();
                yield break;
            }

            takeDamage(activeBleedDamagePerTick);
            timer += activeBleedTickInterval;
        }
        RemoveBleed();
    }

    public void Revive()
    {
        hasDied = false;

        // Re-enable player controller
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
            controller.enabled = true;

        // Re-enable weapon firing
        PlayerWeaponManager weaponManager = GetComponent<PlayerWeaponManager>();
        if (weaponManager != null)
            weaponManager.SetCanFire(true);

        // Re-enable camera controls
        CameraController cameraController = GetComponentInChildren<CameraController>();
        if (cameraController != null)
            cameraController.enabled = true;

        // Hide death camera
        if (deathCamera != null)
            deathCamera.gameObject.SetActive(false);

        // Reset death animation
        if (animator != null)
            animator.Rebind(); // Resets all animation states
    }

}
