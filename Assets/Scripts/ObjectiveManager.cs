using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager instance;

    //this will now track the CURRENT objective's progression 
    private int zombieCount;
    private int spawnerCount;

    [System.Serializable]
    public class Objective
    {
        public UnityEvent completeEvent;
        [TextArea]
        public string objectiveDescription;
        public GameObject waveContainter;

    }
    [SerializeField] public Objective[] objectives;

    private int objectivesIndex;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if(objectives == null || objectives.Length == 0)
        {
            Debug.LogError("Objective Manager: No objectives defined!");
            enabled = false;
            return;
        }
        objectivesIndex = 0;
        SetcurrentObjective(objectivesIndex);
    }

    private void Start()
    {
        objectivesIndex = 0;
        SetcurrentObjective(objectivesIndex);
    }

    private void SetcurrentObjective(int index)
    {
        if (index >= objectives.Length)
        {
            return;
        }

        objectivesIndex = index;
        Objective currentObj = objectives[objectivesIndex];

        if(currentObj.waveContainter != null)
        {
            ZVariant1_AI[] prePlacedZombies = currentObj.waveContainter.GetComponentsInChildren<ZVariant1_AI>(true);
            //zombieCount = prePlacedZombies.Length;

            ZombieVariant2AI[] prePlacedV2Zombies = currentObj.waveContainter.GetComponentsInChildren<ZombieVariant2AI>(true);
            
            zombieCount = prePlacedV2Zombies.Length + prePlacedZombies.Length;

            zombieSpawnTrap[] prePlacedSpawners = currentObj.waveContainter.GetComponentsInChildren<zombieSpawnTrap>(true);
            spawnerCount = prePlacedSpawners.Length;


            for(int i = index + 1; i < objectives.Length; i++)
            {
                if(objectives[i].waveContainter != null)
                {
                    objectives[i].waveContainter.SetActive(false);
                }
            }
            currentObj.waveContainter.SetActive(true);

        }else
        {
            zombieCount = 0;
            spawnerCount = 0;
        }



        if (GameManager.instance != null)
        {
            if (GameManager.instance.objectiveText != null)
            {
                GameManager.instance.objectiveText.text = currentObj.objectiveDescription;
            }
            if (GameManager.instance.zombieCountText != null)
            {
                GameManager.instance.zombieCountText.text = zombieCount.ToString("F0");
            }
            if (GameManager.instance.spawnerCountText != null)
            {
                GameManager.instance.UpdateSpawnerCountUI(spawnerCount);
            }
        }

    }

    public void updateSpawnerCount(int amount)
    {
        spawnerCount += amount;
        if (GameManager.instance != null && GameManager.instance.spawnerCountText != null)
        {
            GameManager.instance.UpdateSpawnerCountUI(spawnerCount);
        }
        CheckObjective();
    }

    public void updateZombieCount(int amount)
    {
        zombieCount += amount;
        if(GameManager.instance != null && GameManager.instance.zombieCountText!= null)
        {
            GameManager.instance.zombieCountText.text = Mathf.Max(0, zombieCount).ToString("F0");
        }
        if (amount < 0)
        {
            if(KillManager.instance != null)
            {
                KillManager.instance.registerKill();
            }
        }
        CheckObjective();
    }

    void CheckObjective()
    {
        if (objectivesIndex >= objectives.Length) return;

        bool zombiesCleared = (zombieCount <= 0);
        bool spawnersCleaner = (spawnerCount <= 0);

        if (zombiesCleared && spawnersCleaner)
        {
            objectives[objectivesIndex].completeEvent?.Invoke();
            SetcurrentObjective(objectivesIndex + 1);
        }


    }

    public int GetZombieCount()
    {
        return zombieCount;
    }
}
