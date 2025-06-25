using UnityEngine;
using UnityEngine.Events;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager instance;
    int zombieCount;
    int spawnerCount;

    [System.Serializable]
    class Objective
    {
        public UnityEvent completeEvent;
        [TextArea]
        public string objectiveDescription;
    }
    [SerializeField] Objective[] objectives;
    int idx;

    private void Awake()
    {
        instance = this;
        GameManager.instance.objectiveText.text = objectives[0].objectiveDescription;
    }

    public void updateSpawnerCount(int amount)
    {
        spawnerCount += amount;
        CheckObjective();
    }

    public void updateZombieCount(int amount)
    {
        zombieCount += amount;
        if (amount < 0)
            KillManager.instance.registerKill();
        GameManager.instance.zombieCountText.text = zombieCount.ToString("F0");
        CheckObjective();
    }

    void CheckObjective()
    {
        if (zombieCount <= 0 && spawnerCount <= 0)
        {
            objectives[idx].completeEvent?.Invoke();
            idx++;
            if (idx >= objectives.Length)
            {
                GameManager.instance.youWin();
            }
            GameManager.instance.objectiveText.text = objectives[idx].objectiveDescription;
        }
    }

    public int GetZombieCount()
    {
        return zombieCount;
    }
}
