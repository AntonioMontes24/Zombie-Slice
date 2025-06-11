using NUnit.Framework.Constraints;
using System.Collections;
using UnityEngine;

public class Lights : MonoBehaviour
{

    public Light targetLight;
    public int flashRate;

    private Coroutine flashCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (targetLight != null) {

            targetLight.enabled = true;
        
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && flashCoroutine == null)
        {
            flashCoroutine = StartCoroutine(FlashLight());
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && flashCoroutine != null) {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
            targetLight.enabled = true;
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

}
