using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class newEnemyAI : MonoBehaviour, IDamage
{

    [SerializeField] float stopDistPlayer;

    [SerializeField] int facePlayer = 6;
    [SerializeField] int HP;
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] int roamDist;
    [SerializeField] int roamStopTime;
    [SerializeField] int animSpeedTrans;
    [SerializeField] float playerDist;
 
    float startSpeed;
    public GameObject playerObj;
    float roamTime;
    private float YOrig;
  
    bool inRange = false;
    bool meleeRange = false;
    bool isDead = false;
    bool headHit;
    // public GameObject playerObj;
    public Collider z1Collide;
    public Collider headCollider;
    Color originalColor;
    Vector3 startingPos;
    Vector3 lastPos;
    public Animator anim;
    void Awake()
    {
        startingPos = transform.position;
        agent.speed = 1.85f; 
    }
    void Start()
    {
        originalColor = model.material.color;
        YOrig = transform.position.y;
        ObjectiveManager.instance.updateZombieCount(1);
        z1Collide = GetComponent<Collider>();
        startSpeed = agent.speed;
        lastPos = transform.position;
        headCollider = GetComponentInChildren<Collider>();
        
    }

    void Update()
    {
        setAnimations();
        //playerDist = Vector3.Distance(GameManager.instance.player.transform.position, transform.position);
        if (agent.remainingDistance < 0.01f)
        {
            roamTime += Time.deltaTime;
        }
        
        
        if (!inRange)
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

    }

    void setAnimations()
    {
        float moveSpeed = (transform.position - lastPos).magnitude / Time.deltaTime;
        lastPos = transform.position;
        float animSpeedCur = anim.GetFloat("Speed");
        if (!meleeRange)
        {
            anim.SetFloat("Speed", Mathf.Lerp(animSpeedCur, moveSpeed, Time.deltaTime * animSpeedTrans));
        }
        else if (meleeRange)
        {
            anim.SetFloat("Speed", 0);
        }
        if (isDead)
            {
                anim.SetBool("isDead", true);
                StartCoroutine(isDeadAnim());
            }
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
        }
        // if (other.gameObject.CompareTag("Enemy_head"))
        // {
        //     headHit = true;
        // }
        // else
        //     headHit = false;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
        }
   
    }



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
        if (GameManager.instance.player.transform.position == null || isDead) return;
        Vector3 playerGroundPOS = new Vector3(GameManager.instance.player.transform.position.x, YOrig, GameManager.instance.player.transform.position.z);
        Vector3 directionPlayer = playerGroundPOS - transform.position;
        Vector3 dirFlat = new Vector3(directionPlayer.x, 0, directionPlayer.z);//Was moving upwards as it got closer to the player
        Vector3 direction = playerGroundPOS - transform.position;
        float distance = direction.magnitude;
        //playerDist = Vector3.Distance(transform.position, playerObj.position);
        //Stop in front of player
        if (playerDist < stopDistPlayer)
        {
            // StartCoroutine(attackAnim());
            meleeRange = true;
            //agent.isStopped = true;
        }
        else if (directionPlayer.magnitude > stopDistPlayer)
        {
            agent.isStopped = false;
            meleeRange = false;
            Vector3 moveTo = transform.position + direction.normalized * (distance - stopDistPlayer);
            //StartCoroutine(stopAttackAnim());
            agent.stoppingDistance = stopDistPlayer;
            agent.SetDestination(moveTo);
        }
        if (dirFlat != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dirFlat);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * facePlayer);
           // StartCoroutine(runAnim());
            agent.speed = 6f;
        }
    }

    void deadEnemy()
    {
        if (!isDead) return;
        if (!anim.GetBool("isDead"))
            StartCoroutine(isDeadAnim());
        
        
    }

    //Animation Coroutines
    //Attack
    IEnumerator attackAnim()
    {
        // Activate hitbox
        foreach (var hitbox in GetComponentsInChildren<zombieHitBox>())
        {
            hitbox.Activate();
        }

        anim.SetTrigger("Attack");

        // Wait for the swing duration (adjust as needed)
        yield return new WaitForSeconds(0.5f);

        // Deactivate hitbox after attack
        foreach (var hitbox in GetComponentsInChildren<zombieHitBox>())
        {
            hitbox.Deactivate();
        }

        yield return !meleeRange;
    }

    IEnumerator stopAttackAnim()
    {
        anim.SetInteger("Motion", 3);
        anim.SetBool("MeleeRange", false);
        yield return new WaitForSeconds(2.0f);
        anim.SetInteger("Motion", 0);
        roamCheck();
    }
    //Get hit
    IEnumerator getHitAnim()
    {
        model.material.color = Color.red;
        agent.isStopped = true;
        anim.SetTrigger("getHit");
        yield return new WaitForSeconds(0.3f);
        agent.isStopped = false;
        model.material.color = originalColor;
    }
    //Die
    IEnumerator isDeadAnim()
    {
        z1Collide.enabled = false;
        anim.SetBool("isDead", true);
        agent.isStopped = true;
        agent.speed = 0;
        yield return new WaitForSeconds(2.5f);
        anim.enabled = false;
        agent.enabled = false;
        ObjectiveManager.instance.updateZombieCount(-1);
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

    
}
