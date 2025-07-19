using UnityEngine;

public class KillManager : MonoBehaviour
{

    private int killCount = 0;
    private int checkpointKillCount = 0;

    public int KillCount => killCount;
    public int CheckpointKillCount => checkpointKillCount;

    public static KillManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void registerKill()
    {
        killCount++;
        Debug.Log("kills: " + killCount);

    }

    public void ResetKills()
    {
        killCount = 0;
        Debug.Log("Kill count reset to zero.");
    }
    public void SaveCheckpointKills()
    {
        checkpointKillCount = killCount;
        Debug.Log("Checkpoint kill count saved: " + checkpointKillCount);
    }

    public void RestoreCheckpointKills()
    {
        killCount = checkpointKillCount;
        Debug.Log("Kill count restored to checkpoint value: " + killCount);
    }
    public void ResetKillCount()
    {
        killCount = 0;
        Debug.Log("Kill count reset!");
    }

    void OnEnable()
    {
        // Reset kill count every time scene loads
        ResetKillCount();
    }

}
