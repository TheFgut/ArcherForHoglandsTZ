using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private Vector2 spawnField;
    [SerializeField] private Zombie zombiePrefab;

    private DiContainer container;
    [Inject]
    public void Init(DiContainer container)
    {
        this.container = container;
    }

    private int spawnCount;

    [ContextMenu("Spawn")]
    public void Spawn()
    {
        GameObject spawnedZombie = container.InstantiatePrefab(zombiePrefab);
        spawnedZombie.name = zombiePrefab.name + spawnCount.ToString();
        spawnedZombie.transform.position = transform.position +
            new Vector3(Random.Range(0, spawnField.x), Random.Range(0, spawnField.y));

        spawnCount++;
    }
}
