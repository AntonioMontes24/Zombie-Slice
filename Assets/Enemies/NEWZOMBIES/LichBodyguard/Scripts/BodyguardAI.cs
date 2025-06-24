using System.Collections;
//using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ZBodyguardAI : MonoBehaviour, IDamage
{
    // create some serialized variables
    [SerializeField] int currHealth;                        // the current health 
    [SerializeField] int maxHealth;                         // the maximum or starting health of the enemy
    [SerializeField] float speed;                           // the speed when walking normally
    [SerializeField] float speedModifier;                   // the modifier if running or slowed
    [SerializeField] int faceTargetSpeed;                   // how fast he faces the target when not moving
    [SerializeField] Renderer model;                        // Model we will use when we flash a new color on hit or when damaged.
    [SerializeField] NavMeshAgent agent;                    // NavMeshAgent to traverse our navmesh
    [SerializeField] Animator animator;                     // this will handle our animations

    [SerializeField] bool playerInRange;                    // is our player in range to be chased
    Vector3 playerDirection;                                // Direction of our player
    [SerializeField] int field_of_view;                     // the number of degrees our of enemy can see
    [SerializeField] Transform headPos;                     // for raycast to player. Can he see where we are at
    [SerializeField] Transform clawPos;                     // where are claws are for damage and hits
    [SerializeField] Collider clawCollider;                 // collider for the claw

    float angle_to_player;                                  // the angle to the player 

    // punch attack 
    int punchCounter;                                       // keeps count until we can punch
    [SerializeField] int punchRate;                         // how often we punch
    [SerializeField] int punchDamage;                       // how hard we punch

    // combo attack
    int comboAttackCounter;                                 // keep count until we can combo
    [SerializeField] int comboAttackRate;                   // how often we combo attack
    [SerializeField] int comboAttackDamage;                 // how much damage does a combo do   

    [SerializeField] EnemyHealthBar healthBar;              // healthbar to be shown above enemy

    [SerializeField] int roamDistance;                      // max distance he can roam from start position
    [SerializeField] int roamStopTimer;                     // how long before he roams again
    Vector3 startingPostion;                                // where his spawn position is
    float roamTime;                                         // counter to see if he can roam 
    float stoppingDistanceOriginal;                         // cache off the original stopping distance

    // for IDamage
    public void takeDamage(int amount)
    {
        // we need to apply the damage.
        // check for death of the variant
        // and if we have a win condition to kill all enemies, update it
        if (currHealth >= 0)
        {
            currHealth -= amount;

            healthBar.updateHealthBar(currHealth, maxHealth);

            // we took damage so we need to head towards the player
            // set our navmesh agent towards the players position
            // agent.SetDestination(GameManager.instance.player.transform.position);
            agent.SetDestination(GameManager.instance.player.transform.position);

            if (currHealth <= 0)
            {
                // remove the corpse by destroying the gameObject
                StartCoroutine(removeCorpse());
            }
        }
    }

    IEnumerator removeCorpse()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetTrigger("isDead");
        // wait 2 seconds
        yield return new WaitForSeconds(2);

        Destroy(gameObject);

        // update the number of zombies left in stage
        GameManager.instance.updateGameGoal(-1);
    }

    public void punchAttack()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            punchCounter = 0;

            animator.SetTrigger("punch");

            IDamage player_dmg = GameManager.instance.player.GetComponent<IDamage>();
            player_dmg.takeDamage(punchDamage);

        }

    }

    public void comboAttack()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            comboAttackCounter = 0;

            animator.SetTrigger("ComboAttack");

            IDamage player_dmg = GameManager.instance.player.GetComponent<IDamage>();
            player_dmg.takeDamage(comboAttackDamage);
        }

    }

    public void clawColliderOn()
    {
        if (clawCollider)
            clawCollider.enabled = true;
    }

    public void clawColliderOff()
    {
        if (clawCollider)
            clawCollider.enabled = false;
    }

    private void Awake()
    {
        healthBar = GetComponentInChildren<EnemyHealthBar>();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // increase the number of zombies left in stage
        GameManager.instance.updateGameGoal(1);

        // set our HP variables for the health bar
        currHealth = maxHealth;
        healthBar.updateHealthBar(currHealth, maxHealth);
        // update the enemy UI / Healthbar
        updateEnemyUI();

        // set our starting position so that we know how far we can roam. This is the point we will check from
        startingPostion = transform.position;
        // set our stopping distance to the stopping distance in Unity
        stoppingDistanceOriginal = agent.stoppingDistance;

    }

    // Update is called once per frame
    void Update()
    {
        comboAttackCounter++;
        punchCounter++;

        if (currHealth >= 0)
        {
            // check if we need to increment our roam or just roam
            if (agent.remainingDistance < 0.01f)
            {
                roamTime += Time.deltaTime;
            }
            if (playerInRange && !canWeSeeThePlayer())
            {
                roamCheck();
            }
            else if (!playerInRange)
            {
                roamCheck();
            }

            if (playerInRange && canWeSeeThePlayer())
            {
  
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    // we need to face the player
                    faceTarget();

                    // set my bool for animator
                    animator.SetBool("inMeleeRange", true);

                    // need to check for biteAttack 
                    if (punchCounter >= punchRate)
                    {

                        // animate the bite
                        punchAttack();

                    }

                    // need to check for swipeAttack 
                    if (comboAttackCounter >= comboAttackRate)
                    {
                        // animate the swipe
                        comboAttack();

                    }

                }
                
            }
        }

    }

    void roam()
    {
        // reset the timer
        roamTime = 0;

        // make sure he is able to get to the location and not stop short
        agent.stoppingDistance = 0;

        // grab a random spot in our sphere on the navmesh
        Vector3 randPos = Random.insideUnitSphere * roamDistance;
        randPos += startingPostion;

        // check if the position is on the navmesh
        NavMeshHit hit;
        NavMesh.SamplePosition(randPos, out hit, roamDistance, 1);

        // move
        agent.SetDestination(hit.position);
    }

    void roamCheck()
    {
        // can i roam and am I stopped. 
        if (roamTime >= roamStopTimer && agent.remainingDistance < 0.1f)
        {
            roam();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // the other object in range is Player
        if (other.CompareTag("Player"))
        {
            // player is in range! 
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // the other object leaving range is Player
        if (other.CompareTag("Player"))
        {
            // player left our range
            playerInRange = false;
        }
    }

    void faceTarget()
    {
        // turn our enemy towards the player when he is not moving
        // we need a direction not a position to rotate, example a position - a position

        Quaternion rotate_val = (Quaternion.LookRotation(new Vector3(playerDirection.x, 0, playerDirection.z)));
        transform.rotation = Quaternion.Lerp(transform.rotation, rotate_val, Time.deltaTime * faceTargetSpeed);

    }

    bool canWeSeeThePlayer()
    {
        // take the players current position from the game manager and subtract our position
        playerDirection = GameManager.instance.player.transform.position - headPos.position;

        // get our angle to the player
        angle_to_player = Vector3.Angle(playerDirection, transform.forward);

        // make our Raycast to the player. If it hits the player, we have line of sight. 
        RaycastHit hit_player;

        if (Physics.Raycast(headPos.position, playerDirection, out hit_player))
        {
            // check if its the player we hit
            if (angle_to_player <= field_of_view && hit_player.collider.CompareTag("Player"))
            {
                // we hit the player with the raycast and he is in our field of view
                // agent.SetDestination(GameManager.instance.player.transform.position);

                agent.SetDestination(GameManager.instance.player.transform.position);

               
                // face the target
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    faceTarget();
                }

                // we need to return true since we found the player
                return true;
            }


        }

        return false;
    }

    void updateEnemyUI()
    {

    }
}
