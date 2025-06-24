using UnityEngine;

public class KillManager : MonoBehaviour
{

    private int killCount = 0;

    public int KillCount => killCount;

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
}
