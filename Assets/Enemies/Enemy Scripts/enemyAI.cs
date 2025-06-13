using System;
using System.Collections;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor.UI;

using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] int HP; //Enemy HP variable
    [SerializeField] int facePlayerSpeed;
    [SerializeField] Transform zHeadPOS; //Zombie 
     int zHP;
    //[SerializeField] int z1Dmg;
    [SerializeField] int FOV; //Field of view
    [SerializeField] LayerMask playerLayer;
    [SerializeField] float hearingRange;
    public Animator anim; //Animator controller

    Color colorOrig; //Original color for hit feedback

    bool playerInRange; //Will check to see if player in range for attack
    bool playerInRangeFar;
    Vector3 playerDir; //Will be used to direct towards player
    float angleToTarget;
    

    public GameObject playerObject;
    //public GameObject playerTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        anim = GetComponent<Animator>();
        //zHP = HP;
        if (playerObject == null)
        {
            playerObject = GameObject.FindWithTag("Player");
        }
        //How will the enemy affect the gamemanger to keep track of the number of enemies left for the goal of the game?
    }

    // Update is called once per frame
    void Update()
    {
        // anim.SetInteger("HP", zHP);
        
        if (zHP > 0)
        {
            float dist = Vector3.Distance(transform.position, playerObject.transform.position);
            if (playerInRange)
            {
                bool seesplayer = PlayerChase();
                if (!seesplayer)
                {
                    FacePlayer();
                }
                //Check if enemy is within melee attack range
                if( dist <2)
                {
                    z1Attack();//Trigger attack animation
                }
                else if (dist >2)
                {
                    anim.SetBool("AttackRange", false);//Trigger animator attack to stop until back in range
                    agent.isStopped = false;
                }
                else
                {
                    anim.SetBool("AttackRange", false);
                }
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    FacePlayer();
                }
            }
        }
    }


    /*MOVEMENT*/
    bool PlayerChase()
    {
        if (playerObject == null)
        {
            return false;
        }
        playerDir = GameManager.instance.player.transform.position - zHeadPOS.position; //Track player - calling zGameManager - created to test movement
        angleToTarget = Vector3.Angle(playerDir, transform.forward);
        //Debug.DrawRay(zHeadPOS.position, playerDir.normalized * 10, Color.red);
        RaycastHit hit;
        if (Physics.Raycast(zHeadPOS.position, playerDir, out hit, Mathf.Infinity, playerLayer))
        {
            if (hit.collider.CompareTag("Player") && angleToTarget < FOV)
            {
                agent.SetDestination(playerObject.transform.position);
                anim.SetBool("playerInRange", true);
                return true;
            }
        }
        // if (playerInRange)
        // {
        //     agent.SetDestination(playerObject.transform.position);
        //     anim.SetBool("playerInRange", true);
        //     return true;
        // }
        if (playerInRangeFar)
        {
            //agent.SetDestination(playerObject.transform.position);
            anim.SetBool("playerInRange", true);
            return true;
        }
        anim.SetBool("playerInRange", false);
        return false;
    }
    void FacePlayer()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * facePlayerSpeed);
    }

    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRangeFar = true;
            //agent.isStopped = false;
            anim.SetBool("playerInRangeClose", true);//Trigger walk animation


        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRangeFar = false;
            //agent.isStopped = true;
            anim.SetBool("playerInRangeClose", false);//Trigger idle animation
        }
    }

    
    /*DAMAGE*/
    //Flash red when hit
    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        anim.SetTrigger("Hit");//Trigger hit animation
        agent.isStopped = true;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
        agent.isStopped = false;
    }
    //Play hit animation when taking damage
    IEnumerator deathAnim()
    {
        anim.SetInteger("isDead", 1);
        agent.isStopped = true;
        agent.SetDestination(agent.transform.position);
        
        
        //playerInRange = false;
        yield return new WaitForSeconds(4.25f);//Play death animation then remove from area (destroy object) after 3 seconds. Could remove this if we want to have bodies remain
        zDead();
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
        // if (zHP < 0)
        // {
        //     return;
        // }
        HP -= amount; //Reduce enemy HP by amount of damage taken from player/weapon
        zHP = HP;
        if (HP < 0)
        {
            StartCoroutine(flashRed());
            anim.SetTrigger("Hit");
            anim.SetInteger("HP", HP);
            StartCoroutine(deathAnim());
            
        }
        else if (HP > 0)
        {
            StartCoroutine(flashRed());
            anim.SetBool("isMoving", true);
            anim.SetTrigger("Hit");
            anim.SetInteger("HP", HP);
            agent.SetDestination(playerObject.transform.position);
        }
    }

    //Future possibility is to slow the enemy down with each hit

}
