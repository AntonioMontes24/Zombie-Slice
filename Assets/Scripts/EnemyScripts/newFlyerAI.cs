using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class newFlyerAI : MonoBehaviour, IDamage, iEnemyHealth
{
    [SerializeField] int HP;
    [SerializeField] int maxHP;
    [SerializeField] GameObject blightBomb;
    [SerializeField] Transform firePos;
    [SerializeField] int animSpeedTrans;
    [SerializeField] float downAngle;
    [SerializeField] v2Mover moverScript;
    public GameObject playerObj;
    public GameObject parentObj;
    public Animator anim;

    Vector3 parentPos;
    bool isDead;
    bool inRange;

    public int CurrentHealth => HP;

    public int maxHealth => maxHP;
    bool fade;
    float alpha = 0f;
    float fadespeed = 1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parentPos = transform.position;
        ObjectiveManager.instance.updateZombieCount(1);
        playerObj = GameManager.instance.player;
        //For death fade
        Material mat = GetComponent<Renderer>().material;
        Color newColor = mat.color;
        newColor.a = alpha;
        mat.color = newColor;

    }

    // Update is called once per frame
    void Update()
    {
        setAnimations();
        if (inRange)
        {
            bomberAttack();
        }
        if (HP <= 0)
        {
            isDead = true;
        }
        else
            isDead = false;

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
        if (HP > 0)
        {
            HP -= amount;
        }
        moverScript.forceChasePlayer();
        if (HP <= 0)
        {
            ObjectiveManager.instance.updateZombieCount(-1);
            moverScript.forceStop();
        }
    }

    void bomberAttack()
    {
        anim.SetTrigger("Fire");
    }

    IEnumerator death()
    {
        anim.SetBool("isDead", true);
        float dropSpeed = 3.0f;
        float elapsed = 0f;
        Vector3 startPos = transform.position; //Current position
        Vector3 targetPos = new Vector3(startPos.x, 0f, startPos.z); //Drop location on y axis


        while (elapsed < dropSpeed)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / dropSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos; //set new position

        yield return new WaitForSeconds(1.5f);

        anim.enabled = false; //disable animator
        //moverScript.SinkingZombie(); // Call zombie sink from parent
        //transform.Translate(Vector3.down * 1.0f * Time.deltaTime);

        deathFade();
        yield return new WaitForSeconds(2.0f);
        Destroy(transform.parent.gameObject);

    }

    public void createBlight()
    {
        if (playerObj == null) return;
        Vector3 target = playerObj.transform.position;
        GameObject bomb = Instantiate(blightBomb, firePos.position, firePos.rotation);
        Rigidbody rb = bomb.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (Quaternion.Euler(downAngle, target.y, target.z) * firePos.forward).normalized;
            rb.linearVelocity = direction;

        }
    }

    private void deathFade()
    {
        if (fade)
        {
            Material mat = GetComponent<Renderer>().material;
            Color newColor = mat.color;
            newColor.a = Mathf.MoveTowards(newColor.a, 0, fadespeed * Time.deltaTime);
            mat.color = newColor;
        }
    }
}
