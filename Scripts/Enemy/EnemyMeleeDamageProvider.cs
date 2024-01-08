using System;
using System.Collections;
using System.Collections.Generic;
using HurricaneVR.Framework.Components;
using RootMotion.Dynamics;
using Unity.VisualScripting;
using UnityEngine;

//Placeholder component for prototyping combat system.
public class EnemyMeleeDamageProvider : MonoBehaviour
{
    [SerializeField]
    private GameObject _sparkPrefab;
    [SerializeField]
    private float _baseDamage = 10f;
    public bool IsAttacking = false;
    public bool IsParryableState = false;
    [HideInInspector]
    public EnemyWeaponController WeaponController;
    
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log($"{collision.gameObject.name}: Hit");
        var contact = collision.contacts[0];
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!IsAttacking) return;
            //Debug.Log($"Hit player.{collision.gameObject.name} ");
            var receiver = collision.gameObject.GetComponent<DamageReceiver>();
            WeaponController.PlayMeleeHitSound();
            if (receiver != null)
            {
                receiver.TakeDamage(_baseDamage);
            }
        }
        else if (collision.gameObject.CompareTag("HeldWeapon"))
        {
            if (IsParryableState)
            {
                WeaponController.Parried();
            }
            WeaponController.PlayGuardSound();
        }
        
        var spark = PoolManager.SpawnObject(_sparkPrefab, contact.point, Quaternion.identity);
        spark.transform.forward = -contact.normal;
    }
}
