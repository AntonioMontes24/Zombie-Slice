using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    [Header("Leave empty to keep this as a regular checkpoint")]
    [SerializeField] bool usesSceneChange;
    [SerializeField] string nextSceneName;

    public int checkpointID;

    [SerializeField] Renderer[] models;
    Color color = Color.white;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") ||
            GameManager.instance.playerSpawnPoint == transform.position)
            return;
        if (usesSceneChange)
            SceneManager.LoadScene(nextSceneName);
        else
        {
            GameManager.instance.playerSpawnPoint = transform.position;
            GameManager.instance.currentCheckpointID = checkpointID;
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
