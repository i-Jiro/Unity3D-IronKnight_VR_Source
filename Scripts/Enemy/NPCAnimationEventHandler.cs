using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Helper component to be placed alongside the animator component.
//Receives calls from animation events and sends it to the root NPC Brain component.
//Hacky way
public class NPCAnimationEventHandler : MonoBehaviour
{
    [SerializeField]
    private NPCBrain _npcBrain;

    public void Awake()
    {
        _npcBrain = GetComponentInParent<NPCBrain>();
    }

    public void Start()
    {
        if(_npcBrain == null)
            Debug.LogWarning("NPC brain was not found/null.");
    }

    public void AttackStart()
    {
        _npcBrain.AttackAnimStart();
    }
    
    public void AttackEnd()
    {
        _npcBrain.AttackAnimEnd();
    }
}
