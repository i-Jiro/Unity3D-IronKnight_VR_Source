using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObjectiveTriggerZone : MonoBehaviour
{
    [Tooltip("Objective ID in the Mission Manager to complete.")]
    public int ID;
    [HideInInspector]public bool IsCompleted;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!MissionManager.Instance) {Debug.Log("Mission Mananger instance was not found."); return;}
            MissionManager.Instance.CompleteObjective(ID);
            gameObject.SetActive(false);
        }
    }
}
