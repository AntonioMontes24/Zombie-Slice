using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] int HP; //Enemy HP variable
    [SerializeField] int facePlayerSpeed;
    [SerializeField] Transform zHeadPOS; //Zombie 

    [SerializeField] int FOV; //Field of view
    [SerializeField] LayerMask playerLayer;

    public Animator anim; //Animator controller
    int hitCount; //Counts number of hits to zombie
    Color colorOrig; //Original color for hit feedback

    bool playerInRange; //Will check to see if player in range for attack

    Vector3 playerDir; //Will be used to direct towards player

    float angleToTarget;
    public GameObject playerObject;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        anim = GetComponent<Animator>();
        if (playerObject == null)
        {
            playerObject = GameObject.FindWithTag("Player");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (HP >= 1)
        {
            //float dist = Vector3.Distance(transform.position, playerObject.transform.position);
            if (playerInRange && PlayerChase())
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    FacePlayer();
                    StartCoroutine(zombieAttack());
                    z1Attack();

                }
                // //Check if enemy is within melee attack range - not working
                // if (dist < 2)
                // {
                //     z1Attack();//Trigger attack animation
                // }
                // else if (dist > 2)
                // {
                //     anim.SetBool("AttackRange", false);//Trigger animator attack to stop until back in range
                //     agent.isStopped = false;
                // }
                else
                {
                    anim.SetBool("AttackRange", false);
                }

            }
        }
    }


    /*MOVEMENT*/
    bool PlayerChase()
    {

        playerDir = GameManager.instance.player.transform.position - zHeadPOS.position;
        angleToTarget = Vector3.Angle(playerDir, transform.forward);
        UnityEngine.Debug.DrawRay(zHeadPOS.position, playerDir);
        RaycastHit hit;
        if (Physics.Raycast(zHeadPOS.position, playerDir, out hit))

        {
            if (angleToTarget <= FOV && hit.collider.CompareTag("Player"))
            {
                //agent.SetDestination(GameManager.instance.player.transform.position);
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    FacePlayer();
                    //StartCoroutine(targetSighted());
                }
                return true;
            }
        }
        return false;
    }
    void FacePlayer()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * facePlayerSpeed);
    }

    //bool zMoving;
    bool isDead;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            StartCoroutine(zIdle());
        }
    }
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         playerInRangeFar = true;
    //         //agent.isStopped = false;
    //         anim.SetBool("playerInRangeClose", true);//Trigger walk animation


    //     }
    // }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            //agent.isStopped = true;
            StartCoroutine(zIdle());
        }
    }


    /*CoRoutines for animation*/
    //Start movement
    IEnumerator zIdle()
    {
        anim.SetBool("isMoving", false);
        yield return new WaitForSeconds(0.1f);
        agent.isStopped = true;

    }
    IEnumerator zombieAttack()
    {
        anim.SetBool("AttackRange", true);
        yield return new WaitForSeconds(0.1f);
    }
    //Flash red when hit
    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        anim.SetTrigger("Hit");
        FacePlayer();
        yield return new WaitForSeconds(0.1f);
        // agent.SetDestination(GameManager.instance.player.transform.position);
        model.material.color = colorOrig;
        agent.isStopped = true;

        yield return StartCoroutine(zombieDamaged());
    }
    //Play hit animation when taking damage
    IEnumerator deathAnim()
    {
        anim.SetInteger("isDead", 1);
        agent.speed = 0;//failsafe to make sure zombie stops moving
        GetComponent<Collider>().enabled = false;//Prevent shooting dead target
        //agent.SetDestination(agent.transform.position);
        //playerInRange = false;
        yield return new WaitForSeconds(3.0f);//Play death animation then remove from area (destroy object) after 3 seconds. Could remove this if we want to have bodies remain
        zDead();
    }

   

    IEnumerator targetSighted()//Will move if target is sighted. 
    {
        anim.SetBool("isMoving", true);
        yield return new WaitForSeconds(0.1f);
        anim.SetBool("isMoving", true);
    }

    IEnumerator zombieDamaged()
    {
        if (!isDead)
        {

            anim.SetBool("isMoving", false);
            agent.isStopped = true;
            yield return new WaitForSeconds(0.1f);
            anim.SetInteger("HP", HP);
            if (hitCount > 1)
            {
                anim.SetBool("isMoving", false);
                anim.SetBool("isRunning", true);
            }
            else
            {
                anim.SetBool("isRunning", false);
                anim.SetBool("isMoving", true);
            }
            agent.isStopped = false;
            agent.SetDestination(gameObject.transform.position);
        }
        if (isDead)
        {
            anim.SetBool("isMoving", false);
            yield return StartCoroutine(deathAnim());
            zDead();

        }
    }
    //Intention is to set the distance parameter in the animator to 0 to trigger attack animation
    private void zDead()
    {

        Destroy(gameObject);
    }
    private void z1Attack()
    {
        agent.isStopped = true;
        anim.SetBool("AttackRange", true);
    }

    public void takeDamage(int amount)
    {

        HP -= amount; //Reduce enemy HP by amount of damage taken from player/weapon
        hitCount++;
        anim.SetInteger("hitCount", hitCount);
        agent.SetDestination(GameManager.instance.player.transform.position);
        statusCheck(HP);//Check current HP value and toggle isDead bool

        StartCoroutine(flashRed());
    }

    private void statusCheck(int HP)
    {
        if (HP <= 0)
        {
            isDead = true;
            anim.SetInteger("isDead", 1);
        }
        else if (HP > 1)
        {
            isDead = false;
            anim.SetInteger("isDead", 0);
        }
    }
    //Future possibility is to slow the enemy down with each hit

}
