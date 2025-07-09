using UnityEngine;
using UnityEngine.Events;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager instance;
    bool done;

    [Tooltip("Activates when all objectives are completed")]
    [SerializeField]
    string allCompleteMessage;

    [System.Serializable]
    class Objective
    {
        [Tooltip("The checkpoint id this objective is contained in")]
        public int checkpointId;
        public GameObject objective;
        public IObjective _objective;
        public UnityEvent onComplete;
        [Tooltip("Make sure to undo anything you did in onComplete")]
        public UnityEvent onRespawnReset;
    }
    
    [SerializeField] Objective[] objectives;
    int idx = 0;

    void Awake() => instance = this;
    void UpdateObjectiveText(string newText) => GameManager.instance.objectiveText.text = newText;
    void OnDestroy() => GameManager.instance.respawnHook -= RespawnReset;

    void RespawnReset()
    {
        for (int i = 0; i < objectives.Length; i++)
            if (objectives[i].checkpointId <= GameManager.instance.currentCheckpointID)
                objectives[i].onRespawnReset.Invoke();
    }

    private void Start()
    {
        GameManager.instance.respawnHook += RespawnReset;
        for (int i = 0; i < objectives.Length; i++)
        {
            objectives[i]._objective = objectives[i].objective.GetComponent<IObjective>();
            objectives[i]._objective.Register(UpdateObjectiveText);
        }
        if (objectives.Length != 0)
            objectives[idx]._objective.Start();
    }

    void Update()
    {
        if (done || objectives.Length == 0)
            return;
        if (idx >= objectives.Length)
        {
            done = true;
            GameManager.instance.objectiveText.text = allCompleteMessage;
            return;
        }
        if (objectives[idx]._objective.isComplete == true)
        {
            objectives[idx].onComplete.Invoke();
            idx++;
            objectives[idx]._objective.Start();
        }
    }
}
