using System.Collections;
using UnityEngine;
using UnityEngine.AI;



public class ZombieVariant2AI : MonoBehaviour, IDamage
{
    // create some serialized variables
    [SerializeField] int currHealth;                        // the current health 
    [SerializeField] float speed;                           // the speed when walking normally
    [SerializeField] float speedModifier;                   // the modifier if running or slowed
    [SerializeField] int faceTargetSpeed;                   // how fast he faces the target when not moving
    [SerializeField] Renderer model;                        // Model we will use when we flash a new color on hit or when damaged.
    [SerializeField] NavMeshAgent agent;                    // NavMeshAgent to traverse our navmesh


    [SerializeField] bool playerInRange;                    // is our player in range to be chased
    Vector3 playerDirection;                                // Direction of our player
    [SerializeField] int field_of_view;                     // the number of degrees our of enemy can see
    [SerializeField] Transform headPos;                     // for raycast to player. Can he see where we are at
    float angle_to_player;                                  // the angle to the player 

    // for IDamage
    public void takeDamage(int amount)
    {
        // we need to apply the damage.
        // check for death of the variant
        // and if we have a win condition to kill all enemies, update it

        currHealth -= amount;

        // we took damage so we need to head towards the player
        // set our navmesh agent towards the players position
        // agent.SetDestination(GameManager.instance.player.transform.position);
        agent.SetDestination(ZGameManager.instance.player.transform.position);

        if (currHealth <= 0)
        {
            // we are dead now
            Destroy(gameObject);
        }  

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
       
        if (playerInRange && canWeSeeThePlayer())
        {
            
           
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                // we need to face the player
                faceTarget();
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
        playerDirection = ZGameManager.instance.player.transform.position - headPos.position;

        // get our angle to the player
        angle_to_player = Vector3.Angle(playerDirection, transform.forward);

        // make our Raycast to the player. If it hits the player, we have line of sight. 
        RaycastHit hit_player;

        if(Physics.Raycast(headPos.position, playerDirection, out hit_player))
        {
            // check if its the player we hit
            if(angle_to_player <= field_of_view && hit_player.collider.CompareTag("Player"))
            {
                // we hit the player with the raycast and he is in our field of view
                // agent.SetDestination(GameManager.instance.player.transform.position);
                agent.SetDestination(ZGameManager.instance.player.transform.position);

                // update any attack countdown 

                // check our attackcountdown to the attack rate here

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
        return false;
    }
}
