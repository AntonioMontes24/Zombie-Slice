using UnityEngine;

public class ShellCasing : MonoBehaviour
{
    public AudioClip hitSound;
    public float minVelocity = 0.1f;
    public float destroyDelay = 5f;

    private AudioSource audioSource;
    private bool hasPlayed = false;

    void Start()//Handles shell sfx 
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = hitSound;
        audioSource.spatialBlend = 1f; // 3D sound
        Destroy(gameObject, destroyDelay);
    }

    void OnCollisionEnter(Collision collision)//Checks collision with ground or object to play sfx
    {
        if (!hasPlayed && collision.relativeVelocity.magnitude > minVelocity)
        {
            audioSource.Play();
            hasPlayed = true;
        }
    }
}
