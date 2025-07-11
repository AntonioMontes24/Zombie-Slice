using UnityEngine;
using TMPro;
using System.Collections.Generic;

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

        public KeyCode[] requiredKeys;

    }

    public TutorialSteps[] steps;
    public TMP_Text tutorialText;

    private int currentStep = 0;
    private Transform player;
    private HashSet<KeyCode> keysPressed = new HashSet<KeyCode>();


    void Start()
    {

        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (steps.Length > 0)
        {
            ShowCurrentStep();
        }

        
    }

    // Update is called once per frame
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
            foreach (KeyCode key in step.requiredKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    keysPressed.Add(key);
                }
            }

            // All keys in requiredKeys must be pressed
            bool allKeysPressed = true;
            foreach (KeyCode key in step.requiredKeys)
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
        tutorialText.text = steps[currentStep].message;
    }

    void AdvanceStep()
    {
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
