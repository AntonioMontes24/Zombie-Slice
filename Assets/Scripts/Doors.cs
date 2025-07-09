using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private GameObject doorModel;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            return;

        Debug.Log(other.transform.name);

        IOpen open = other.GetComponent<IOpen>();

        if (open != null)
        {
            doorModel.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            return;

        IOpen open = other.GetComponent<IOpen>();

        if (open != null)
        {
            doorModel.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
