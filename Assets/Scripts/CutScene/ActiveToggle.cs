using UnityEngine;

public class ActiveToggle : MonoBehaviour
{
    public void ToggleActiveState()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
