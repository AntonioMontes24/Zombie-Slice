using UnityEngine;

public class SceneSpawnManager : MonoBehaviour
{
    [SerializeField] private string defaultSpawnPointName = "DefaultSpawn";

    void Start()
    {
        string spawnName = PlayerPrefs.GetString("LastSpawnPoint", defaultSpawnPointName);

        GameObject spawnPoint = GameObject.Find(spawnName);
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (spawnPoint != null && player != null)
        {
            player.transform.position = spawnPoint.transform.position;
            player.transform.rotation = spawnPoint.transform.rotation;
        }
        else
        {
            Debug.LogWarning("Spawn point or player not found! Make sure they exist and are tagged properly.");
        }
    }
}
