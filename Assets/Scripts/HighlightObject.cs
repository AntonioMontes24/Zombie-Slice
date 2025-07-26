using UnityEngine;

public class HighlightObject : MonoBehaviour
{
    [SerializeField] private Renderer[] renderersToHighlight;
    [SerializeField] private Color highlightColor = Color.yellow;

    private Material[] originalMaterials;
    private Material[] instancedMaterials;
    private Color[] originalColors;

    void Awake()
    {
        if (renderersToHighlight == null || renderersToHighlight.Length == 0)
            renderersToHighlight = GetComponentsInChildren<Renderer>();

        int count = renderersToHighlight.Length;
        originalMaterials = new Material[count];
        instancedMaterials = new Material[count];
        originalColors = new Color[count];

        for (int i = 0; i < count; i++)
        {
            originalMaterials[i] = renderersToHighlight[i].sharedMaterial;
            instancedMaterials[i] = new Material(originalMaterials[i]); // Clone
            originalColors[i] = instancedMaterials[i].color;
            renderersToHighlight[i].material = instancedMaterials[i];
        }
    }

    public void EnableHighlight()
    {
        for (int i = 0; i < renderersToHighlight.Length; i++)
        {
            instancedMaterials[i].color = highlightColor;

            // Optional Emission Glow
            instancedMaterials[i].EnableKeyword("_EMISSION");
            instancedMaterials[i].SetColor("_EmissionColor", highlightColor * .65f);
        }
    }

    public void DisableHighlight()
    {
        for (int i = 0; i < renderersToHighlight.Length; i++)
        {
            instancedMaterials[i].color = originalColors[i];

            // Disable emission
            instancedMaterials[i].SetColor("_EmissionColor", Color.black);
            instancedMaterials[i].DisableKeyword("_EMISSION");
        }
    }
}