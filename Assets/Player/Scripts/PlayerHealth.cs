using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamage
{
    [Header("Player Health")]
    [SerializeField] int maxHealth;
    [SerializeField] int currentHealth;
    public int HPOrig;

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
        audioSource = GetComponent<AudioSource>();
    }

    public void takeDamage(int amount)// handles damage take
    {
        if (hasDied) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            if (!hasDied)
            {
                hasDied = true;
                if (deathSound && audioSource)
                    audioSource.PlayOneShot(deathSound);
                Die();

                if (damageSoundRoutine != null)
                    StopCoroutine(damageSoundRoutine);
            }
        }
        else
        {
            if (!isTakingDotDamage)
            {
                isTakingDotDamage = true;
                damageSoundRoutine = StartCoroutine(LoopHurtSound());
            }
        }

        Debug.Log("PlayerHealth: " + currentHealth);
    }

    public void Heal(int amount)//Handles healing waiting on health pick up to test
    {
        currentHealth += Mathf.Min(currentHealth + amount, maxHealth);
    }

    void Die()//Handles death/VFX/SFX
    {
        if (deathEffect) Instantiate(deathEffect, transform.position, Quaternion.identity);
        if (deathSound && audioSource) audioSource.PlayOneShot(deathSound);

        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        var weapon = GetComponent<PlayerWeaponManager>();
        if (weapon != null) weapon.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Player died!");
    }

    public void updateOrigHP()//Updates Health
    {
        HPOrig = currentHealth;
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


    //Temporary Debug Info
    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 24;
        style.normal.textColor = Color.red;

        GUI.Label(new Rect(10,10,300,40), "Health: " + currentHealth,style);
    }

}
