using System.Collections;
//using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ZBodyguardAI : MonoBehaviour, IDamage, iEnemyHealth
{
    // create some serialized variables
    [SerializeField] int currHealth;                        // the current health 
    [SerializeField] int _maxHealth;                         // the maximum or starting health of the enemy
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
    // int punchCounter;                                       // keeps count until we can punch
    // [SerializeField] int punchRate;                         // how often we punch
    [SerializeField] int punchDamage;                       // how hard we punch

    // combo attack
    // int comboAttackCounter;                                 // keep count until we can combo
    // [SerializeField] int comboAttackRate;                   // how often we combo attack
    [SerializeField] int comboAttackDamage;                 // how much damage does a combo do   

    

    [SerializeField] int roamDistance;                      // max distance he can roam from start position
    [SerializeField] int roamStopTimer;                     // how long before he roams again
    Vector3 startingPostion;                                // where his spawn position is
    float roamTime;                                         // counter to see if he can roam 
    float stoppingDistanceOriginal;                         // cache off the original stopping distance

    [SerializeField] int animSpeedTransition;               // for the LERP on transitions

    int attackCounter;                                       // incremented until we hit the attackRate
    [SerializeField] int attackRate;                         // our Attack is on cooldown
    [SerializeField] int attackDamage;                       // how much damage do we do
    int attackToDo;

    // sound
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip aud_clip_idle;
    [SerializeField] AudioClip aud_clip_attack;
    [SerializeField] AudioClip aud_clip_punch;
    [SerializeField] AudioClip aud_clip_combo;
    [SerializeField] AudioClip aud_clip_death;
    [SerializeField] int audioCounter;

    public int CurrentHealth
    {
        get { return currHealth; }
    }

    public int maxHealth
    {
        get { return _maxHealth; }
    }


    // for IDamage
    public void takeDamage(int amount)
    {
        // we need to apply the damage.
        // check for death of the variant
        // and if we have a win condition to kill all enemies, update it
        if (currHealth >= 0)
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

    IEnumerator removeCorpse()
    {
        agent.isStopped = true;

        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetTrigger("isDead");
        // wait 2 seconds
        yield return new WaitForSeconds(4);

        Destroy(gameObject);

        // update the number of zombies left in stage
        ObjectiveManager.instance.updateZombieCount(-1);
    }

    IEnumerator punchAttack()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            attackCounter = 0;

            animator.SetTrigger("punch");

            // IDamage player_dmg = GameManager.instance.player.GetComponent<IDamage>();
            // player_dmg.takeDamage(punchDamage);

            yield return new WaitForSeconds(1);

            

        }

    }

    IEnumerator comboAttack()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            attackCounter = 0;

            animator.SetTrigger("ComboAttack");

            // IDamage player_dmg = GameManager.instance.player.GetComponent<IDamage>();
            // player_dmg.takeDamage(comboAttackDamage);

            yield return new WaitForSeconds(5);

            // player_dmg.takeDamage(comboAttackDamage);
        }

    }

    public void assignComboDamage()
    {
        IDamage player_dmg = GameManager.instance.player.GetComponent<IDamage>();
        player_dmg.takeDamage(comboAttackDamage);
    }

    public void assignPunchDamage()
    {
        IDamage player_dmg = GameManager.instance.player.GetComponent<IDamage>();
        player_dmg.takeDamage(punchDamage);
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
        
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // increase the number of zombies left in stage
        ObjectiveManager.instance.updateZombieCount(1);

        // set our HP variables for the health bar
        currHealth = _maxHealth;

        // set our starting position so that we know how far we can roam. This is the point we will check from
        startingPostion = transform.position;
        // set our stopping distance to the stopping distance in Unity
        stoppingDistanceOriginal = agent.stoppingDistance;

        // start with a disabled claw
        clawCollider.enabled = false;

        audioCounter = 399;
        attackCounter = attackRate - 1;

    }

    public int chooseAttack()
    {
        return Random.Range(0, 10);
    }

    public void playAudioPunch()
    {
        if (aud_clip_punch != null)
            aud.PlayOneShot(aud_clip_punch);
    }

    public void playAudioCombo()
    {
        if (aud_clip_combo != null)
            aud.PlayOneShot(aud_clip_combo);
    }

    public void playAudioIdle()
    {
        if (aud_clip_idle != null)
            aud.PlayOneShot(aud_clip_idle);
    }

    public void playAudioDeath()
    {
        aud.Stop();
        if (aud_clip_death != null)
            aud.PlayOneShot(aud_clip_death);
    }

    // Update is called once per frame
    void Update()
    {
        if (currHealth >= 0)
        {
            
            if (playerInRange && canWeSeeThePlayer())
            {
                // handle audio
                if (audioCounter >= 400)
                {
                    audioCounter = 0;

                    aud.Stop();

                    // play sound
                    if (aud_clip_attack != null)
                        aud.PlayOneShot(aud_clip_attack);

                }
                else
                {
                    audioCounter++;
                    // attackCounter++;
                }

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    // we need to face the player
                    faceTarget();

                    // can we attack
                    if (attackCounter >= attackRate)
                    {

                        // choose an attack 
                        attackToDo = chooseAttack();

                        if (attackToDo >= 4)
                        {
                            // animate the swipe
                            StartCoroutine(punchAttack());

                        }
                        // need to check for combo
                        else
                        {
                            // animate the bite
                            StartCoroutine(comboAttack());

                        }
                    }
                    else
                    {
                        // no attack but increment the counter
                        attackCounter++;
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
        animator.SetFloat("Speed", 0);

        return false;
    }

   
}
