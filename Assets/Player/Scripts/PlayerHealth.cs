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

    Coroutine damageSoundRoutine;

    bool playedHurtSound;
    bool isTakingDotDamage;
    bool hasDied;

    private AudioSource audioSource;

    private void Start()
    {
        currentHealth = maxHealth;
        updatePlayerUI();
        audioSource = GetComponent<AudioSource>();
    }

    public void takeDamage(int amount)// handles damage take
    {
        if (hasDied) return;

        // flash the damage on the screen
        StartCoroutine(damageFlash());

        currentHealth -= amount;
        updatePlayerUI();

        if (currentHealth <= 0)
        {
            if (!hasDied)
            {
                hasDied = true;
                if (deathSound && audioSource)
                    audioSource.PlayOneShot(deathSound);
                Die();
                GameManager.instance.youLose();

                if (damageSoundRoutine != null)
                    StopCoroutine(damageSoundRoutine);
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
    }

    public bool CanHeal()
    {
        return maxHealth != currentHealth;
    }

    void Die()//Handles death/VFX/SFX
    {
        if (deathEffect) Instantiate(deathEffect, transform.position, Quaternion.identity);
        if (deathSound && audioSource) audioSource.PlayOneShot(deathSound);

        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        var weapon = GetComponent<PlayerWeaponManager>();
        if (weapon != null) weapon.enabled = false;
        GameManager.instance.youLose();
        Debug.Log("Player died!");
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
        //playedHurtSound = false;
    }

    void updatePlayerUI()
    {
        float healthPercent = (float)currentHealth / maxHealth;

        // Update fill amount
        GameManager.instance.playerHPBar.fillAmount = healthPercent;

        // Update color
        if (healthPercent >= 0.5f)
        {
            GameManager.instance.playerHPBar.color = Color.green;
        }
        else if (healthPercent >= 0.25f)
        {
            GameManager.instance.playerHPBar.color = Color.yellow;
        }
        else
        {
            GameManager.instance.playerHPBar.color = Color.red;
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
}
