using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{

    public void BackButton()
    {
        SceneManager.LoadScene("Main Menu");
    }


}
