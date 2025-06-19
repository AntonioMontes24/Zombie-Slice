using System.Collections;
//using Unity.VisualScripting;
using UnityEngine;


public class damage : MonoBehaviour
{
    enum damageType { moving, stationary, DOT, homing }
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;
    [SerializeField] int damageAmount;
    [SerializeField] float dammgeRate;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;

    bool isDamaging;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (type == damageType.moving || type == damageType.homing)
        {
            Destroy(gameObject, destroyTime);

            if (type == damageType.moving)
            {
                rb.linearVelocity = transform.forward * speed;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (type == damageType.homing)
        {
            rb.linearVelocity = (ZGameManager.instance.player.transform.position - transform.position).normalized * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && type != damageType.DOT)
        {
            dmg.takeDamage(damageAmount);
        }

        if (type == damageType.homing || type == damageType.moving)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && type == damageType.DOT & !isDamaging)
        {
            StartCoroutine(damageOther(dmg));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.CancelHurtLoop();
        }
    }

    IEnumerator damageOther(IDamage d)
    {
        isDamaging = true;
        d.takeDamage(damageAmount);
        yield return new WaitForSeconds(dammgeRate);
        isDamaging = false;
    }
}
