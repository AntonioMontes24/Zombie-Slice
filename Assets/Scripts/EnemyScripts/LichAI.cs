using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;


public class LichAI : MonoBehaviour, IDamage, iEnemyHealth
{
    // create some serialized variables
    [Header("Lich Health")]
    [SerializeField] int currHealth;                        // the current health
    [SerializeField] int _maxHealth = 100;

    [Header("Lich Movement")]
    [SerializeField] float speed = 3.5f;                           // the speed when walking normally
    [SerializeField] float speedModifier = 1f;                   // the modifier if running or slowed
    [SerializeField] int faceTargetSpeed = 5;                   // how fast he faces the target when not moving

    [Header("Lich Components")]
    [SerializeField] Renderer model;                        // Model we will use when we flash a new color on hit or when damaged.
    [SerializeField] NavMeshAgent agent;                    // NavMeshAgent to traverse our navmesh
    [SerializeField] Animator animator;                     // this will handle our animations

    [Header("Player Detection")]
    [SerializeField] bool playerInRange;                    // is our player in range to be chased
    private Transform playerTransform;
    Vector3 playerDirection;                                // Direction of our player
    //[SerializeField] int field_of_view = 60;                     // the number of degrees our of enemy can see
    [SerializeField] Transform headPos;                     // for raycast to player. Can he see where we are at
    //float angle_to_player;                                  // the angle to the player 

    // blightball fields
    [Header("Blight Ball Attack")]
    [SerializeField] Transform shootPosition;               // where does our blightball spawn from
    [SerializeField] GameObject blightBall;                 // our projectile
    [SerializeField] int blightBallDuration;                // how long a blightball lasts

    // field for Blightstorms
    [Header("Blight Storm Attack")]
    [SerializeField] Transform BS1Position;               // where does our blightball spawn from
    [SerializeField] Transform BS2Position;               // where does our blightball spawn from
    [SerializeField] Transform BS3Position;               // where does our blightball spawn from
    [SerializeField] Transform BS4Position;               // where does our blightball spawn from

    [Header("Lich Audio")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip aud_clip_idle;
    [SerializeField] AudioClip aud_clip_attack;
    [SerializeField] AudioClip aud_clip_death;
    [SerializeField] float idleAudioPlayRate = 5f;
    private float audioTimer;


    // new random attack info
    int attackToUse;
    [SerializeField] float attackDelay = 1.0f;
    //int attackCounter;
    //[SerializeField] int attackRate = 100;
    private float cooldownTimer;
    private bool isAttacking = false;

    public int CurrentHealth => currHealth;
    public int maxHealth => _maxHealth;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (aud == null) aud = GetComponent<AudioSource>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("LichAI: Player tag not found!");
            enabled = false;
        }

    }

    void Start()
    {
        currHealth = _maxHealth;
        audioTimer = idleAudioPlayRate;
        cooldownTimer = attackDelay;

        if (GameManager.instance != null)
        {
            GameManager.instance.SetCurrentEnemy(this);
            GameManager.instance.UpdateEnemyHealthBar(this);
        }
        else
        {
            Debug.LogWarning("LichAI: GameManager instance no found at start.");
        }
        if (ObjectiveManager.instance != null)
        {
            ObjectiveManager.instance.updateZombieCount(1);
        }

        if (aud != null && aud_clip_idle != null && !aud.isPlaying)
        {
            aud.clip = aud_clip_idle;
            aud.loop = true;
            aud.Play();
        }
    }

    void Update()
    {
        if (currHealth <= 0 || playerTransform == null) return;

        if (playerInRange)
        {
            faceTarget();
        }

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        audioTimer -= Time.deltaTime;
        if (audioTimer <= 0)
        {
            if (aud != null && aud_clip_idle != null && !aud.isPlaying)
            {
                aud.clip = aud_clip_idle;
                aud.loop = true;
                aud.Play();
            }
            audioTimer = idleAudioPlayRate;
        }

        if (!isAttacking && cooldownTimer <= 0 && playerInRange && HasLineOfSightToPlayer())
        {
            cooldownTimer = attackDelay;

            if (aud != null && aud.isPlaying && aud.clip == aud_clip_idle)
            {
                aud.Stop();
            }

            if (aud != null && aud_clip_attack != null)
            {
                aud.PlayOneShot(aud_clip_attack);
            }

            attackToUse = chooseAttack();

            if (attackToUse >= 2)
            {
                StartCoroutine(ShootAttackRoutine("shootBlightBall"));
            }
            else
            {
                StartCoroutine(ShootAttackRoutine("shootBlightstorm"));
            }
        }
    }

    IEnumerator ShootAttackRoutine(string triggerName)
    {
        isAttacking = true;
        //attackCounter = 0;

        if (animator != null)
        {
            animator.SetTrigger(triggerName);

            yield return null;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            float waitTime = 0f;

            if (stateInfo.shortNameHash == Animator.StringToHash("Standing 1H Magic Attack 01"))
            {
                waitTime = stateInfo.length;
            }
            else if (stateInfo.shortNameHash == Animator.StringToHash("Standing 2H Magic Attack 05"))
            {
                waitTime = stateInfo.length;
            }
            else
            {
                waitTime = 3.0f;
            }
            waitTime += 0.1f;


            yield return new WaitForSeconds(waitTime);
        }
        else
        {
            yield return new WaitForSeconds(3.0f);
        }
        isAttacking = false;
    }

    public void takeDamage(int amount)
    {
        if (currHealth <= 0) return;

        currHealth -= amount;

        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateEnemyHealthBar(this);
        }

        if (currHealth <= 0)
        {
            currHealth = 0;
            Die();
        }
    }

    void Die()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.HideEnemyUI();
        }

        if (aud != null)
        {
            aud.Stop();
            if (aud_clip_death != null)
            {
                aud.PlayOneShot(aud_clip_death);
            }
        }

        if (animator != null)
        {
            animator.SetTrigger("isDead");
        }

        if (ObjectiveManager.instance != null)
        {
            ObjectiveManager.instance.updateZombieCount(-1);
        }

        StartCoroutine(RemoveCorpseAfterDelay(5f));


    }

    IEnumerator RemoveCorpseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public void createBlightBall()
    {
        if (blightBall != null && shootPosition != null)
        {
            Instantiate(blightBall, shootPosition.position, transform.rotation);
        }
    }

    public void createBlightStorm()
    {
        if (blightBall != null)
        {
            Instantiate(blightBall, BS1Position.position, transform.rotation);
            Instantiate(blightBall, BS2Position.position, transform.rotation);
            Instantiate(blightBall, BS3Position.position, transform.rotation);
            Instantiate(blightBall, BS4Position.position, transform.rotation);
        }
    }

    public int chooseAttack()
    {
        return Random.Range(0, 5);

    }

    private void OnTriggerEnter(Collider other)
    {
        // the other object in range is Player
        if (other.CompareTag("Player"))
        {
            // player is in range! 
            playerInRange = true;
            if (GameManager.instance != null)
            {
                GameManager.instance.SetCurrentEnemy(this);
                GameManager.instance.UpdateEnemyHealthBar(this);
            }
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
        if (playerTransform == null) return;

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0;
        Quaternion rotate_val = (Quaternion.LookRotation(directionToPlayer));
        transform.rotation = Quaternion.Slerp(transform.rotation, rotate_val, Time.deltaTime * faceTargetSpeed);

    }

    bool HasLineOfSightToPlayer()
    {
        if (playerTransform == null || headPos == null) return false;


        // take the players current position from the game manager and subtract our position
        playerDirection = playerTransform.position - headPos.position;
        //angle_to_player = Vector3.Angle(playerDirection, transform.forward);

        // make our Raycast to the player. If it hits the player, we have line of sight. 
        RaycastHit hit_player;
        int layerMask = LayerMask.GetMask("Default", "Player");

        if (Physics.Raycast(headPos.position, playerDirection, out hit_player, Mathf.Infinity, layerMask))
        {
            if (hit_player.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }
}


