using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    float testRespawnTime;
    [SerializeField]
    EnemySpawnTable enemies;
    // you cannot use interfaces in serialized fields for some reason, so these are separate.
    [SerializeField]
    LineSpawnComponent[] lineSpawnComponent;
    [SerializeField]
    CircleSpawnComponent[] circleSpawnComponent;
    [SerializeField]
    RectangleSpawnComponent[] rectangleSpawnComponent;
    List<ISpawnComponent> components;

    public void SetEnemiesTable(EnemySpawnTable newEnemies) => enemies = newEnemies;

    public void SpawnEnemies(int number)
    {
        for (int i = 0; i < number; i++)
        {
            var max = components.Sum((component) => component.GetSize());
            var pick = Random.Range(0, max);
            var current = 0f;
            for (int s = 0; s < components.Count; s++)
            {
                current += components[s].GetSize();
                if (current > pick)
                {
                    Instantiate(enemies.PickEnemy(), components[s].GetSpawnPosition(), Quaternion.identity);
                    break;
                }
            }
        }
    }

    private void Awake()
    {
        List<ISpawnComponent> components2 = new();
        components2.AddRange(lineSpawnComponent);
        components2.AddRange(circleSpawnComponent);
        components2.AddRange(rectangleSpawnComponent);
        components = components2;
    }

    float timer;

    void Update()
    {
        // TODO: Replace once waves are better understood.
        timer += Time.deltaTime;

        if (timer > testRespawnTime)
        {
            timer = 0;
            SpawnEnemies(5);
        }
    }
}
