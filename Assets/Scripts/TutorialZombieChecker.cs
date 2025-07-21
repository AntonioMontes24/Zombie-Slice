using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ApartmentTutorialZombieChecker : MonoBehaviour
{
    private bool isChecking = false;

    void Update()
    {
       
        if (SceneManager.GetActiveScene().name != "Apartment_Tutorial")
            return;

        
        if (isChecking)
            return;

        
        int zombieCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (zombieCount == 0)
        {
            
            StartCoroutine(DelayThenLoadNextScene());
            isChecking = true;
        }
    }

    private IEnumerator DelayThenLoadNextScene()
    {
        yield return new WaitForSeconds(3f);

        
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
}