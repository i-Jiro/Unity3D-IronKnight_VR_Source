using System;
using System.Collections.Generic;
using System.Linq;
using HurricaneVR.Framework.Core;
using UnityEngine;
using MEC;
using RootMotion.Dynamics;

public class BladedWeapon : MonoBehaviour
{
    public bool Enabled = true;
    private float _velocity;
    [SerializeField]private float _baseDamage = 10.0f;
    public float BaseDamage => _baseDamage;
    [SerializeField] private float _impactForce;
    public float ImpactForce => _impactForce;

    [Min(0)] [Tooltip("Minimum velocity needed to slash on collision impact.")]
    [SerializeField] private float _velocityThreshold;
    [SerializeField] HVRGrabbable _grabbable;
    [Header("Collisions")]
    [SerializeField] private BoxCollider _baseBladeCollider;
    [Tooltip("Colliders to trigger slash through.")]
    [SerializeField] private BoxCollider[] _edgeColliders;
    [Tooltip("Colliders to ignore on hit.")]
    [SerializeField] private Collider[] _collidersToIgnore;
    [Tooltip("Layers to check for in overlap.")]
    [SerializeField] private LayerMask _layerMask;
    [InspectorName("Edge Collider Contact Offset")]
    [SerializeField] private float _edgeContactOffset = 0.001f;
    [Range(0f, 1f)]
    [SerializeField] private float _dotThreshold = 0.001f;

    [SerializeField] private GameObject _sparkPrefab;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _hitSoundClip;
    [HideInInspector] public bool IsHeld => _grabbable.IsBeingHeld; 
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_grabbable == null)
            _grabbable = GetComponent<HVRGrabbable>();
        foreach (var edgeCollider in _edgeColliders)
        {
            if(edgeCollider.contactOffset > _edgeContactOffset)
                edgeCollider.contactOffset = _edgeContactOffset;
            if(edgeCollider.transform.localScale != Vector3.one)
                Debug.LogWarning(_baseBladeCollider.name + "'s transform scale is not 1,1,1. Inaccuracy can occur on overlap.");
        }
        
        if(_baseBladeCollider.transform.localScale != Vector3.one)
            Debug.LogWarning(_baseBladeCollider.name + "'s transform scale is not 1,1,1. Inaccuracy can occur on overlap.");
    }

    private void Start()
    {
        if(PoolManager.Instance != null)
            PoolManager.WarmPool(_sparkPrefab, 5);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!Enabled) return;
        var impactVelocity = collision.relativeVelocity.magnitude;
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (!_audioSource.isPlaying)
            {
                _audioSource.PlayOneShot(_hitSoundClip);
            }
            var contact = collision.contacts[0];
            if (!VelocityCheck(impactVelocity)) return;
            
            var broadcaster = collision.rigidbody.GetComponent<MuscleCollisionBroadcaster>();
            var receiver = collision.rigidbody.GetComponent<DamageReceiver>();
            
            receiver.TakeDamage(_baseDamage);
            broadcaster.Hit(100f, _impactForce * -contact.normal, contact.point);
            
            var spark = PoolManager.SpawnObject(_sparkPrefab, contact.point, Quaternion.identity);
            spark.transform.forward = -contact.normal;
        }
        //TODO: Hacky way of playing spark particles on prop. Need change.
        else if (collision.gameObject.name.Contains("Prop"))
        {
            var contact = collision.contacts[0];
            var spark = PoolManager.SpawnObject(_sparkPrefab, contact.point, Quaternion.identity);
            spark.transform.forward = -contact.normal;
        }
        
        //Mesh Cutting Start
        if (!collision.gameObject.CompareTag("Cuttable")) return;
        if (!VelocityCheck(impactVelocity)) return;
        
        //Check if the collisions if where caused by edge colliders.
        for (int i = 0; i < collision.contactCount; i++)
        {
            var contact = collision.GetContact(i);
            foreach (var edge in _edgeColliders)
            {
                if (contact.thisCollider == edge)
                {
                    //Check if the impact was from the forward direction of edge collider
                    var dot = Vector3.Dot(edge.transform.forward, -contact.normal);
                    if (dot >= _dotThreshold)
                        Timing.RunCoroutine(IgnoreCollisions(collision), Segment.Update);
                    break;
                }
            }
        }
    }
    
    //TODO: Change these methods. There is a better way. PLEASE FIX ASAP
    public void UpdateHoldTag()
    {
        gameObject.tag = "HeldWeapon";
        _baseBladeCollider.tag = "HeldWeapon";
        foreach(var edge in _edgeColliders)
        {
            edge.tag = "HeldWeapon";
        }
    }

    public void UpdateDropTag()
    {
        gameObject.tag = "Weapon";
        _baseBladeCollider.tag = "Weapon";
        foreach(var edge in _edgeColliders)
        {
            edge.tag = "Weapon";
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = UnityEngine.Matrix4x4.TRS(_baseBladeCollider.transform.position, _baseBladeCollider.transform.rotation,
            _baseBladeCollider.transform.lossyScale);
        if(_baseBladeCollider != null)
            Gizmos.DrawWireCube(_baseBladeCollider.center, _baseBladeCollider.size);
    }
    
    //Check if the weapon has moved fast enough to cut.
    private bool VelocityCheck(float relativeVelocity)
    {
        //Is player holding weapon, otherwise return false.
        if (!_grabbable.IsHandGrabbed)
        { return false;}
        
        return relativeVelocity > _velocityThreshold;
    }
    
    //Output a list of colliders that overlap dimensions of the edge and base colliders.
    //Returns if there was any collision overlaps.
    private bool OverlapCollisions(out Collider[] colliders)
    {
        colliders = Array.Empty<Collider>();
        for(int i = 0; i < _edgeColliders.Length; i++)
        {
            Vector3 extent = _edgeColliders[i].size * 0.50f;
            Vector3 center = _edgeColliders[i].transform.TransformPoint(_edgeColliders[i].center);
            Quaternion rot = _edgeColliders[i].transform.rotation;
            var cols = Physics.OverlapBox(center, extent, rot, _layerMask);
            colliders = colliders.Union(cols).ToArray();
        }
        
        Vector3 halfExtents = _baseBladeCollider.size * 0.50f;
        Vector3 position = _baseBladeCollider.transform.TransformPoint(_baseBladeCollider.center);
        Quaternion orientation = _baseBladeCollider.transform.rotation;
        var baseCols = Physics.OverlapBox(position, halfExtents, orientation, _layerMask);
        colliders = colliders.Union(baseCols).ToArray();

        return colliders.Length > 0;
    }
    
    
    // Temporarily disable physical collision between the weapon and the collided object.
    IEnumerator<float> IgnoreCollisions(Collision collision)
    {
        Vector3 position = _baseBladeCollider.transform.TransformPoint(_baseBladeCollider.center);

        //Cache impact
        Vector3 pointOfImpact = position;
        Vector3 impactNormal = _baseBladeCollider.transform.up;
        Collider impactedCollider = collision.collider;
        
        Physics.IgnoreCollision(_baseBladeCollider,impactedCollider, true);
        foreach(var edge in _edgeColliders)
        {
            Physics.IgnoreCollision(edge, impactedCollider, true);
        }
        foreach (var collider in _collidersToIgnore)
        {
            Physics.IgnoreCollision(collider, impactedCollider, true);
        }
        
        
        Physics.SyncTransforms();
        OverlapCollisions(out var totalColliders);
        //Boxcast based on the edge/base colliders sizes to check if it's inside another collider.
        //Check until it isn't inside before re-enabling collisions again.
        while (totalColliders.Length > 0)
        {
            if (!totalColliders.Contains(impactedCollider))
            {
                break;
            }
            Physics.SyncTransforms();
            OverlapCollisions(out totalColliders);
            yield return Timing.WaitForOneFrame;
        }

        Physics.IgnoreCollision(_baseBladeCollider, impactedCollider, false);
        foreach(var edge in _edgeColliders)
        {
            Physics.IgnoreCollision(edge, impactedCollider, false);
        }
        foreach (var collider in _collidersToIgnore)
        {
            Physics.IgnoreCollision(collider, impactedCollider, false);
        }
        WeaponMeshCutter.Instance.Cut(impactNormal, pointOfImpact);
    }
}
