using System.Collections;
using JetBrains.Annotations;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.AI;

public class v2Mover : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] int roamDist;
    [SerializeField] int roamStop;
    [SerializeField] float stopDistPlayer;
    [SerializeField] float facePlayerSpeed;
    [SerializeField] int stopDist;

    // public GameObject playerObj;

    bool playerInRange;
    bool isDead;
    float roamTime;


    Vector3 startingPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingPos = transform.position;
        //ObjectiveManager.instance.updateZombieCount(1);

    }

    // Update is called once per frame
    void Update()
    {
        if (agent.remainingDistance < 0.01f)
        {
            roamTime += Time.deltaTime;
        }

        if (!playerInRange)
        {
            roamCheck();
        }
        else
        {
            chasePlayer();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void roamCheck()
    {
        if (roamTime >= roamStop && agent.remainingDistance < 0.01f)
        {
            roam();
        }
    }

    void roam()
    {
        roamTime = 0;
        agent.stoppingDistance = 0;

        Vector3 ranPos = Random.insideUnitSphere * roamDist;

        ranPos += startingPos;
        NavMeshHit hit;
        NavMesh.SamplePosition(ranPos, out hit, roamDist, 1);
        agent.SetDestination(hit.position);
        agent.speed = 1.85f;
    }

    void chasePlayer()
    {
        if (GameManager.instance.player.transform.position == null || isDead) return;
        Vector3 playerGroundPOS = new Vector3(GameManager.instance.player.transform.position.x, 0, GameManager.instance.player.transform.position.z);
        Vector3 directionPlayer = playerGroundPOS - transform.position;
        Vector3 dirFlat = new Vector3(directionPlayer.x, 0, directionPlayer.z);//Was moving upwards as it got closer to the player
        Vector3 direction = playerGroundPOS - transform.position;
        float distance = direction.magnitude;
        //Stop in front of player
        if (distance < stopDistPlayer)
        {
            agent.isStopped = true;

        }
        else
        {
            agent.isStopped = false;
            Vector3 moveTo = transform.position + direction.normalized * (distance - stopDistPlayer);
            agent.stoppingDistance = stopDist;
            agent.SetDestination(moveTo);
        }
        if (dirFlat != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dirFlat);
            transform.rotation = lookRotation * Quaternion.Euler(0, 0, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * facePlayerSpeed);

        }
    }
    //Will call from child object when damage taken
    public void forceChasePlayer()
    {
        playerInRange = true;
        roamTime = 0f;

        if (GameManager.instance.player.transform != null)
        {
            Vector3 playerGroundPOS = new Vector3(GameManager.instance.player.transform.transform.position.x, 0, GameManager.instance.player.transform.transform.position.z);
            agent.stoppingDistance = stopDist;
            agent.SetDestination(playerGroundPOS);
            agent.speed = 6.0f;
        }
    }
    //After death zombie sinks into ground before being destroyed
    private IEnumerator forceDeathSink()
    {
        agent.enabled = false;
        transform.Translate(Vector3.down * 1.0f * Time.deltaTime);
        yield return new WaitForSeconds(2.0f);
        Destroy(gameObject, 2.0f);
       // ObjectiveManager.instance.updateZombieCount(-1); //Calls this too many times. not sure why

    }

    public void SinkingZombie()
    {
        StartCoroutine(forceDeathSink());
    }

    public void forceStop()
    {
        agent.isStopped = true;
        agent.speed = 0;
        isDead = true;
    }

}
