using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class newEnemyAI : MonoBehaviour, IDamage
{

    [SerializeField] public float stopDistPlayer = 1.0f;

    [SerializeField] int facePlayer = 6;
    [SerializeField] int HP;
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] int roamDist;
    [SerializeField] int roamStopTime;
    //[SerializeField] int hitDmg;
 
    float startSpeed;

    float roamTime;
    private float YOrig;
  
    bool inRange = false;
    bool meleeRange = false;
    bool isDead = false;
    public GameObject playerObj;
    public Collider z1Collide;
    Color originalColor;
    Vector3 startingPos;
    public Animator anim;
    void Start()
    {
        originalColor = model.material.color;
        YOrig = transform.position.y;
        GameManager.instance.updateGameGoal(1);
        z1Collide = GetComponent<Collider>();
        startSpeed = agent.speed;
       
    }

    void Update()
    {
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
    // Activate hitbox
    foreach (var hitbox in GetComponentsInChildren<zombieHitBox>())
    {
        hitbox.Activate();
    }

    anim.SetBool("MeleeRange", true);

    // Wait for the swing duration (adjust as needed)
    yield return new WaitForSeconds(0.5f);

    // Deactivate hitbox after attack
    foreach (var hitbox in GetComponentsInChildren<zombieHitBox>())
    {
        hitbox.Deactivate();
    }

    yield return !meleeRange;
}


    // IEnumerator attackAnim()
    // {
    //     anim.SetBool("MeleeRange", true);
    //     yield return new WaitForSeconds(1f);
    //     anim.SetBool("MeleeRange", false);
        
        
    // }
    IEnumerator stopAttackAnim()
    {
        anim.SetInteger("Motion", 3);
        yield return new WaitForSeconds(2.0f);
        anim.SetInteger("Motion", 0);
        roamCheck();
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
        Destroy(gameObject);
    }

    public void takeDamage(int amount)
    {
        if (isDead) return;
        HP -= amount;
        StartCoroutine(getHitAnim());
        inRange = true;
        //ChasePlayer();
        if (HP <= 0)
        {
            isDead = true;
        }
        agent.isStopped = true;
    }

    
}
