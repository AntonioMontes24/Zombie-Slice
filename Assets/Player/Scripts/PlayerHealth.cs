using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    public float maxHealth;
    public float currentHealth;

    [Header("VFX")]
    public GameObject deathEffect;//If we want like a bloody screen or something
    public AudioClip hurtSound;
    public AudioClip deathSound;


    private AudioSource audioSource;

    private void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth,0f,maxHealth);

        if(hurtSound) audioSource?.PlayOneShot(hurtSound);

        if (currentHealth <= 0f)
        {
            Die();
        }
        Debug.Log("PlayerHealth: " + currentHealth);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    void Die()
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
    //Temporary Debug Info
     void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 24;
        style.normal.textColor = Color.red;

        GUI.Label(new Rect(10,10,300,40), "Health: " + currentHealth,style);
    }

}
