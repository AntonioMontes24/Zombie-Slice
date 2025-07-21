using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialSteps
    {
        public string message;
        public Transform target;
        public float triggerRadius;

        public bool requireKeyPress = false;
        public bool requireProximity = false;

        public Key[] requiredKeys;
        public GameObject keyItemToHighlight;
        public InputActionReference[] requiredActions; // Unity Input System references

    }
    

    public TutorialSteps[] steps;
    public TMP_Text tutorialText;

    private int currentStep = 0;
    private Transform player;

    private HashSet<Key> keysPressed = new HashSet<Key>();
    private HashSet<InputActionReference> actionsTriggered = new HashSet<InputActionReference>();

    private GameObject lastHighlightedObject;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Enable all required actions
        foreach (var step in steps)
        {
            if (step.requiredActions != null)
            {
                foreach (var actionRef in step.requiredActions)
                {
                    actionRef?.action.Enable();
                }
            }
        }

        if (steps.Length > 0)
        {
            ShowCurrentStep();
        }
    }

    void Update()
    {
        if (currentStep >= steps.Length || player == null) return;

        TutorialSteps step = steps[currentStep];
        bool inRange = true;

        // Proximity check
        if (step.requireProximity && step.target != null)
        {
            float distance = Vector3.Distance(player.position, step.target.position);
            inRange = distance <= step.triggerRadius;
        }

        // Key press check (legacy keys)
        bool legacyKeysCompleted = true;
        if (step.requireKeyPress && step.requiredKeys != null && step.requiredKeys.Length > 0)
        {
            foreach (Key key in step.requiredKeys)
            {
                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    keysPressed.Add(key);
                }
            }

            foreach (Key key in step.requiredKeys)
            {
                if (!keysPressed.Contains(key))
                {
                    legacyKeysCompleted = false;
                    break;
                }
            }
        }

        // Input system actions check
        bool inputActionsCompleted = true;
        if (step.requireKeyPress && step.requiredActions != null && step.requiredActions.Length > 0)
        {
            foreach (var actionRef in step.requiredActions)
            {
                if (actionRef != null && actionRef.action.triggered)
                {
                    actionsTriggered.Add(actionRef);
                }
            }

            foreach (var actionRef in step.requiredActions)
            {
                if (!actionsTriggered.Contains(actionRef))
                {
                    inputActionsCompleted = false;
                    break;
                }
            }
        }

        // Decide if we should advance
        if (step.requireKeyPress && inRange)
        {
            if (legacyKeysCompleted && inputActionsCompleted)
            {
                AdvanceStep();
            }
        }
        else if (!step.requireKeyPress && step.requireProximity && inRange)
        {
            AdvanceStep();
        }
    }

    void ShowCurrentStep()
    {
        if (tutorialText == null)
            return;

        tutorialText.gameObject.SetActive(true);
        tutorialText.text = steps[currentStep].message;

        keysPressed.Clear();
        actionsTriggered.Clear();

        // Disable highlight on last object
        if (lastHighlightedObject != null)
        {
            HighlightObject prevHighlight = lastHighlightedObject.GetComponent<HighlightObject>();
            if (prevHighlight != null)
                prevHighlight.DisableHighlight();
        }

        // Enable highlight on new object
        GameObject newHighlight = steps[currentStep].keyItemToHighlight;
        if (newHighlight != null)
        {
            HighlightObject highlight = newHighlight.GetComponent<HighlightObject>();
            if (highlight != null)
                highlight.EnableHighlight();

            lastHighlightedObject = newHighlight;
        }
    }

    void AdvanceStep()
    {
        if (currentStep < steps.Length && steps[currentStep].target != null)
        {
            HighlightObject highlight = steps[currentStep].target.GetComponent<HighlightObject>();
            if (highlight != null)
                highlight.DisableHighlight();
        }

        currentStep++;

        if (currentStep < steps.Length)
        {
            ShowCurrentStep();
        }
        else
        {
            if (tutorialText != null)
                tutorialText.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        foreach (var step in steps)
        {
            if (step.requiredActions != null)
            {
                foreach (var actionRef in step.requiredActions)
                {
                    actionRef?.action?.Disable();
                }
            }
        }
    }

    public void StartTutorial()
    {
        currentStep = 0;
        keysPressed.Clear();
        actionsTriggered.Clear();

        // Reassign player in case it changed between scenes
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (steps.Length > 0)
        {
            ShowCurrentStep();
        }

        // Enable input actions again (in case they were disabled)
        foreach (var step in steps)
        {
            if (step.requiredActions != null)
            {
                foreach (var actionRef in step.requiredActions)
                {
                    actionRef?.action?.Enable();
                }
            }
        }
    }
    public bool IsTutorialComplete => currentStep >= steps.Length;
}