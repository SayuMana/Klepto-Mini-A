using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    private List<AggressiveEnemy> aggressiveEnemies = new List<AggressiveEnemy>();

    void Awake()
    {
        Instance = this;
    }

    public void RegisterAggressive(AggressiveEnemy enemy)
    {
        if (!aggressiveEnemies.Contains(enemy))
            aggressiveEnemies.Add(enemy);
    }

    public void UnregisterAggressive(AggressiveEnemy enemy)
    {
        aggressiveEnemies.Remove(enemy);
    }

    // Kembalikan AggressiveEnemy terdekat dari posisi tertentu
    public AggressiveEnemy GetNearestAggressive(Vector3 fromPosition)
    {
        AggressiveEnemy nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var enemy in aggressiveEnemies)
        {
            float dist = Vector3.Distance(fromPosition, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }

        return nearest;
    }
}