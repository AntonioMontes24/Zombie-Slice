using System.Collections;
//using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;


public class MeatyZombieBossAI : MonoBehaviour, IDamage, iEnemyHealth
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

    
    Vector3 playerDirection;                                // Direction of our player
    [SerializeField] int field_of_view;                     // the number of degrees our of enemy can see
    [SerializeField] Transform headPos;                     // for raycast to player. Can he see where we are at
    float angle_to_player;                                  // the angle to the player 

    // blightball fields
    [SerializeField] Transform [] shootPositions;               // where does our blightball spawn from
    [SerializeField] GameObject blightBall;                 // our projectile
    [SerializeField] int blightBallDuration;                // how long a blightball lasts

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
        throw new System.NotImplementedException();
    }

    public void assignHeavyAttackDamage()
    {
        // assign damage to the player
    }

    public void assignChargeDamage()
    {
        // assign damage to the player
    }

    


    public void heavyAttack()
    {
        // animate the swing 
        // play sound
    }

    public void fireBlightBalls()
    {
        // this will fire the 12 blight balls after the leap

    }

    public void spawnBlightPool()
    {
        // this will spawn the blight pool after the leap
    }

    public void spawnMultipleBlightPools(Transform[] spawn_points)
    {
        // this will spawn blightpools in phase 2 
    }

    IEnumerator chargeAttack()
    {
        // this will find the players location
        // reset the sound meter to 0
        // run towards the players location
        // when we arrive we do a heavy attack

        yield return new WaitForSeconds(4);
    }

    IEnumerator leapAttack()
    {
        // boss will choose a random spot on the navmesh and move to it. 
        // boss will leap from the spot and leave a blight pool . 
        // when he lands he will fire off 12 blightballs in the same directions as the numbers on a clock. 


        yield return new WaitForSeconds(4);
    }

    IEnumerator removeCorpse()
    {
        yield return new WaitForSeconds(4);
    }

    IEnumerator startOfFight()
    {
        // boss will have an animation, maybe say something and move to phase 1 

        yield return new WaitForSeconds(4);
    }

    IEnumerator Phase1()
    {
        // boss will be in this state from 100% - 71% health.
        // Boss will choose a random spot
        // move to the spot 
        // leap animation
        // leave a blight pool at landing spot and fire off 12 blight balls in a circle around him


        yield return new WaitForSeconds(4);
    }

    IEnumerator Phase2()
    {
        // Boss will be in this state from 70% - 40% health
        // Boss will move to his original spawn location
        // Roar animation and sound
        // spawn 8 blight pools 
        // repeat until 40% health

        yield return new WaitForSeconds(4);
    }

    IEnumerator Phase3()
    {
        // Boss will be in this state from 40% until dead
        // Boss will use phase 1 attacks until his sound counter is above its threshold. 
        // when sound threshold is hit .. it is reset and boss will charge at the player and power attack for big damage. 
        // repeat this process until dead.

        yield return new WaitForSeconds(4);
    }

    private void Awake()
    {
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
