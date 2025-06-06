using UnityEngine;

public class enemyAI : MonoBehaviour
{

    [SerializeField] int HP;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void takeDamage(int amount)
    {
        HP -= amount; //Reduce enemy HP by amount of damage taken from player/weapon
    }
}
