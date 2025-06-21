using UnityEngine;
using System.Collections;

public class wallScript : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    //public Vector3 currentPosition;
    [SerializeField] float slideSpeed;
    [SerializeField] float slideTime;
    bool isSliding;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // if (!isSliding)
        //     currentPosition = startPosition;
        //trigger activation when gamegoal met
    }

    public void OpenDoor()
    {
        if (!isSliding)
        {
            isSliding = true;
            startPosition = transform.position;
            slideTime = 0f;
            StartCoroutine(slideDoor());
        }
    }

    IEnumerator slideDoor()
    {
        while (slideTime < 1f)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, slideTime);
            slideTime += Time.deltaTime * slideSpeed;
            yield return null;
        }
        transform.position = endPosition;
        isSliding = false;
    }

    //Replace collision trigger to activate door when all regular enemies are dead.
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OpenDoor();
        }
    }
    //Psuedo - player enters into krypt to face boss
    /*
    bool bossFight = true;
    CloseDoor();
    Spawn boss;
    activate boss health and timer UI
    if(timer<=0)
    option 1: spawn an overwhelming number of zombies in the room - instant death
    spawn horde;
    option 2: have room actually hovering over the edge of the main plane map. - When timer expires, destroy floor 
    have room below with lava (or fire, key point is DOT damage)

    Either option will trigger a gameover screen
    */
    //After player has entered the krypt close the door behind them. - no escape 
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CloseDoor();
           
        }
    }
    public void CloseDoor()
    {
        if (!isSliding)
        {
            isSliding = true;
            startPosition = transform.position;
            slideTime = 0f;

            endPosition = startPosition == endPosition ? this.startPosition : this.startPosition;

            StartCoroutine(slideDoor());
        }
    }

}
