using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveKill : MonoBehaviour
{
    public int ID;
    private HealthHandler _healthHandler;
    
    public void Start()
    {
        _healthHandler = GetComponent<HealthHandler>();
        if (_healthHandler)
        {
            _healthHandler.OnDeath += ObjectiveClear;
        }
        else
        {
            Debug.LogWarning("Could not find health handler on current object.");
        }
    }

    public void ObjectiveClear()
    {
        if (MissionManager.Instance)
        {
            MissionManager.Instance.CompleteObjective(ID);
        }
    }

    private void OnDestroy()
    {
        if (_healthHandler)
        {
            _healthHandler.OnDeath -= ObjectiveClear;
        }
    }
}
