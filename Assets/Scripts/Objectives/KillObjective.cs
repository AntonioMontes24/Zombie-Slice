using System;
using UnityEngine;

public class KillObjective : MonoBehaviour, IObjective
{
    [Tooltip("This is the transform parent of all of the enemies you want to kill. Make an empty gameobject for this.")]
    [SerializeField]
    Transform targetEnemyGroupTransformParent;

    [Tooltip("Enemy count remaining is appended to this custom description")]
    [SerializeField]
    string description;

    bool active = false;

    public bool isComplete => !active;

    Action<string> _callback;

    public void Register(Action<string> callback)
    {
        _callback = callback;
    }
    private void Start() => GameManager.instance.respawnHook += ResetObjective;
    private void OnDestroy() => GameManager.instance.respawnHook -= ResetObjective;
    void ResetObjective() => active = false;

    void IObjective.Start() => active = true;

    void Update()
    {
        if (!active)
            return;
        if (targetEnemyGroupTransformParent.childCount == 0)
        {
            active = false;
            return;
        }
        _callback.Invoke(string.Format("{0}\nEnemies Remaining: {1}", description, targetEnemyGroupTransformParent.childCount));
    }
}
