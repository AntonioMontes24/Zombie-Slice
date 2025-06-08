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


    bool playerInRange;                                     // is our player in range to be chased
    Vector3 playerDirection;                                // Direction of our player
    Color originalColor;                                    // the original color of the enemy. Used when calling flash red.

    // for IDamage
    public void takeDamage(int amount)
    {
        // we need to apply the damage.
        // check for death of the variant
        // and if we have a win condition to kill all enemies, update it

        currHealth -= amount;

        if(currHealth <= 0)
        {
            // we are dead now
            Destroy(gameObject);
        }
        else
        {
            // we still hit the target but it is still alive. 
            flashRed();
        }

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get our original color initialized
        originalColor = model.material.color;


        
    }

    // Update is called once per frame
    void Update()
    {
        // we need to grab the player direction
        // if the player is in our range (sphere collider) turn to face him and send him towards the player
        // we want to make him stop before he collides with the player

        // take the players current position from the game manager and subtract our position
        playerDirection = GameManager.instance.player.transform.position - transform.position;
        // playerDirection = ZGameManager.instance.player.transform.position - transform.position;

        if (playerInRange)
        {
            // set our navmesh agent towards the players position
            agent.SetDestination(GameManager.instance.player.transform.position);
            // agent.SetDestination(ZGameManager.instance.player.transform.position);

            if(agent.remainingDistance <= agent.stoppingDistance)
            {
                // we need to face the player
                faceTarget();
            }
        }

        
    }
    
    IEnumerator flashRed()
    {
        model.material.color = Color.red;                   // set the material color to red to show a hit
        yield return new WaitForSeconds(0.1f);              // wait for 1 tenth of a second to make sure we have a chance to see it
        model.material.color = originalColor;               // set our material color back to original
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
}
