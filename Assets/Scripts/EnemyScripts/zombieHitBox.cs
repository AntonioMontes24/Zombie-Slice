using UnityEngine;

public class zombieHitBox : MonoBehaviour
{
    [SerializeField] int hitDmg = 10;
    private bool hitTarget;
    private bool isActive;
    private Collider col;
    void Awake()
    {
        col = GetComponent<Collider>();
        col.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!isActive || hitTarget)
            return;

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null)
        {
            dmg.takeDamage(hitDmg);
            hitTarget = true;

        }
    }

    public void ResetHit()
    {
        hitTarget = false;
        isActive = false;
        col.enabled = false;
    }

    public void Activate()
    {
        isActive = true;
        hitTarget = false;
        col.enabled = true;
    }

    public void Deactivate()
    {
        isActive = false;
        col.enabled = false;
    }
}
