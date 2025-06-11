using NUnit.Framework.Constraints;
using System.Collections;
using UnityEngine;

public class Lights : MonoBehaviour
{

    public Light targetLight;
    public int flashRate;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (targetLight != null) {

            StartCoroutine(FlashLight());
        
        }
        
    }

    private IEnumerator FlashLight()
    {

        float interval = 1f / flashRate;

        while (true)
        {
            targetLight.enabled = !targetLight.enabled;
            yield return new WaitForSeconds(interval);
        }

    }

    // Update is called once per frame
    void Update()
    {

        
    }
}
