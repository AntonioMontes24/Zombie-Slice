using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class ProgressionBarriers : MonoBehaviour
{
    [SerializeField] private List<GameObject> barrierObjectsToRemove;

    public UnityEvent listenForObjectiveCompleteEvent;
  
    private bool objectiveMetByEvent = false;
    private bool playerAtBarrierTrigger = false;

    public void OnObjectiveComplete()
    {
        objectiveMetByEvent = true;

        if (playerAtBarrierTrigger)
        {
            RemoveAllBarrier();
        }
    }


    void Update()
    {
        if(objectiveMetByEvent && playerAtBarrierTrigger)
        {
            RemoveAllBarrier();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerAtBarrierTrigger = true;
            if (objectiveMetByEvent)
            {
                RemoveAllBarrier();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerAtBarrierTrigger = false;
        }
    }

    private void RemoveAllBarrier()
    {
        if(barrierObjectsToRemove == null || barrierObjectsToRemove.Count == 0)
        {
            return;
        }

        foreach(GameObject barrier in barrierObjectsToRemove)
        {
            if(barrier != null)
            {
                barrier.SetActive(false);
            }
        }

        this.enabled = false;
        Collider selfCollider = GetComponent<Collider>();
        if(selfCollider != null)
        {
            selfCollider.enabled = false;
        }
    }

    public void ResetBarrier()
    {
        objectiveMetByEvent = false;
        playerAtBarrierTrigger = false;
        this.enabled = true;

        Collider selfCollider = GetComponent<Collider>();
        if(selfCollider != null)
        {
            selfCollider.enabled = true;
        }

        if(barrierObjectsToRemove != null)
        {
            foreach(GameObject barrier in barrierObjectsToRemove)
            {
                if(barrier != null)
                {
                    barrier.SetActive(true);
                }
            }
        }
    }
}
