using UnityEngine;

public class KillManager : MonoBehaviour
{

    public int killCount = 0;

    public static KillManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
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

    public int getKillCount()
    {
        return killCount; 
    }
}
