using System;
using System.Collections;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] int HP; //Enemy HP variable
    [SerializeField] int facePlayerSpeed;
    int zHP;
    public Animator anim; //Animator controller
    Color colorOrig; //Original color for hit feedback

    bool playerInRange; //Will check to see if player in range for attack
    Vector3 playerDir; //Will be used to direct towards player
    
    public GameObject playerObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        anim = GetComponent<Animator>();
        //How will the enemy affect the gamemanger to keep track of the number of enemies left for the goal of the game?
    }

    // Update is called once per frame
    void Update()
    {
        playerDir = zGameManager.instance.player.transform.position - transform.position; //Track player - calling zGameManager - created to test movement
        float dist = Vector3.Distance(transform.position, playerObject.transform.position);
        if (playerInRange)
        {

            //z1Attack();

            agent.SetDestination(zGameManager.instance.player.transform.position);
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                FacePlayer();
                
            }
            //Check if enemy is within melee attack range
            if (dist <= 1.75)
            {
                z1Attack();//Trigger attack animation
            }
            else if (dist >= 1.76)
            {
                anim.SetBool("AttackRange", false);//Trigger animator attack to stop until back in range
                agent.isStopped = false;
            }
          
        }
        else
        {

            anim.SetBool("AttackRange", false);
        }
    }

    /*MOVEMENT*/
    void FacePlayer()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * facePlayerSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            agent.isStopped = false;
            anim.SetBool("playerInRange", true);//Trigger walk animation
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            agent.isStopped = true;
            anim.SetBool("playerInRange", false);//Trigger idle animation
        }
    }



    /*DAMAGE*/
    public void takeDamge(int amount)
    {
        HP -= amount; //Reduce enemy HP by amount of damage taken from player/weapon
        zHP = HP;
        anim.SetTrigger("Hit");
        anim.SetInteger("HP", zHP);

        if (HP <= 0)
        {
            StartCoroutine(deathAnim());//Trigger death animation
            //Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }
    //Flash red when hit
    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        anim.SetTrigger("Hit");//Trigger hit animation
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }
    //Play hit animation when taking damage
    IEnumerator deathAnim()
    {
        anim.SetBool("isDead", true);
        yield return new WaitForSeconds(3.0f);//Play death animation then remove from area (destroy object) after 3 seconds. Could remove this if we want to have bodies remain
        Destroy(gameObject);
    }
    //Intention is to set the distance parameter in the animator to 0 to trigger attack animation
    private void z1Attack()
    {
        agent.isStopped = true;
        anim.SetBool("AttackRange", true);
    }

   

    //Future possibility is to slow the enemy down with each hit

}
