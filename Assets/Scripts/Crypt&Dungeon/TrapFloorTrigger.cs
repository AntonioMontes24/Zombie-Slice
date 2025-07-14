using UnityEngine;

public class TrapFloorTrigger : MonoBehaviour
{
    public GameObject risingWall;                // Wall that rises to trap player
    public GameObject[] trapFloorPanels;         // Panels to destroy
    public GameObject[] risingFloorPanels;       // Panels that rise up after trap
    public GameObject zombiePrefab;              // Zombie to spawn
    public Transform[] zombieSpawnPoints;        // Where zombies appear
    public bool spawnZombiesOnTrigger = true;    // If true, spawn them dynamically

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            ActivateTrap();
        }
    }

    void ActivateTrap()
    {
        // Raise the wall
        if (risingWall != null)
        {
            risingWall.SetActive(true); // Or use animation if available
        }

        // Remove trap floor panels
        foreach (GameObject panel in trapFloorPanels)
        {
            Destroy(panel); // Or panel.SetActive(false);
        }

        // Raise the replacement floor panels
        foreach (GameObject panel in risingFloorPanels)
        {
            panel.SetActive(true); // Or trigger animation
        }

        // Spawn zombies if needed
        if (spawnZombiesOnTrigger)
        {
            foreach (Transform spawn in zombieSpawnPoints)
            {
                Instantiate(zombiePrefab, spawn.position, spawn.rotation);
            }
        }
    }
}
