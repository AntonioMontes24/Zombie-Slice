using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] int maxHealth;
    [SerializeField] int currentHealth;
    public int HPOrig;

    [Header("VFX")]
    public GameObject deathEffect;//If we want like a bloody screen or something
    public AudioClip hurtSound;//hurt sfx   
    public AudioClip deathSound;// death sfx


    private AudioSource audioSource;

    private void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(int amount)// handles damage take
    {
        currentHealth -= amount;

        if(hurtSound) audioSource?.PlayOneShot(hurtSound);

        if (currentHealth <= 0)
        {
            Die();
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


    //Temporary Debug Info
     void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 24;
        style.normal.textColor = Color.red;

        GUI.Label(new Rect(10,10,300,40), "Health: " + currentHealth,style);
    }

}
