using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class newEnemyAI : MonoBehaviour, IDamage
{
    //public Transform[] patrolPoints;
    //[SerializeField] public float speed = 2f;
    //[SerializeField] public float stopDist = 0.5f;
    [SerializeField] public float stopDistPlayer = 1.0f;
    // [SerializeField] float moveWait = 2f;
    [SerializeField] int facePlayer = 6;
    [SerializeField] int HP;
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] int roamDist;
    [SerializeField] int roamStopTime;
    [SerializeField] int hitDmg;
    //int hitcount;
    [SerializeField] int hitRate;
    [SerializeField] float hitTimer;
    float startSpeed;
    // [SerializeField] int hitRate;
    //int hitCounter;
    //int currentPointIndex = 0;
    //float origStopDist;
    float roamTime;
    private float YOrig;
   // bool playerHit;
    //[SerializeField] float waitTimer = 0f;
    // bool isWaiting = false;
    bool inRange = false;
    bool meleeRange = false;
    bool isDead = false;
    //bool isRunning;
    public GameObject playerObj;
    public Collider z1Collide;
    public Collider ZombitHitBox;
    Color originalColor;
    Vector3 startingPos;
    public Animator anim;
    void Start()
    {
        originalColor = model.material.color;
        YOrig = transform.position.y;
        GameManager.instance.updateGameGoal(1);
        //origStopDist = agent.stoppingDistance;
        //ShufflePoints();
        //hitcount = 0;
        z1Collide = GetComponent<Collider>();
        startSpeed = agent.speed;
        ZombitHitBox = GetComponent<Collider>();
    }

    void Update()
    {
        if (agent.remainingDistance < 0.01f)
        {
            roamTime += Time.deltaTime;
        }
        if (!inRange)
            //patrolNextArea();
            roamCheck();
        else
            ChasePlayer();
        if (HP <= 0)
        {
            isDead = true;
            deadEnemy();
        }
        else
        {
            isDead = false;
        }

        // if (meleeRange && !isDead)
        // {
        //     hitTimer += Time.deltaTime;
        //     if (hitTimer >= hitRate)
        //     {
        //         attackTarget();
        //         hitTimer = 0f;
        //     }
        // }
        

    }


    // void patrolNextArea()
    // {

    //     if (patrolPoints.Length == 0)
    //         return;


    //     StartCoroutine(walkAnim());
    //     //Shuffler Fisher-Yates
    //     if (isWaiting)
    //     {
    //         StartCoroutine(idleAnim());
    //         waitTimer -= Time.deltaTime;
    //         if (waitTimer <= 0f)
    //         {
    //             isWaiting = false;
    //             //currentPointIndex = Random.Range(0, patrolPoints.Length);
    //             currentPointIndex++;
    //             if (currentPointIndex >= patrolPoints.Length)
    //             {
    //                 currentPointIndex = 0;
    //                 ShufflePoints();
    //             }
    //         }
    //     }
    //     else
    //     {
    //         Transform target = patrolPoints[currentPointIndex];
    //         transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    //         Vector3 direction = target.position - transform.position;
    //         if (direction != Vector3.zero)
    //         {
    //             Quaternion lookRotation = Quaternion.LookRotation(direction);
    //             // transform.rotation = lookRotation * Quaternion.Euler(0, 180, 0);
    //             transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    //         }

    //         if (Vector3.Distance(transform.position, target.position) <= stopDist)
    //         {
    //             isWaiting = true;
    //             waitTimer = moveWait;
    //         }
    //     }

    // }

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
        //Error check
        //Debug.Log("Player out of range");
    }

    

    // void ShufflePoints()
    // {
    //     for (int i = 0; i < patrolPoints.Length; i++)
    //     {
    //         int randIndex = Random.Range(i, patrolPoints.Length);
    //         Transform temp = patrolPoints[i];
    //         patrolPoints[i] = patrolPoints[randIndex];
    //         patrolPoints[randIndex] = temp;
    //     }
    // }
    void roamCheck()
    {
        if (roamTime >= roamStopTime && agent.remainingDistance < 0.01f)
        {
            roam();
        }
    }
    void roam()
    {
        roamTime = 0;
        agent.stoppingDistance = 0;

        Vector3 randPos = Random.insideUnitSphere * roamDist;

        randPos += startingPos;
        NavMeshHit hit;
        NavMesh.SamplePosition(randPos, out hit, roamDist, 1);
        agent.SetDestination(hit.position);
        agent.speed = 1.85f;
    }
    void ChasePlayer()
    {
        if (playerObj == null || isDead) return;
        Vector3 playerGroundPOS = new Vector3(playerObj.transform.position.x, YOrig, playerObj.transform.position.z);
        Vector3 directionPlayer = playerGroundPOS - transform.position;
        Vector3 dirFlat = new Vector3(directionPlayer.x, 0, directionPlayer.z);//Was moving upwards as it got closer to the player

        //Stop in front of player
        if (directionPlayer.magnitude < stopDistPlayer)
        {
            meleeRange = true;
            StartCoroutine(attackAnim());
            //Error Check
        }
        else
        {
            meleeRange = false;
            StartCoroutine(stopAttackAnim());
            //transform.position = Vector3.MoveTowards(transform.position, playerGroundPOS, speed * Time.deltaTime);
            agent.stoppingDistance = stopDistPlayer;
            agent.SetDestination(playerGroundPOS);
        }
        if (dirFlat != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dirFlat);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * facePlayer);
            StartCoroutine(runAnim());
            agent.speed = 6f;
        }
        // transform.position = Vector3.MoveTowards(transform.position, playerGroundPOS, speed * Time.deltaTime);
    }

    void deadEnemy()
    {
        if (!isDead) return;
        if (!anim.GetBool("isDead"))
            StartCoroutine(isDeadAnim());
        
        
    }

    //Animation Coroutines
    //Walk
    IEnumerator walkAnim()
    {
        anim.SetInteger("Motion", 0);
        anim.SetBool("isRunning", false);
        yield return new WaitForSeconds(0.1f);

    }
    //Stop Moving
    IEnumerator stopAnim()
    {
        anim.SetInteger("Motion", 3);
        yield return new WaitForSeconds(0.1f);
        anim.SetBool("isRunning", false);
    }
    //Idle
    IEnumerator idleAnim()
    {
        anim.SetInteger("Motion", 3);
        if (!inRange)
            anim.SetBool("isRunning", false);
        yield return !inRange;
    }
    //Run
    IEnumerator runAnim()
    {
        anim.SetInteger("Motion", 1);
        yield return new WaitForSeconds(0.1f);
        anim.SetBool("isRunning", true);
    }
    //Attack
    IEnumerator attackAnim()
    {
        anim.SetBool("MeleeRange", true);
        // playerHit = true;
        // hitcount = 1;
        // attackTarget();
        yield return new WaitForSeconds(1f);
        
        // yield return !meleeRange;
    }
    IEnumerator stopAttackAnim()
    {
        anim.SetBool("MeleeRange", false);
        yield return StartCoroutine(stopAnim());
        yield return StartCoroutine(walkAnim());
    }
    //Get hit
    IEnumerator getHitAnim()
    {
        model.material.color = Color.red;
        agent.isStopped = true;
        anim.SetTrigger("Hit");
        yield return new WaitForSeconds(0.3f);
        agent.isStopped = false;
        model.material.color = originalColor;
        
        // if (isDead)
        // {
        //     yield return new WaitForSeconds(0.1f);
        //     anim.SetTrigger("Dead");
        // }
        // else
        //     yield return new WaitForSeconds(0.1f);
    }
    //Die
    IEnumerator isDeadAnim()
    {
        z1Collide.enabled = false;
        anim.SetBool("isDead", true);
        agent.speed = 0f;
        yield return new WaitForSeconds(2.5f);
        anim.enabled = false;
        agent.enabled = false;
        Destroy(gameObject);
    }

    public void takeDamage(int amount)
    {
        if (isDead) return;
        HP -= amount;
        StartCoroutine(getHitAnim());
        inRange = true;
        ChasePlayer();
        if (HP <= 0)
        {
            isDead = true;
        }
        agent.isStopped = true;
    }

    // void attackTarget()
    // {
    //     IDamage dmg = GameManager.instance.player.GetComponent<IDamage>();
    //     if (meleeRange)
    //     {
    //         if (meleeRange && dmg != null)
    //         {
    //             dmg.takeDamage(hitDmg);
    //         }
    //     }
    // }
    
}
