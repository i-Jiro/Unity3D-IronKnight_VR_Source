using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracker : MonoBehaviour
{
    public bool DebugMode = false;
    [Tooltip("Origin of tracking sphere. Local space of component.")]
    public Vector3 TrackOrigin = Vector3.zero;
    [Tooltip("Tracking sphere radius.")]
    public float TrackRadius = 1f;
    public LayerMask LayerMask;
    public int MaxColliders = 10;
    public bool IsEnabled = false;
    private bool _hasFoundWeapon;
    public bool HasFoundWeapon => _hasFoundWeapon;

    [HideInInspector] public GameObject TrackedWeapon { get; private set; }


    private void FixedUpdate()
    {
        if (!IsEnabled) return;
        var colliders = new Collider[MaxColliders];
        int numOfColliders = Physics.OverlapSphereNonAlloc(transform.TransformPoint(TrackOrigin), TrackRadius, colliders, LayerMask);
        if (numOfColliders <= 0)
        {
           TrackedWeapon = null;
           _hasFoundWeapon = false;
           return;
        }

        for (int i = 0; i < numOfColliders; i++)
        {
           if (colliders[i].gameObject.CompareTag("HeldWeapon"))
           {
               TrackedWeapon = colliders[i].gameObject;
               _hasFoundWeapon = true;
               return;
           }
        }
    }

    private void OnDrawGizmos()
    {
        if (!DebugMode) return;
        if (!enabled) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(TrackOrigin, TrackRadius);
        if (TrackedWeapon != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.InverseTransformPoint(TrackedWeapon.transform.position), TrackOrigin);
        }
    }
}
