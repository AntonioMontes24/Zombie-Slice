using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class LichAI : MonoBehaviour, IDamage, iEnemyHealth
{
    // create some serialized variables
    [SerializeField] int currHealth;                        // the current health
    [SerializeField] int _maxHealth;                        // max health of lich
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

    // blightball fields
    [SerializeField] Transform shootPosition;               // where does our blightball spawn from
    [SerializeField] GameObject blightBall;                 // our projectile
    [SerializeField] int blightBallDuration;                // how long a blightball lasts

    // field for Blightstorms
    [SerializeField] Transform BS1Position;               // where does our blightball spawn from
    [SerializeField] Transform BS2Position;               // where does our blightball spawn from
    [SerializeField] Transform BS3Position;               // where does our blightball spawn from
    [SerializeField] Transform BS4Position;               // where does our blightball spawn from

    // we will check for blight storm then blight ball
    // [SerializeField] int blightBallCounter;                 // increment to see if we can attack
    // [SerializeField] int blightBallRate;                    // our rate of fire
    // [SerializeField] int blightStormCounter;                // increment to see if we can blight storm
    // [SerializeField] int blightStormRate;                   // our rate of fire for blight storm

    // new random attack info
    int attackToUse;
    int attackCounter;
    [SerializeField] int attackRate;

    // the boss has two attacks. 
    // 1. Blightball 
    // 2. Blight Storm

    public int CurrentHealth
    {
        get { return currHealth; }
    }

    public int maxHealth
    {
        get { return _maxHealth; }
    }

    IEnumerator removeCorpse()
    {
        animator.SetTrigger("isDead");

        // wait 2 seconds
        yield return new WaitForSeconds(2);

        Destroy(gameObject);

    }

    IEnumerator shootBlightBall()
    {
        // reset attack counter
        attackCounter = 0;

        // reset the timer set the animation
        // blightBallCounter = 0;
        yield return new WaitForSeconds(0.1f);
        animator.SetTrigger("shootBlightBall");

    }

    IEnumerator shootBlightStorm()
    {
        // reset the attack counter
        attackCounter = 0;

        // reset the timer set the animation
        // blightStormCounter = 0;
        yield return new WaitForSeconds(0.1f);
        animator.SetTrigger("shootBlightstorm");

    }

    public void createBlightBall()
    {
        // create one of our prefabs at the shoot position 
        Instantiate(blightBall, shootPosition.position, transform.rotation);
    }

    public void createBlightStorm()
    {
        // make for blightballs
        Instantiate(blightBall, BS1Position.position, transform.rotation);
        Instantiate(blightBall, BS2Position.position, transform.rotation);
        Instantiate(blightBall, BS3Position.position, transform.rotation);
        Instantiate(blightBall, BS4Position.position, transform.rotation);
    }

    public int chooseAttack()
    {
        return Random.Range(0, 5);

    }  

    public void takeDamage(int amount)
    {
        if (currHealth > 0)
        {
            currHealth -= amount;
            GameManager.instance.UpdateEnemyHealthBar(this);

            if (currHealth <= 0)
            {
                // remove the corpse by destroying the gameObject
                StartCoroutine(removeCorpse());
                GameManager.instance.enemyInfoPanel.SetActive(false);
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currHealth = _maxHealth;
        ObjectiveManager.instance.updateZombieCount(1);

    }

    // Update is called once per frame
    void Update()
    {
        if (currHealth > 0)
        {
            attackCounter++;
            // we are alive to lets check if we can attack

            // we check blight storm before we check blight ball
            if (canWeSeeThePlayer() && playerInRange && attackCounter >= attackRate)
            {
                attackToUse = chooseAttack();

                if(attackToUse >= 2)
                {
                    // we can blight ball
                    StartCoroutine(shootBlightBall());
                }
                else
                {
                    // we can blight storm
                    StartCoroutine(shootBlightStorm());
                }

                
                /*
                // we can see the player
                // can we blight storm
                if (blightStormCounter >= blightStormRate)
                {
                    // we can blight storm
                    StartCoroutine(shootBlightStorm());
                    
                }
                else if (blightBallCounter >= blightBallRate)
                {
                    // we can blight ball
                    StartCoroutine(shootBlightBall());
                    
                }
                else
                {
                    // update our counts
                    blightBallCounter++;
                    blightStormCounter++;
                }
                */
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

                faceTarget();

                // we need to return true since we found the player
                return true;
            }

        }

        return false;
    }
}


