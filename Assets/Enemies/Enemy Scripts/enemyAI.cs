using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] int HP; //Enemy HP variable
    [SerializeField] int facePlayerSpeed;
   
   // public Transform target;
    //public Rigidbody rb;
    Color colorOrig; //Original color for hit feedback

    bool playerInRange; //Will check to see if player in range for attack
    Vector3 playerDir; //Will be used to direct towards player

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
       
        //How will the enemy affect the gamemanger to keep track of the number of enemies left for the goal of the game?
    }

    // Update is called once per frame
    void Update()
    {
       playerDir = zGameManager.instance.player.transform.position - transform.position; //Track player - calling zGameManager - created to test movement

        if (playerInRange)
        {
            agent.SetDestination(zGameManager.instance.player.transform.position);
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                FacePlayer();
            }
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount; //Reduce enemy HP by amount of damage taken from player/weapon
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
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

  

    /*DAMAGE*/
    public void takeDamge(int amount)
    {
        HP -= amount;

        if (HP <= 0)
        {
            Destroy(gameObject);
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
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    //Future possibility is to slow the enemy down with each hit

}
