using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class BossAI : MonoBehaviour, IDamage, iEnemyHealth
{
    // create some serialized variables
    [SerializeField] int currHealth;                        // the current health
    [SerializeField] int _maxHealth;                        // max health
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
    float angle_to_player;                                  // the angle to the player 
   
    // power attack fields
    bool canPowerAttack;                                    // can we power attack
    int powerAttackCounter;                                 // increment to be able to power attack
    [SerializeField] int powerAttackRate;                   // how often we can power attack
    [SerializeField] int powerAttackDamage;                 // how much damage does power attack do

    // blightball fields
    [SerializeField] Transform shootPosition;               // where does our blightball spawn from
    [SerializeField] GameObject blightBall;                 // our projectile
    bool canShootBlight;                                    // can we shoot our blight ball
    int blightBallCounter;                                  // increment to be able to shoot our blight ball
    // [SerializeField] int blightBallRate;                    // how often we can shoot our blight ball
    [SerializeField] int blightBallDamage;                  // how much damage does a blight ball do
    [SerializeField] float minShootDistance;                // minimum range to shoot blightball

    [SerializeField] Collider clawCollider;                 // Collider for the power attack
    int attackToUse;
    int attackCounter;
    [SerializeField] int attackRate;

    public bool see_player;

    public int CurrentHealth
    {
        get { return currHealth; }
    }

    public int maxHealth
    {
        get { return _maxHealth; }
    }

    public int chooseAttack()
    {
        return Random.Range(0, 5);

    }

    // coroutines 
    IEnumerator powerAttack()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            attackCounter = 0;

            animator.SetTrigger("Attack");

            // wait 1 second
            yield return new WaitForSeconds(1);

        }
    }

    public void assignPowerAttackDamage()
    {
        // assign damage to the player
        IDamage player_dmg = GameManager.instance.player.GetComponent<IDamage>();
        player_dmg.takeDamage(powerAttackDamage);
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

    IEnumerator removeCorpse()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetTrigger("isDead");

        animator.SetFloat("Speed", 0);

        clawColliderOff();
        agent.enabled = false;
        
        yield return new WaitForSeconds(4.0f);
        Destroy(gameObject);

        
    }

    IEnumerator shootBlightBall()
    {
        attackCounter = 0;
        animator.SetTrigger("Blightball");
        // wait 1 second
        yield return new WaitForSeconds(1);

    }
    public void createBlightBall()
    {
        // create one of our prefabs at the shoot position 
        Instantiate(blightBall, shootPosition.position, transform.rotation);
    }

    public void takeDamage(int amount)
    {
        if (currHealth > 0)
        {
            currHealth -= amount;

            GameManager.instance.UpdateEnemyHealthBar(this);

            // we took damage so we need to head towards the player
            // set our navmesh agent towards the players position
            // agent.SetDestination(GameManager.instance.player.transform.position);
            agent.SetDestination(GameManager.instance.player.transform.position);
            animator.SetFloat("Speed", 1);

            if (currHealth <= 0)
            {
                // remove the corpse by destroying the gameObject
                StartCoroutine(removeCorpse());
                GameManager.instance.enemyInfoPanel.SetActive(false);
            }
        }
    }

    private void Awake()
    {
        currHealth = _maxHealth;
        minShootDistance = agent.stoppingDistance + 20.0f;
        // increase the number of zombies left in stage
        ObjectiveManager.instance.updateZombieCount(1);

        attackCounter = attackRate - 1;

        see_player = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        if (currHealth > 0)
        {
            see_player = canWeSeeThePlayer();

            if (playerInRange && canWeSeeThePlayer())
            {
                attackCounter++;
                if(canWeSeeThePlayer() && playerInRange && attackCounter >= attackRate)
                {
                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        // we need to face the player
                        faceTarget();

                        // melee attack
                        StartCoroutine(powerAttack());
                    }
                    else
                    {
                       // ranged attack
                       StartCoroutine(shootBlightBall());
                    }
                }
 
            }
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
                animator.SetFloat("Speed", 1);


                // face the target
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    faceTarget();
                    animator.SetFloat("Speed", 0);
                }

                // we need to return true since we found the player
                return true;
            }


        }

        return false;
    }
}
