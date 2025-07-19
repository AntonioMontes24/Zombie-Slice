using System.Collections;
//using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.Audio;


public class ZombieVariant2AI : MonoBehaviour, IDamage, iEnemyHealth
{
    [Header("Zombie Health")]
    [SerializeField] private int currHealth;                        // the current health 
    [SerializeField] private int _maxHealth;                        // enemy Max Health

    [Header("Zombie Movement")]
    [SerializeField] private float walkSpeed = 1.5f;                           // the speed when walking normally
    [SerializeField] private float chaseSpeed = 3.5f;                   // speed while chasing
    [SerializeField] private float faceTargetSpeed = 5f;                   // how fast he faces the target when not moving

    [Header("Zombie Components")]
    [SerializeField] private Renderer model;                        // Model we will use when we flash a new color on hit or when damaged.
    [SerializeField] private NavMeshAgent agent;                    // NavMeshAgent to traverse our navmesh
    [SerializeField] private Animator animator;                     // this will handle our animations
    [SerializeField] private Transform headPos;                     // for raycast to player. Can he see where we are at

    [Header("Zombie Detection")]
    [SerializeField] private float detectionRange = 10f;            // max distance to chase player when aggro'd
    [SerializeField] private float aggroRange = 15f;                // how close player needs to be to aggro (if in FOV)
    [SerializeField] private float attackRange = 2f;                
    [SerializeField] private float fieldOfViewAngle = 60f;          

    [Header("Zombie Attack")]
    [SerializeField] private int swipeDamage = 10;                       // how much damage do we do
    [SerializeField] private int biteDamage = 20;                        // initial bite damage - applied DoT damage
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private Collider clawCollider;                 // collider for the claw
    [SerializeField] private Collider teethCollider;                // collider for the teeth

    [Header("Bit Attack")]
    [SerializeField] private int bleedDamagePerTick = 3;
    [SerializeField] private float bleedTickInterval = 2f;          // damage every 2 seconds
    [SerializeField] private float bleedDuration = 10f;             // dot duration


    [Header("Zombie Roam")]
    [SerializeField] private float roamDistance = 10f;                      // max distance he can roam from start position
    [SerializeField] private float roamInterval = 5f;                     // how long before he roams again
    private Vector3 startingPostion;                                // where his spawn position is
    private float roamTimer;                                         // counter to see if he can roam 

    private Transform playerTransform;
    private bool isAttacking = false;
    private float lastAttackTime;
    private bool isAggroed = false;

    [Header("Zombie Audio")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip aud_clip_idle;
    [SerializeField] AudioClip aud_clip_attackLoop;
    [SerializeField] AudioClip aud_clip_swipe;  // 1 second long
    [SerializeField] AudioClip aud_clip_bite;   // 2 seconds long
    [SerializeField] AudioClip aud_clip_death;

    // what does this do???
    // List<PickupSpawner> pickups = new List<PickupSpawner>();

    public int CurrentHealth
    {
        get { return currHealth; }
        private set { currHealth = value; } // private setter for consistency
    }

    public int maxHealth
    {
        get { return _maxHealth; }
    }

    // for IDamage
    public void takeDamage(int amount)
    {
        if(currHealth <= 0) return;

        currHealth -= amount;
        currHealth = Mathf.Max(currHealth, 0); // ensure health doesnt go below 0

        if (!isAggroed)
        {
            isAggroed=true;
        }
        if(GameManager.instance != null)
        {
            GameManager.instance.UpdateEnemyHealthBar(this);
            if(GameManager.instance.enemyInfoPanel != null)
            {
                GameManager.instance.enemyInfoPanel.SetActive(true);
            }
        }

        if(agent != null && agent.enabled && playerTransform != null)
        {
            agent.SetDestination(playerTransform.position);
            agent.isStopped = false;
            SetAnimatorSpeed(chaseSpeed);
        }

        if(currHealth <= 0)
        {
            Die();
        }

    }

    private void Die()
    {
        if(GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;
        if(agent != null) agent.enabled = false;
        this.enabled = false;

        if(animator != null)
        {
            animator.SetTrigger("DeathTrigger");
        }

        PlayAudioDeath();
        ObjectiveManager.instance.updateZombieCount(-1);
        StartCoroutine(RemoveCorpseRoutine());

        if(GameManager.instance != null && GameManager.instance.enemyInfoPanel != null)
        {
            GameManager.instance.enemyInfoPanel.SetActive(false);
        }
        isAggroed = false;
    }

    IEnumerator RemoveCorpseRoutine()
    {
        yield return new WaitForSeconds(aud_clip_death != null ? aud_clip_death.length + 0.5f : 4f);

        if(ObjectiveManager.instance != null)
        {
            ObjectiveManager.instance.updateZombieCount(-1);
        }
        Destroy(gameObject);
    }

    IEnumerator SwipeAttackRoutine()
    {
        isAttacking = true;
        SetAnimatorSpeed(0);
        agent.isStopped = true;

        if(animator != null)
        {
            animator.SetTrigger("SwipeAttackTrigger");
        }
        PlayAudioSwipe();

        // wait for animation before next attack / movement
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length * 0.8f);

        // damage is applied via animation event

        // wait for remaining cd
        yield return new WaitForSeconds(attackCooldown - (animator.GetCurrentAnimatorStateInfo(0).length * 0.8f));

        isAttacking = false;
        lastAttackTime = Time.time;
        agent.isStopped = false;
            
    }

    IEnumerator BiteAttackRoutine()
    {
        isAttacking = true;
        SetAnimatorSpeed(0);
        agent.isStopped = true;

        if(animator != null)
        {
            animator.SetTrigger("BiteAttackTrigger");
        }
        PlayAudioBite();

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length * 0.8f);

        // apply dot directly to playerhealth script
        ApplyBiteBleedEffect();

        // wait for cd
        yield return new WaitForSeconds(attackCooldown - (animator.GetCurrentAnimatorStateInfo(0).length * 0.8f));

        isAttacking = false;
        lastAttackTime = Time.time;
        agent.isStopped = false;

    }

    private void ApplyBiteBleedEffect()
    {
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            GameManager.instance.ShowBittenStatus(bleedDuration);
            PlayerHealth playerHealth = GameManager.instance.player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ApplyBleed(bleedDamagePerTick, bleedTickInterval, bleedDuration);
            }
        }
    }

    public void AssignSwipeDamage()
    {
        if(GameManager.instance != null && GameManager.instance.player != null)
        {
            IDamage player_dmg = GameManager.instance.player.GetComponent<IDamage>();
            if(player_dmg != null)
            {
                player_dmg.takeDamage(swipeDamage);
            }
        }
    }

    public void AssignBiteDamage()
    {
        if(GameManager.instance != null && GameManager.instance.player != null)
        {
            IDamage player_dmg = GameManager.instance.player.GetComponent<IDamage>();
            if(player_dmg != null)
            {
                player_dmg.takeDamage(biteDamage);
            }
        }
    }

    private void Awake()
    {
        
        if(agent == null) agent = GetComponent<NavMeshAgent>();
        if(animator == null) animator = GetComponent<Animator>();
        if(aud == null) aud = GetComponent<AudioSource>();
        if(model == null) model = GetComponentInChildren<MeshRenderer>();

        currHealth = _maxHealth;
        startingPostion = transform.position;

        if(agent != null)
        {
            agent.stoppingDistance = attackRange;
        }

        if(GameManager.instance != null && GameManager.instance.player != null)
        {
            playerTransform = GameManager.instance.player.transform;
        } else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if(playerObj != null) playerTransform = playerObj.transform;
            else
            {
                Debug.LogWarning("Player not found with tag 'Player' in Zombie Variant 2");
            }
        }

        if(clawCollider != null) clawCollider.enabled = false;
        if(teethCollider != null) teethCollider.enabled = false;

        if(ObjectiveManager.instance != null)
        {
            ObjectiveManager.instance.updateZombieCount(1);
        }

        roamTimer = roamInterval;

    }

    void Update()
    {

        if (currHealth <= 0) return;

        if(agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            SetAnimatorSpeed(0);
            return;
        }

        // aggro 
        if (!isAggroed)
        {
            if(playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                if(distanceToPlayer <= aggroRange && CanWeSeeThePlayer(distanceToPlayer))
                {
                    isAggroed = true;
                }
                else
                {
                    HandleRoamingState();
                }
            }
            else
            {

                HandleRoamingState();

            }
        } else
        {
            HandleAggroedState();
        }
    }

    private void HandleRoamingState()
    {
        PlayAudioLooping(aud_clip_idle);

        roamTimer += Time.deltaTime;
        if(roamTimer >= roamInterval && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            Roam();
        }
        SetAnimatorSpeed(agent.velocity.magnitude);
    }

    private void HandleAggroedState()
    {
        float distanceToPlayer = playerTransform != null ? Vector3.Distance(transform.position, playerTransform.position) : Mathf.Infinity;
        bool canSeePlayerNow = playerTransform != null && CanWeSeeThePlayer(distanceToPlayer);




        if(canSeePlayerNow && distanceToPlayer <= detectionRange)
        {
            roamTimer = roamInterval;

            if(distanceToPlayer <= agent.stoppingDistance + 0.1f) // player is in attack range
            {
                agent.isStopped = true;
                SetAnimatorSpeed(0);
                FaceTarget();

                if(!isAttacking && Time.time >= lastAttackTime + attackCooldown)
                {
                    // rng attack choice
                    int attackChoice = Random.Range(0, 10);
                    if (attackChoice >= 4)
                    {
                        StartCoroutine(SwipeAttackRoutine());
                    }else
                    {
                        StartCoroutine(BiteAttackRoutine());
                    }
                }
            }
            else // player is detected but not in range
            {
                agent.SetDestination(playerTransform.position);
                agent.isStopped = false;
                SetAnimatorSpeed(chaseSpeed);
            }
            PlayAudioLooping(aud_clip_attackLoop); // when chasing / attacking
        } else
        {
            // deaggro if player out of range
            if(distanceToPlayer > detectionRange + 1f)
            {
                isAggroed = false;
                HandleRoamingState();
            }
            else // still within detection range, but lost LOS
            {
                // last known player position
                if(agent.remainingDistance <= agent.stoppingDistance + 0.1f)
                {
                    HandleRoamingState();
                }
                else
                {
                    // keep moving to last position it was walking to
                    SetAnimatorSpeed(walkSpeed);
                }
            }
        }

    }

    void Roam()
    {
        roamTimer = 0f;

        Vector3 randomDirection = Random.insideUnitSphere * roamDistance;
        randomDirection += startingPostion;

        NavMeshHit hit;
        if(NavMesh.SamplePosition(randomDirection, out hit, roamDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            agent.isStopped = false;
        }
        SetAnimatorSpeed(walkSpeed);
    }

    void FaceTarget()
    {
        if (playerTransform == null) return;

        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * faceTargetSpeed);

    }

    bool CanWeSeeThePlayer(float currentDistanceToPlayer)
    {
       if (playerTransform == null || headPos == null) return false;

       Vector3 directionToPlayer = playerTransform.position - headPos.position;
        float angleToPlayer = Vector3.Angle(directionToPlayer, transform.forward);

        // checking range for los check based on aggro 
        float effectiveRange = isAggroed ? detectionRange : aggroRange;

        if(angleToPlayer <= fieldOfViewAngle * 0.5f && currentDistanceToPlayer <= effectiveRange)
        {
            RaycastHit hit;
            if(Physics.Raycast(headPos.position, directionToPlayer.normalized, out hit, effectiveRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    return true; // player in fov, range and in los
                }
            }
        }
        return false; // player not seen
    }

    private void PlayAudioLooping(AudioClip clip)
    {
        if (aud == null || clip == null) return;

        if (aud.clip != clip || !aud.isPlaying)
        {
            aud.clip = clip;
            aud.loop = true;
            aud.Play();
        }
    }

    public void PlayAudioSwipe()
    {
        if (aud_clip_swipe != null)
            aud.PlayOneShot(aud_clip_swipe);
    }

    public void PlayAudioBite()
    {
        if (aud_clip_bite != null)
            aud.PlayOneShot(aud_clip_bite);
    }

    public void PlayAudioIdle()
    {
        if (aud_clip_idle != null)
            aud.PlayOneShot(aud_clip_idle);
    }

    public void PlayAudioDeath()
    {
        aud.Stop();
        if (aud_clip_death != null)
            aud.PlayOneShot(aud_clip_death);
    }

    public void ClawColliderOn()
    {
        if (clawCollider)
            clawCollider.enabled = true;
    }

    public void ClawColliderOff()
    {
        if (clawCollider)
            clawCollider.enabled = false;
    }

    public void TeethColliderOn()
    {
        if(teethCollider != null)
        {if (teethCollider)
                teethCollider.enabled = true;
        }
    }

    public void TeethColliderOff()
    {
        if(teethCollider != null)
        {if (teethCollider)
                teethCollider.enabled = false;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        // the other object in range is Player
        if (other.CompareTag("Player"))
        {
            // player is in range! 
            playerTransform = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // the other object leaving range is Player
        if (other.CompareTag("Player"))
        {
            // player left our range
            playerTransform = null;
        }
    }


    void SetAnimatorSpeed(float speed)
    {
        if(animator != null)
        {
            animator.SetFloat("Speed", speed);
        }
    }
}
