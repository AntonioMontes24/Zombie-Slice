using UnityEngine;

public class zombieHitBox : MonoBehaviour
{
    [SerializeField] int hitDmg = 10;
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
            Debug.Log($"{gameObject.name} hit {other.name} for {hitDmg} damage.");
        }
    }

    public void ResetHit()
    {
        hitTarget = false;
        isActive = false;
    }

    public void Activate()
    {
        isActive = true;
        hitTarget = false;
    }

    public void Deactivate()
    {
        isActive = false;
    }
}
