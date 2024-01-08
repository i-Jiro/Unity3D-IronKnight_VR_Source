using System;
using System.Collections;
using System.Collections.Generic;
using HexabodyVR.PlayerController;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private HealthHandler _healthHandler;
    private List<DamageReceiver> _damageReceivers = new List<DamageReceiver>();
    
    public HexaBodyPlayer4 Body;
    public HealthHandler HealthHandler => _healthHandler;
    
    private void Awake()
    {
        if (!_healthHandler)
        {
            _healthHandler = GetComponent<HealthHandler>();
        }
        CreateDamageReceivers();
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
        _healthHandler.OnDeath += Kill;
        _healthHandler.OnDamage += Knockback;
    }
    
    public void Initialize()
    {
        _healthHandler.Initialize();
    }

    private void CreateDamageReceivers()
    {
        if (!_healthHandler)
        {
            Debug.LogWarning("Health Handler was not found.\n Could not create damage receivers.");
            return;
        }
        
        var head = Body.Head.AddComponent<DamageReceiver>();
        head.healthHandler = _healthHandler;
        head.gameObject.tag = "Player";
        
        
        var pelvis = Body.Pelvis.AddComponent<DamageReceiver>();
        pelvis.healthHandler = _healthHandler;
        pelvis.gameObject.tag = "Player";
        
        var knee = Body.KneeCollider.AddComponent<DamageReceiver>();
        knee.healthHandler = _healthHandler;
        knee.gameObject.tag = "Player";
    }

    public void RelocateRig(Vector3 position, Vector3 forwardDirection)
    {
        Body.MoveToPosition(position);
        Body.FaceDirection(forwardDirection);
    }

    public void FreezeRigidBody()
    {
        //Body.LocoBall.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void ReleaseRigidBody()
    {
        Body.LocoBall.constraints = RigidbodyConstraints.None;
    }

    private void Kill()
    {
        //Game Over Screen
    }

    //Knock back player on getting damaged;
    private void Knockback()
    {
        //Body.Pelvis.AddForce(-Body.Pelvis.transform.forward * 10f);
        //Debug.Log("Knockback");
        Body.NormalizeVelocity();
    }

    private void OnDestroy()
    {
        if (_healthHandler)
        {
            _healthHandler.OnDeath -= Kill;
        }
    }
}
