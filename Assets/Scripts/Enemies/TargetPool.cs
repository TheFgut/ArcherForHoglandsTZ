using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TargetPool
{
    private List<IHasFraction>[] pools;


    public TargetPool()
    {
        var values = Enum.GetValues(typeof(Fraction));
        pools = new List<IHasFraction>[values.Length];
        for (int i = 0; i < values.Length;i++)
        {
            pools[i] = new List<IHasFraction>();
        }
    }

    public void RegisterTargetInPool(IHasFraction target)
    {
        pools[(int)target.fraction].Add(target);
    }

    public void RemoveTargetFromPool(IHasFraction target)
    {
        pools[(int)target.fraction].Remove(target);
    }

    public GameObject getNearestEnemy(Fraction enemyFraction, Vector3 pos)
    {
        float minDist = float.MaxValue;
        GameObject nearestEnemy = null;
        foreach (var enemy in pools[(int)enemyFraction])
        {
            float distToEnemy = (enemy.position - pos).magnitude;
            if (distToEnemy < minDist)
            {
                minDist = distToEnemy;
                nearestEnemy = enemy.gameObject;
            }
        }
        return nearestEnemy;
    }
}

public interface IHasFraction
{
    public Fraction fraction { get; }

    public Vector3 position { get; }

    public GameObject gameObject { get; }
}


public enum Fraction
{
    Pig,
    Zombie
}
