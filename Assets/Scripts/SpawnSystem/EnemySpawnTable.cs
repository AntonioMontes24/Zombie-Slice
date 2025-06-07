using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "EnemySpawnTable", menuName = "Scriptable Objects/EnemySpawnTable")]
public class EnemySpawnTable : ScriptableObject
{
    public EnemySpawnEntry[] enemies;

    public GameObject PickEnemy()
    {
        var max = enemies.Sum((enemy) => enemy.rarity);
        var pick = Random.Range(0, max);
        var current = 0f;
        for(int i = 0; i < enemies.Length; i++)
        {
            current += enemies[i].rarity;
            if (current > pick)
                return enemies[i].enemy;
        }
        return null;
    }
}
