using UnityEngine;

public class zombieHitBox : MonoBehaviour
{
    [SerializeField] int hitDmg;
    private bool hitTarget;
    private bool isActive;
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
    }

    public void activeHit()
    {
        isActive = true;
        hitTarget = false;
    }

    public void notActiveHit()
    {
        isActive = false;
    }
}
