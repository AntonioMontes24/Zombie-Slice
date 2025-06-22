using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{

    [SerializeField] private Slider enemyHealthBar;

    public void updateHealthBar(int currHealth, int maxHealth)
    {
        enemyHealthBar.value = (float)currHealth / maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
