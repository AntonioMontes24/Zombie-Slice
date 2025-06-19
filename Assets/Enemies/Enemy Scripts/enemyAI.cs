using System.Collections;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform headPOS;
    [SerializeField] int HP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int roamDist;
    [SerializeField] int roamStopTime;
    [SerializeField] Transform hitPos; //Hitting position may delete since melee
    [SerializeField] float hitRate;
    [SerializeField] int animSpeedTrans;
    [SerializeField] int FOV;
    [SerializeField] int hit_Damage;
    [SerializeField] int facePlayer;
    [SerializeField] Animator anim;

    Color colorOrig;
    public GameObject playerObj;
    private float origY;
    float hitTimer;
    float angleToPlayer;
    float roamTime;
    float stoppingDistOrig;
    public float stopDistPlayer;

    bool playerInRange;
    bool meleeRange;
    bool isDead;
    Vector3 playerDir;
    Vector3 startingPos;

    void Start()
    {

        colorOrig = model.material.color;
        GameManager.instance.updateGameGoal(1);
        startingPos = transform.position;
        playerObj = GameObject.FindWithTag("Player");
        stoppingDistOrig = agent.stoppingDistance;
        origY = transform.position.y; // Grab Y
    }

    void Update()
    {
        setAnimations();
        if (agent.remainingDistance < 0.01f)
        {
            roamTime += Time.deltaTime;
        }
        if (playerInRange)
        {
            roamCheck();
        }
        else if (!playerInRange)
        {
            roamCheck();
        }
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
        //set last
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
    }

    void seePlayer()
    {
        if (playerObj == null) return;

        Vector3 playerGroundPOS = new Vector3(playerObj.transform.position.x, origY, playerObj.transform.position.z);
        Vector3 directionPlayer = playerGroundPOS - transform.position;
        Vector3 dirFlat = new Vector3(directionPlayer.x, 0, directionPlayer.z);//Was moving upwards as it got closer to the player

        //Stop in front of player
        if (directionPlayer.magnitude < stopDistPlayer)
        {
            hitAttack();
            //attack animation

        }
        else
        {
            
            //stop attack animation
            transform.position = Vector3.MoveTowards(transform.position, playerGroundPOS, faceTargetSpeed * Time.deltaTime);
        }
        if (dirFlat != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dirFlat);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * facePlayer);
            //run animation
        }
    }

    void deadEnemy()
    {
        if (isDead)
        {
            //play dead animation
            GameManager.instance.updateGameGoal(-1);

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            agent.stoppingDistance = 0;
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        //play get hit animation
        if (!isDead)
        {
            seePlayer();
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    void hitAttack()
    {
       
        //play attack animation
        IDamage player_hit = GameManager.instance.player.GetComponent<IDamage>();
        player_hit.takeDamage(hit_Damage);
    }
    
}
