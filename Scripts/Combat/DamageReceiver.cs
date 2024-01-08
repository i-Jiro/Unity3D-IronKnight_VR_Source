using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    public delegate void OnHitEventHandler();
    public event OnHitEventHandler OnHit;
    [HideInInspector]public HealthHandler healthHandler;
    
    public void TakeDamage(float damage)
    {
        OnHit?.Invoke();
        healthHandler.TakeDamage(damage);
    }
}
