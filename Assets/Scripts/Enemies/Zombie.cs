using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Zombie : MonoBehaviour, IHasFraction, IKillable
{
    public Fraction fraction => Fraction.Zombie;

    public Vector3 position => transform.position;

    private bool _alive = true;

    public bool isAlive => _alive;



    [SerializeField] private Health health;
    [SerializeField] private CircleCollider2D circCollider;

    private TargetPool _targetPool;

    public event IKillable.dieCallback onDie;

    private ILogger logger;

    [Inject]
    public void Init(TargetPool targetPool, ILogger logger)
    {
        this.logger = logger;
        _targetPool = targetPool;
        _targetPool.RegisterTargetInPool(this);
        health.onHealthFinishes += Die;
    }

    private void Die()
    {
        onDie?.Invoke();
        _targetPool.RemoveTargetFromPool(this);
        _alive = false;
        circCollider.enabled = false;
        StartCoroutine(durTimeDestroyer());

    }
    const float timeToDestroy = 3;
    private IEnumerator durTimeDestroyer()
    {
        
        float destroyDur = timeToDestroy;
        do
        {
            destroyDur -= Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero,Vector3.one, destroyDur / timeToDestroy);
            yield return new WaitForEndOfFrame();
        }while (destroyDur > 0);
        Destroy(gameObject);
    }

    public void Hurt(Arrow arrow)
    {
        if (!_alive) return;
        logger.Log(string.Format("{0} recieved damage from \"{1}\"", name, arrow.ToString()));
        health.Decrease(arrow.damage);
    }
}


[System.Serializable]
public class Health
{
    [SerializeField] private float _health;

    [SerializeField] private float _maxValue;
    [SerializeField] private float _minValue;

    public event healthChangeCallback onHealthDecrease;
    public event healthChangeCallback onHealthFinishes;

    public void Decrease(float amount)
    {
        _health -= amount;
        onHealthDecrease?.Invoke();
        if (_health <= 0)
        {
            onHealthFinishes?.Invoke();
        }

    }

    public delegate void healthChangeCallback();
}
