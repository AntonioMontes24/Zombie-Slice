using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager instance;
    public int currentCheckpointID;

    bool done;

    [SerializeField] GameObject[] objectives;
    IObjective[] objectiveInterfaces;
    int idx = 0;

    void Awake() => instance = this;

    void UpdateObjectiveText(string newText) => GameManager.instance.objectiveText.text = newText;

    private void Start()
    {
        objectiveInterfaces = new IObjective[objectives.Length];
        for (int i = 0; i < objectives.Length; i++)
        {
            objectiveInterfaces[i] = objectives[i].GetComponent<IObjective>();
            objectiveInterfaces[i].Register(UpdateObjectiveText);
        }
        if (objectiveInterfaces.Length != 0)
            objectiveInterfaces[idx].Start();
    }

    void Update()
    {
        if (done || objectiveInterfaces.Length == 0)
            return;
        if (idx >= objectiveInterfaces.Length)
        {
            done = true;
            GameManager.instance.youWin();
            return;
        }
        if (objectiveInterfaces[idx].isComplete == true)
        {
            idx++;
            objectiveInterfaces[idx].Start();
        }
    }
}
