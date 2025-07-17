using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text fpsText;
    private float measurePeriod = 0.5f;
    private int fpsAccumulator = 0;
    private float nextPeriod = 0;
    private int currentFPS;

    void OnEnable()
    {
        fpsAccumulator = 0;
        nextPeriod = Time.realtimeSinceStartup + measurePeriod;
    }

    void Update()
    {
        fpsAccumulator++;

        if (Time.realtimeSinceStartup > nextPeriod)
        {
            currentFPS = (int)(fpsAccumulator / measurePeriod);
            fpsAccumulator = 0;
            nextPeriod += measurePeriod;

            if (fpsText != null)
            {
                fpsText.text = currentFPS + " FPS";
            }
        }
    }
}
