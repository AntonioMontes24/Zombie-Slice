using UnityEngine;
using UnityEngine.SceneManagement;

public class DoNotDestroy : MonoBehaviour
{
    private void Awake()
    {
        GameObject[] musicObj = GameObject.FindGameObjectsWithTag("GameMusic");
       
        if(musicObj.Length > 1)
        {
            Destroy(this.gameObject);
        }
        


        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        Scene scene = SceneManager.GetActiveScene();
        Debug.Log("SCENE: " + scene.name);
        if (scene.name == "Zombie_Scene(Main)")
        {
            Destroy(this.gameObject);
        }
    }


}
