using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class ZVariant1_AI : MonoBehaviour, IDamage
{
    // create some serialized variables
    [SerializeField] int currHealth;                        // the current health 
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

    // attack fields
    bool canAttack;                                          // can we attack
    int attackCounter;                                       // incremented until we hit the attackRate
    [SerializeField] int attackRate;                         // our Attack is on cooldown
    [SerializeField] int attackDamage;                       // how much damage do we do

    public void takeDamage(int amount)
    {
        if (currHealth > 0)
        {
            currHealth -= amount;

            // we took damage so we need to head towards the player
            // set our navmesh agent towards the players position
            // agent.SetDestination(GameManager.instance.player.transform.position);
            agent.SetDestination(ZGameManager.instance.player.transform.position);

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

    }

    IEnumerator attack()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetBool("canAttack", true);

            // wait 1 second
            yield return new WaitForSeconds(1);

            animator.SetBool("canAttack", false);

            // assign damage to the player
            IDamage player_dmg = ZGameManager.instance.player.GetComponent<IDamage>();
            player_dmg.takeDamage(attackDamage);
        }

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currHealth >= 0)
        {
            if (playerInRange && canWeSeeThePlayer())
            {
                attackCounter++;
      
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    // we need to face the player
                    faceTarget();

                    // set my bool for animator
                    animator.SetBool("inMeleeRange", true);

                    // need to check for biteAttack 
                    if (attackCounter >= attackRate)
                    {
                        // reset the counter
                        attackCounter = 0;

                        // animate the bite
                        StartCoroutine(attack());
                    }

                }
                else
                {
                    animator.SetBool("inMeleeRange", false);
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
     
        Quaternion rotate_val = (Quaternion.LookRotation(new Vector3(playerDirection.x, 0, playerDirection.z)));
        transform.rotation = Quaternion.Lerp(transform.rotation, rotate_val, Time.deltaTime * faceTargetSpeed);

    }

    bool canWeSeeThePlayer()
    {
        // take the players current position from the game manager and subtract our position
        playerDirection = ZGameManager.instance.player.transform.position - headPos.position;

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

                agent.SetDestination(ZGameManager.instance.player.transform.position);

                if (animator != null && animator.runtimeAnimatorController != null)
                    animator.SetBool("isWalking", true);

                // face the target
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    faceTarget();
                }

                // we need to return true since we found the player
                return true;
            }

        }

        // we did not hit the player
        animator.SetBool("isWalking", false);
        return false;
    }
}
