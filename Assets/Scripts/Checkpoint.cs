using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Checkpoint : MonoBehaviour
{
    [Header("Leave empty to keep this as a regular checkpoint")]
    [SerializeField] bool usesSceneChange;
    [SerializeField] string nextSceneName;

    [SerializeField] Renderer[] models;
    Color color = Color.white;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || 
            Vector3.Distance(GameManager.instance.playerSpawnPoint, transform.position) < 0.1f)
            return;

        if (usesSceneChange)
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // Save new spawn point
            GameManager.instance.playerSpawnPoint = transform.position;

            // Save player health
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                GameManager.instance.savedHealth = playerHealth.CurrentHealth;
            }

            // Save weapon data
            PlayerWeaponManager weaponManager = other.GetComponent<PlayerWeaponManager>();
            if (weaponManager != null)
            {
                GameManager.instance.savedWeaponData = weaponManager.GetWeaponSaveData();
            }

            // Save checkpoint kills
            if (KillManager.instance != null)
            {
                KillManager.instance.SaveCheckpointKills();  // Save kills at checkpoint
            }

            StartCoroutine(Flash());
        }
    }

    IEnumerator Flash()
    {
        foreach (var model in models)
            model.material.color = Color.green;
        yield return new WaitForSeconds(0.5f);
        foreach (var model in models)
            model.material.color = color;
    }
}
