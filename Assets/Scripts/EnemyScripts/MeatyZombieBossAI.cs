using System.Collections;
//using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


public class MeatyZombieBossAI : MonoBehaviour, IDamage, iEnemyHealth
{

    // create some serialized variables
    [SerializeField] int currHealth;                        // the current health
    [SerializeField] int _maxHealth;                        // max health 
    float health_percentage;

    [SerializeField] float speed;                           // the speed when walking normally
    [SerializeField] float speedModifier;                   // the modifier if running or slowed
    [SerializeField] int faceTargetSpeed;                   // how fast he faces the target when not moving
    

    [SerializeField] Renderer model;                        // Model we will use when we flash a new color on hit or when damaged.
    [SerializeField] NavMeshAgent agent;                    // NavMeshAgent to traverse our navmesh
    [SerializeField] Animator animator;                     // this will handle our animations

    
    Vector3 playerDirection;                                // Direction of our player
    [SerializeField] int field_of_view;                     // the number of degrees our of enemy can see
    [SerializeField] Transform headPos;                     // for raycast to player. Can he see where we are at
    float angle_to_player;                                  // the angle to the player 

    // blightball fields
    [SerializeField] Transform shoot_pos1;
    [SerializeField] Transform shoot_pos2;
    [SerializeField] Transform shoot_pos3;
    [SerializeField] Transform shoot_pos4;

    [SerializeField] Transform shoot_pos5;
    [SerializeField] Transform shoot_pos6;
    [SerializeField] Transform shoot_pos7;
    [SerializeField] Transform shoot_pos8;

    [SerializeField] Collider fist_collider;
    [SerializeField] private string outroScene = "OutroScene";
    

    [SerializeField] GameObject blightBall;                 // our projectile
    [SerializeField] int blightBallDuration;                // how long a blightball lasts

    Vector3 initial_start_position;
    Vector3 players_last_location;

    [SerializeField] int search_distance;
    bool executing_phase;
    int current_phase;

    bool has_moved_to_spawn;

    [SerializeField] Transform[] blight_pool_spawns;
    
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip fight_start;
    [SerializeField] AudioClip aud_clip_engaged;
    [SerializeField] AudioClip aud_clip_leap;
    [SerializeField] AudioClip aud_clip_roar;
    [SerializeField] AudioClip aud_clip_charge;
    [SerializeField] AudioClip aud_clip_heavy_attack;
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
    public void takeDamage(int amount)
    {
        if (currHealth >= 0)
        {
            currHealth -= amount;

            if (GameManager.instance != null)
            {
                GameManager.instance.UpdateEnemyHealthBar(this);
                if (GameManager.instance.enemyInfoPanel != null)
                {
                    GameManager.instance.enemyInfoPanel.SetActive(true);
                }
            }

            if (currHealth <= 0)
            {
                // remove the corpse by destroying the gameObject
                StartCoroutine(removeCorpse());
                GameManager.instance.enemyInfoPanel.SetActive(false);
            }
        }
    }

    public void fistColliderOn()
    {
        if (fist_collider)
            fist_collider.enabled = true;
    }

    public void fistColliderOff()
    {
        if (fist_collider)
            fist_collider.enabled = false;
    }

    public void assignHeavyAttackDamage()
    {
        // assign damage to the player
    }

    public void assignChargeDamage()
    {
        // assign damage to the player
    }

    IEnumerator walkToSpot()
    {
        // choose a spot and move to it
        // grab a random spot in our sphere on the navmesh
        Vector3 randPos = Random.insideUnitSphere * search_distance;
        randPos += initial_start_position;

        // check if the position is on the navmesh
        NavMeshHit hit;
        if(NavMesh.SamplePosition(randPos, out hit, search_distance, 1))
            agent.SetDestination(hit.position);
            agent.isStopped = false;

        // NavMesh.SamplePosition(randPos, out hit, search_distance, 1);

        animator.SetFloat("Speed", 1);

        aud.PlayOneShot(aud_clip_engaged);

        yield return new WaitForSeconds(3);

    }


    IEnumerator heavyAttack()
    {
        // animate the swing 
        // play sound

        if (aud_clip_heavy_attack != null)
            aud.PlayOneShot(aud_clip_heavy_attack);

        animator.SetTrigger("HeavyAttack");

        yield return new WaitForSeconds(3);

    }

    public void fireBlightBalls()
    {
        float angle = 0;
        Quaternion blightball_rotation = Quaternion.Euler(0f, angle, 0);
        Instantiate(blightBall, shoot_pos1.position, blightball_rotation);

        angle += 45;
        blightball_rotation = Quaternion.Euler(0f, angle, 0);
        Instantiate(blightBall, shoot_pos2.position, blightball_rotation);

        angle += 45;
        blightball_rotation = Quaternion.Euler(0f, angle, 0);
        Instantiate(blightBall, shoot_pos3.position, blightball_rotation);

        angle += 45;
        blightball_rotation = Quaternion.Euler(0f, angle, 0);
        Instantiate(blightBall, shoot_pos4.position, blightball_rotation);

        angle += 45;
        blightball_rotation = Quaternion.Euler(0f, angle, 0);
        Instantiate(blightBall, shoot_pos5.position, blightball_rotation);

        angle += 45;
        blightball_rotation = Quaternion.Euler(0f, angle, 0);
        Instantiate(blightBall, shoot_pos6.position, blightball_rotation);

        angle += 45;
        blightball_rotation = Quaternion.Euler(0f, angle, 0);
        Instantiate(blightBall, shoot_pos7.position, blightball_rotation);

        angle += 45;
        blightball_rotation = Quaternion.Euler(0f, angle, 0);
        Instantiate(blightBall, shoot_pos8.position, blightball_rotation);
    }

    public void spawnBlightPool()
    {
        // this will spawn the blight pool after the leap
    }

    public void spawnMultipleBlightPools(Transform[] spawn_points)
    {
        // this will spawn blightpools in phase 2 
    }

    public void checkForPhase()
    {
        // get or current health percentage
        health_percentage = currHealth * 100 / maxHealth ;

        if(health_percentage > 80)
        {
            current_phase = 1;
        }
        else if (health_percentage > 40)
        {
            current_phase = 2;
        }
        else 
        { 
            current_phase = 3;
        }
    }

    IEnumerator chargeAttack()
    {
      
        players_last_location = GameManager.instance.player.transform.position;

        Vector3 targetDirection = GameManager.instance.player.transform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3 (targetDirection.x, 0, targetDirection.z));
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 3.0f * Time.deltaTime);

        // while was here
        agent.SetDestination(players_last_location);
        animator.SetFloat("Speed", 1);
        
           
        yield return new WaitForSeconds(4);
    }

    IEnumerator leapAttack()
    {
        // boss will choose a random spot on the navmesh and move to it. 
        // boss will leap from the spot and leave a blight pool . 
        // when he lands he will fire off 12 blightballs in the same directions as the numbers on a clock. 

        // now we leap 
        animator.SetTrigger("Leap");

        aud.PlayOneShot(aud_clip_leap);

        yield return new WaitForSeconds(4);

    }

    IEnumerator moveToSpawn()
    {
        // move to the spawn location
        // move
        agent.SetDestination(initial_start_position);
        animator.SetFloat("Speed", 1);

        yield return new WaitForSeconds(4);

        
    }

    IEnumerator roar()
    {
        if(aud_clip_roar != null)
            aud.PlayOneShot(aud_clip_roar);

        animator.SetTrigger("Roar");

        
        yield return new WaitForSeconds(4);

        
    }

    IEnumerator removeCorpse()
    {
        animator.enabled = false;

        // Enable the Animator
        animator.enabled = true;

        //Rebind the animator and update it
        animator.Rebind();
        animator.Update(0f);

        animator.SetTrigger("isDead");

        aud.Stop();

        if (aud_clip_death != null)
            aud.PlayOneShot(aud_clip_death);

        // wait 2 seconds
        yield return new WaitForSeconds(5);


        Destroy(gameObject);
        SceneManager.LoadScene(outroScene);
    }

    IEnumerator startOfFight()
    {
        // boss will have an animation, maybe say something and move to phase 1 
        if(fight_start != null)
           aud.PlayOneShot(fight_start);

        fistColliderOff();

        yield return new WaitForSeconds(2);

        has_moved_to_spawn = false;

        // set the current phase to 1
        current_phase = 1;
    }

    IEnumerator Phase1()
    {
        // boss will be in this state from 100% - 71% health.
        // Boss will choose a random spot
        // move to the spot 
        // leap animation
        // leave a blight pool at landing spot and fire off 12 blight balls in a circle around him

        // set our bool so that we know we are executing a state
        executing_phase = true;

        StartCoroutine(walkToSpot());

        yield return new WaitForSeconds(2);

        StartCoroutine(leapAttack());

        yield return new WaitForSeconds(4);

        executing_phase = false;

        // current_phase = 2;
    }

    IEnumerator Phase2()
    {
        // Boss will be in this state from 70% - 40% health
        // Boss will move to his original spawn location
        // Roar animation and sound
        // spawn 8 blight pools 
        // repeat until 40% health

        // set our bool so that we know we are executing a state
        executing_phase = true;

        
        animator.SetFloat("Speed", 0);
        StartCoroutine(roar());
        
        yield return new WaitForSeconds(4);

        // rotate the enemy
        float angle = 1;
        int offset = Random.Range(1, 360);
        angle = offset * angle;

        Quaternion roar_rotation = Quaternion.Euler(0f, angle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, roar_rotation, Time.deltaTime * 3);

        executing_phase = false;

        // current_phase = 3;
    }

    IEnumerator Phase3()
    {
        // Boss will be in this state from 40% until dead
        // Boss will use phase 1 attacks until his sound counter is above its threshold. 
        // when sound threshold is hit .. it is reset and boss will charge at the player and power attack for big damage. 
        // repeat this process until dead.
        executing_phase = true;

      
        StartCoroutine(chargeAttack());


       
        StartCoroutine(heavyAttack());

        yield return new WaitForSeconds(4);

        executing_phase = false;
    }

    private void Awake()
    {
        // set our data here
        
        currHealth = maxHealth;
        health_percentage = currHealth / maxHealth;

        initial_start_position = transform.position;
        executing_phase = false;
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(startOfFight());
    }

    // Update is called once per frame
    void Update()
    {
        if (currHealth > 0)
        {
            // check what phase we are in based off our health percentage
            checkForPhase();

            switch(current_phase)
            {
                case 1:
                    {
                        if(!executing_phase)
                        {
                            StartCoroutine(Phase1());
                        }
                        
                        break;
                    }
                case 2:
                    {
                        if(!executing_phase)
                        {
                            StartCoroutine(Phase2());
                        }
                        
                        break;
                    }
                case 3:
                    {
                        if(!executing_phase)
                        {
                            StartCoroutine(Phase3());
                        }
                        
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }
}
