using System;
using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Serialization;

public class HealthHandler : MonoBehaviour
{
    public float StartingHealthPoints = 100f;
    public float CurrentHealthPoints = 100f;
    [HideInInspector] public bool IsDead = false;

    public delegate void OnDamageEventHandler();
    public event OnDamageEventHandler OnDamage;
    public delegate void OnDeathEventHandler();
    public event OnDeathEventHandler OnDeath;
    
    public void TakeDamage(float damage)
    {
        CurrentHealthPoints -= damage;
        if (CurrentHealthPoints < 0)
            CurrentHealthPoints = 0;
        OnDamage?.Invoke();
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        CurrentHealthPoints = StartingHealthPoints;
        IsDead = false;
        Timing.RunCoroutine(_HealthCheck(), Segment.Update);
    }

    private IEnumerator<float> _HealthCheck()
    {
        while (CurrentHealthPoints > 0)
        {
            yield return Timing.WaitForOneFrame;
        }
        OnDeath?.Invoke();
        IsDead = true;
    }
}
