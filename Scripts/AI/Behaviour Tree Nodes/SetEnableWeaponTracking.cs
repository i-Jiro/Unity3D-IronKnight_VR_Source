using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class SetEnableWeaponTracking : Action
{
    public bool SetEnable = true;
    private NPCBrain _npcBrain;

    public override void OnAwake()
    { 
        _npcBrain = GetComponent<NPCBrain>();
    }

    public override TaskStatus OnUpdate()
    {
        if (_npcBrain == null)
        {
            Debug.LogWarning($"{gameObject.name}: NPC brain was null!");
            return TaskStatus.Failure;
        }
        
        if (_npcBrain.Tracker == null)
        {
            Debug.LogWarning($"{gameObject.name}: Tracker was null!");
            return TaskStatus.Failure;
        }

        _npcBrain.Tracker.enabled = SetEnable;
        _npcBrain.DefenseStance.DisableTracking();
        return TaskStatus.Success;
    }
}
