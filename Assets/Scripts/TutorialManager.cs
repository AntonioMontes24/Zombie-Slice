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
    }

    private GameObject lastHighlightedObject;

    public TutorialSteps[] steps;
    public TMP_Text tutorialText;

    private int currentStep = 0;
    private Transform player;
    private HashSet<Key> keysPressed = new HashSet<Key>();

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (steps.Length > 0)
        {
            ShowCurrentStep();
        }
    }

    void Update()
    {
        if (currentStep >= steps.Length) return;

        TutorialSteps step = steps[currentStep];
        bool inRange = true;

        // Proximity check
        if (step.requireProximity && step.target != null)
        {
            float distance = Vector3.Distance(player.position, step.target.position);
            inRange = distance <= step.triggerRadius;
        }

        // Key press check 
        if (step.requireKeyPress && inRange)
        {
            foreach (Key key in step.requiredKeys)
            {
                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    keysPressed.Add(key);
                }
            }

            // All keys in requiredKeys must be pressed
            bool allKeysPressed = true;
            foreach (Key key in step.requiredKeys)
            {
                if (!keysPressed.Contains(key))
                {
                    allKeysPressed = false;
                    break;
                }
            }

            if (allKeysPressed)
            {
                AdvanceStep();
            }
        }
        // Proximity only steps
        else if (!step.requireKeyPress && step.requireProximity && inRange)
        {
            AdvanceStep();
        }
    }

    void ShowCurrentStep()
    {
        tutorialText.gameObject.SetActive(true);
        tutorialText.text = steps[currentStep].message;

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
        if (steps[currentStep].target != null)
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
            tutorialText.gameObject.SetActive(false);
        }
    }
}
