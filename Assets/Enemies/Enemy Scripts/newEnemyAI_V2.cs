using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class newEnemyAI_V2 : MonoBehaviour, IDamage
{
    [SerializeField] int HP;
    [SerializeField] int animSpeedTrans;
    [SerializeField] v2Mover moverScript;
    [SerializeField] Renderer model;
    [SerializeField] float meleeDist;
    [SerializeField] headHit HeadScript;
    public GameObject playerObj;
    public GameObject parentObj;
    
    public Animator anim;
    public Collider z2Collide;
    public Collider headCollider;
    Color orignialColor;
    Vector3 parentPos;
    bool isDead;
    bool inRange;
    bool meleeRange;
    bool headHit;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parentPos = transform.position;
        ObjectiveManager.instance.updateZombieCount(1);
        orignialColor = model.material.color;
        z2Collide = GetComponent<Collider>();
        headCollider = GetComponent<Collider>();
        playerObj = GameManager.instance.player;
        HeadScript = GetComponentInChildren<headHit>();
    }

    // Update is called once per frame
    void Update()
    {
        setAnimations();
        if (inRange)
        {
            moverScript.forceChasePlayer();
        }
        if (HP <= 0)
        {
            isDead = true;
        }
        else
            isDead = false;

        checkDist();

        if (meleeRange)
            meleeAttack();
    }

    void checkDist()//Checking distance of parent to player
    {
        if (GameManager.instance.player != null && parentObj != null)
        {
            float dist = Vector3.Distance(parentObj.transform.position, GameManager.instance.player.transform.position);
            meleeRange = dist <= meleeDist;
        }
        else
        {
            meleeRange = false;
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
        }
    }

    void setAnimations()
    {
        float moveSpeed = (transform.position - parentPos).magnitude / Time.deltaTime;
        parentPos = transform.position;
        float animSpeedCur = anim.GetFloat("Speed");
        anim.SetFloat("Speed", Mathf.Lerp(animSpeedCur, moveSpeed, Time.deltaTime * animSpeedTrans));
        if (isDead)
        {
            anim.SetBool("isDead", true);
            StartCoroutine(death());
        }
    }

    public void takeDamage(int amount)
    {
        if (HeadScript.hit)
        {
            amount *= 2;
            HP -= amount;
            HeadScript.hit = false;
        }
        if (HP > 0)
            {
                HP -= amount;
            }
        StartCoroutine(getHit());
        moverScript.forceChasePlayer();
        if (HP <= 0)
        {

            ObjectiveManager.instance.updateZombieCount(-1);
        }
        
    }

    void meleeAttack()
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            StartCoroutine(attack());
        }
        
    }

    IEnumerator attack()
    {
        foreach (var hitbox in GetComponentsInChildren<zombieHitBox>())
        {
            hitbox.Activate();
        }
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);
        foreach (var hitbox in GetComponentsInChildren<zombieHitBox>())
        {
            hitbox.Deactivate();
        }
        
    }

    IEnumerator death()
    {
        anim.SetBool("isDead", true);
        z2Collide.enabled = false;
        yield return new WaitForSeconds(2.0f);
        anim.enabled = false;
        ObjectiveManager.instance.updateZombieCount(-1);
        Destroy(parentObj.gameObject);
    }

    IEnumerator getHit()
    {
        model.material.color = Color.red;
        anim.SetTrigger("getHit");
        yield return new WaitForSeconds(0.1f);
        model.material.color = orignialColor;
    }
}

