using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.InputSystem.Editor;

public class ZVariant1_AI : MonoBehaviour, IDamage, iEnemyHealth
{
    [Header("Zombie Health")]
    [SerializeField] int currHealth;                        // the current health 
    [SerializeField] int _maxHealth;                        // the max health

    [Header("Zombie Movement")]
    [SerializeField] float speed;                           // the speed when walking normally
    [SerializeField] float speedModifier = 1.0f;            // the modifier if running or slowed
    [SerializeField] int faceTargetSpeed;                   // how fast he faces the target when not moving

    [Header("Zombie Components")]
    [SerializeField] Renderer model;                        // Model we will use when we flash a new color on hit or when damaged.
    [SerializeField] NavMeshAgent agent;                    // NavMeshAgent to traverse our navmesh
    [SerializeField] Animator animator;                     // this will handle our animations

    [Header("Zombie Detection & Vision")]
    [SerializeField] bool playerInRange;                    // is our player in range to be chased
    [SerializeField] float aggroRange = 15f;                // max distance for the trigger collider
    [SerializeField] int field_of_view = 90;                // the number of degrees our of enemy can see
    [SerializeField] Transform headPos;                     // for raycast to player. Can he see where we are at
    float angle_to_player;                                  // the angle to the player 
    Vector3 playerDirection;                                // Direction of our player

    [Header("Zombie Attack")]
    [SerializeField] float meleeAttackRange = 2.0f;         // how close the player needs to be for successful hit
    [SerializeField] int attackRate;                        // our Attack is on cooldown
    [SerializeField] int attackDamage;                      // how much damage do we do
    private float attackCounter;                                      // incremented until we hit the attackRate
    [SerializeField] Collider clawCollider;                 // claw collider

    [Header("Zombie Roaming")]
    [SerializeField] int roamDistance;                      // max distance he can roam from start position
    [SerializeField] int roamStopTimer;                     // how long before he roams again
    Vector3 startingPostion;                                // where his spawn position is
    private float roamTime;                                         // counter to see if he can roam 
    private float stoppingDistanceOriginal;                         // cache off the original stopping distance

    [Header("Zombie Audio")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip aud_clip_idle;
    [SerializeField] AudioClip aud_clip_attack;
    [SerializeField] AudioClip aud_clip_death;
    [SerializeField] AudioClip aud_clip_swipe;
    [SerializeField] int audioCounter;

    [Header("Zombie Animation")]
    private float currentAnimSpeed = 0f; // Smooth animation transition speed
    private float targetAnimSpeed = 0f;


    private bool isAttacking = false;
    //casched player transofrm for effeciency
    private Transform playerTransform;

    //int iEnemyHealth.CurrentHealth
    //{
    //    get {  return currHealth; }
    //}

    //int iEnemyHealth.maxHealth
    //{
    //    get {  return _maxHealth; }
    //}

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
        if(currHealth > 0)
        {
            currHealth -= amount;
            currHealth = Mathf.Max(currHealth, 0);

            if(GameManager.instance != null)
                GameManager.instance.UpdateEnemyHealthBar(this); //Moved it to a single statemenet
            else                                                 
                Debug.LogError("GameManager.instance is NULL!!");

            if(playerTransform != null)
            {
                agent.SetDestination(playerTransform.position);
                agent.isStopped = false;
                SetTargetAnimSpeed(speed * speedModifier);
            }
        }

        if (currHealth <= 0)
        {
            if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;
            if (agent.enabled) agent.enabled = false;
            this.enabled = false;
            StartCoroutine(removeCorpse());
            GameManager.instance.enemyInfoPanel.SetActive(false);
        }
    }

    IEnumerator removeCorpse()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            if(agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }

            animator.SetTrigger("isDead"); // RoamerDeath state
        }

        playAudioDeath();
        ObjectiveManager.instance.updateZombieCount(-1);

        if(PickupSpawner.instance != null)
        {
            PickupSpawner.instance.SpawnPickupsAtLocation(transform.position);
        } else
        {
            Debug.LogWarning("Pickup Spawner: pickup spawner is null");
        }

        yield return new WaitForSeconds(4);
        Destroy(gameObject);
    }

    IEnumerator attackAnimationTrigger()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            isAttacking = true;
            agent.isStopped = true;
            SetTargetAnimSpeed(0);
            animator.SetTrigger("swipe");
        }
        yield return new WaitForSeconds(1.0f);
        isAttacking = false;
    }

    public void assignAttackDamage()
    {
        // only deal damage if player is still within melee range
        if(playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if(distanceToPlayer <= meleeAttackRange)
            {
                IDamage player_dmg = GameManager.instance.player.GetComponent<IDamage>();
                if(player_dmg != null)
                {
                    player_dmg.takeDamage(attackDamage);
                    playAudioSwipe();
                }
            }
        }
    }

    public void playAudioDeath()
    {
        aud.Stop();

        if (aud_clip_death != null)
            aud.PlayOneShot(aud_clip_death);
    }

    public void playAudioIdle()
    {
        if (aud_clip_idle != null)
            aud.PlayOneShot(aud_clip_idle);
    }

    public void playAudioSwipe()
    {
        aud.Stop();

        if (aud_clip_swipe != null)
            aud.PlayOneShot(aud_clip_swipe);
    }

    public void clawColliderOn()
    {
        if (clawCollider)
            clawCollider.enabled = true;
    }

    public void clawColliderOff()
    {
        if (clawCollider)
            clawCollider.enabled = false;
    }

    void Start()
    {
        currHealth = _maxHealth;
        playerTransform = GameManager.instance.player.transform; // cache player transform

        attackCounter = attackRate;
        roamTime = roamStopTimer;

        agent.isStopped = true;
        SetAnimationSpeed(0);
    }

    private void Awake()
    {
        startingPostion = transform.position;
        stoppingDistanceOriginal = agent.stoppingDistance;
        agent.stoppingDistance = meleeAttackRange * 0.8f;
    }
    // Update is called once per frame
    void Update()
    {
        if(currHealth <= 0) return;

        if(attackCounter < attackRate)
            attackCounter += Time.deltaTime;

        if(isAttacking)
        {
            // Freeze movement and animation while attacking
            agent.isStopped = true;
            SetTargetAnimSpeed(0);
            UpdateAnimSpeed(); // <-- still update anim speed to match blend tree
            return;
        }

        bool playerIsVisible = canWeSeeThePlayer();

        if(playerIsVisible)
        {
            roamTime = 0;
            faceTarget();

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if(distanceToPlayer <= meleeAttackRange)
            {
                agent.isStopped = true;
                SetTargetAnimSpeed(0);

                if(attackCounter >= attackRate)
                {
                    StartCoroutine(attackAnimationTrigger());
                    attackCounter = 0;
                }
            } else
            {
                agent.SetDestination(playerTransform.position);
                agent.isStopped = false;
                agent.speed = speed * speedModifier;
                SetTargetAnimSpeed(speed * speedModifier);
            }
        } else
        {
            roamCheck();
        }
        UpdateAnimSpeed(); // <-- smoothly transitions speed every frame
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

            if(agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }

            SetAnimationSpeed(0);
            agent.stoppingDistance = stoppingDistanceOriginal;
            roamTime = roamStopTimer;
        }
    }

    void faceTarget()
    {
        if (playerTransform == null) return;

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0;

        if(directionToPlayer.sqrMagnitude > 0.01f)
        {
            Quaternion rotate_val = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotate_val, Time.deltaTime * faceTargetSpeed);
        }
    }

    bool canWeSeeThePlayer()
    {
        if (playerTransform == null || headPos == null) return false;

        Vector3 directionToPlayerFromHead = GameManager.instance.player.transform.position - headPos.position;
        float distanceToPlayer = directionToPlayerFromHead.magnitude;
        Vector3 zombieForwardFlat = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 playerDirectionFlat = new Vector3(directionToPlayerFromHead.x, 0, directionToPlayerFromHead.z).normalized;
        angle_to_player = Vector3.Angle(zombieForwardFlat, playerDirectionFlat);
        //angle to player relative to zombie's forward direction
        bool inFOV = angle_to_player <= field_of_view / 2;

        // make our Raycast to the player. If it hits the player, we have line of sight.
        RaycastHit hit_player;

        if (Physics.Raycast(headPos.position, directionToPlayerFromHead.normalized, out hit_player, distanceToPlayer + 0.1f))
        {
            // check if its the player we hit
            if (hit_player.collider.CompareTag("Player"))
            {
                return inFOV;
            }
        }
        return false;
    }

    void roam()
    {
        roamTime = 0;
        agent.stoppingDistance = 0;

        Vector3 randPos = Random.insideUnitSphere * roamDistance;
        randPos += startingPostion;

        NavMeshHit hit;
        if(NavMesh.SamplePosition(randPos, out hit, roamDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            agent.isStopped = false;
            agent.speed = speed * 0.5f * speedModifier;
            SetTargetAnimSpeed(speed * 0.5f * speedModifier);
        } else
        {
            roamTime = roamStopTimer;
            SetTargetAnimSpeed(0);
            agent.isStopped = true;
        }
    }

    void roamCheck()
    {
        if(!agent.enabled || !agent.isOnNavMesh)
        {
            SetTargetAnimSpeed(0);
            return;
        }

        if(agent.remainingDistance < 0.1f && !agent.pathPending && agent.velocity.sqrMagnitude < 0.1f)
        {
            agent.isStopped = true;
            roamTime += Time.deltaTime;

            SetTargetAnimSpeed(0);

            if (roamTime >= roamStopTimer)
                roam();
        } else
        {
            agent.speed = speed * 0.5f * speedModifier;
            SetTargetAnimSpeed(speed * 0.5f * speedModifier);
            roamTime = 0;
        }
    }

    void SetTargetAnimSpeed(float worldSpeed)
    {
        // Normalize agent speed to match blend tree input range
        float normalizedSpeed = Mathf.Clamp01(worldSpeed / agent.speed);
        targetAnimSpeed = normalizedSpeed;
    }

    void UpdateAnimSpeed()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            float smoothSpeed = Mathf.MoveTowards(animator.GetFloat("Speed"), targetAnimSpeed, Time.deltaTime * 5f);
            animator.SetFloat("Speed", smoothSpeed);
        }
    }
    void SetAnimationSpeed(float currentSpeed)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            float animSpeed = Mathf.Clamp01(currentSpeed / speed); // Normalized 0–1
            animator.SetFloat("Speed", animSpeed); // Drives blend tree
        }
    }
}
