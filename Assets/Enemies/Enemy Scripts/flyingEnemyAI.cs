using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class flyingEnemyAI : MonoBehaviour, IDamage
{
    public Transform[] patrolPoints;
    [SerializeField] public float speed = 2f;
    [SerializeField] public float stopDist = 0.5f;
    [SerializeField] float playerStopDistance;
    [SerializeField] float moveWait = 2f;
    [SerializeField] float fireRate;
    [SerializeField] NavMeshAgent agent;
    //[SerializeField] Renderer model;
    [SerializeField] int facePlayerSpeed;
    [SerializeField] int health;
    [SerializeField] GameObject blightBomb;
    [SerializeField] Transform firePos;
    [SerializeField] int animSpeedTrans;
    [SerializeField] float downAngle;
    Vector3 lastPos;
    [SerializeField] Vector3 startingPos;
    [SerializeField] int blightBombDamage;
    // public GameObject playerObj;
    public Animator anim;
    int currentPointIndex = 0;
    private float flightPos;

    [SerializeField] float waitTimer = 0f;
    bool isWaiting = false;
    bool inRange;
    bool isDead;
    void Start()
    {
        ShufflePoints();
        ObjectiveManager.instance.updateZombieCount(1);
        flightPos = transform.position.y;
        lastPos = transform.position;
        
    }

    void Update()
    {
        setAnimations();
        if (!inRange)
            patrolNextArea();
        else
            chasePlayer();
        if (isDead)
        {
            //death animation
        ObjectiveManager.instance.updateZombieCount(-1);
        }
    }

    void setAnimations()
    {
        float moveSpeed = (transform.position - lastPos).magnitude / Time.deltaTime;
        lastPos = transform.position;
        float animSpeedCur = anim.GetFloat("Speed");
        anim.SetFloat("Speed", Mathf.Lerp(animSpeedCur, moveSpeed, Time.deltaTime * animSpeedTrans));
        if (isDead)
        {
            anim.SetBool("isDead", true);
            StartCoroutine(death());
        }
    }
    void patrolNextArea()
    {
        if (patrolPoints.Length == 0)
            return;
        //Shuffler Fisher-Yates


        if (isWaiting)
        {
            bomberAttack();
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;

                currentPointIndex++;
                if (currentPointIndex >= patrolPoints.Length)
                {
                    currentPointIndex = 0;
                    ShufflePoints();
                }
            }
        }
        else
        {
            Transform target = patrolPoints[currentPointIndex];
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            Vector3 direction = target.position - transform.position;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = lookRotation * Quaternion.Euler(0, 180, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }

            if (Vector3.Distance(transform.position, target.position) <= stopDist)
            {
                isWaiting = true;
                waitTimer = moveWait;
            }
        }

    }

    void chasePlayer()
    {
        if (GameManager.instance.player.transform.position == null || isDead) return;
        Vector3 playerGroundPOS = new Vector3(GameManager.instance.player.transform.position.x, flightPos, GameManager.instance.player.transform.position.z);
        Vector3 directionPlayer = playerGroundPOS - transform.position;
        Vector3 dirFlat = new Vector3(directionPlayer.x, flightPos, directionPlayer.z);//Was moving upwards as it got closer to the player
        Vector3 direction = playerGroundPOS - transform.position;
        float distance = direction.magnitude;
        //Stop in front of player
        if (distance < playerStopDistance)
        {
            bomberAttack();
            //animate
        }
        else
        {
            Vector3 moveTo = transform.position + direction.normalized * (distance - playerStopDistance);
            agent.stoppingDistance = stopDist;
            agent.SetDestination(moveTo);
        }
        if (dirFlat != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dirFlat);
            transform.rotation = lookRotation * Quaternion.Euler(0, 180, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * facePlayerSpeed);

        }
    }

    void ShufflePoints()
    {
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            int randIndex = Random.Range(i, patrolPoints.Length);
            Transform temp = patrolPoints[i];
            patrolPoints[i] = patrolPoints[randIndex];
            patrolPoints[randIndex] = temp;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
        }
    }

    public void playerInRange(bool status)
    {
        inRange = status;
    }
    public void takeDamage(int amount)
    {
        if (health > 0)
        {
            health -= amount;
            agent.SetDestination(GameManager.instance.player.transform.position);
        }
    }

    void bomberAttack()
    {
        anim.SetTrigger("Fire");
    }

    public void createBlight()
    {
        if (GameManager.instance.player.transform.position == null) return;
        Vector3 target = GameManager.instance.player.transform.position; //Target player location

        target.y = 0f; //Target ground
        if (!inRange)
        {
            //Create
            GameObject bomb = Instantiate(blightBomb, firePos.position, firePos.rotation);
            Rigidbody rb = bomb.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (Quaternion.Euler(downAngle, 0, 0) * firePos.forward).normalized;
                rb.linearVelocity = direction * 10f;
            }

        }
        else if (inRange)
        {
            GameObject bomb = Instantiate(blightBomb, firePos.position, firePos.rotation);
            Rigidbody rb = bomb.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (Quaternion.Euler(downAngle, target.y, 0) * firePos.forward).normalized;
                rb.linearVelocity = direction * 10f;
            }

        }


    }

    IEnumerator death()
    {
        anim.enabled = false;
        Vector3 deathDrop = new Vector3(transform.position.x, 0, transform.position.z);
        yield return new WaitForSeconds(3.0f);
        Destroy(gameObject);
    }

   
}
