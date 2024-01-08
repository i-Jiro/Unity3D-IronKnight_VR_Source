using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using RootMotion.Demos;
using RootMotion.Dynamics;
using UnityEngine;

public class EnemyWeaponController : MonoBehaviour
{
    public PuppetMasterProp PuppetMasterProp;
    public EnemyMeleeDamageProvider DamageProvider;
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _parryClip;
    [SerializeField] private AudioClip _hitClip;
    [SerializeField] private AudioClip _guardClip;

    public float ActionDuration = 0.75f;

    public delegate void OnParryEventHandler();
    public event OnParryEventHandler OnParried;

    private void Awake()
    {
        _audioSource.GetComponent<AudioSource>();
        if (!PuppetMasterProp)
            PuppetMasterProp = GetComponent<PuppetMasterProp>();
    }

    private void Start()
    {
        DamageProvider.WeaponController = this;
    }

    public void StartMeleeAttack()
    {
        if (PuppetMasterProp is PuppetMasterPropMelee melee)
        {
            melee.StartAction(ActionDuration);
            //TODO: Coroutine to enable when weapon is parryable after certain frames.
            DamageProvider.IsParryableState = true;
            DamageProvider.IsAttacking = true;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Prop was not a melee type!");
        }
    }

    public void EndMeleeAttack()
    {
        DamageProvider.IsAttacking = false;
        DamageProvider.IsParryableState = false;
    }

    //TODO: Change these sound methods to be more elegant.
    public void PlayMeleeHitSound()
    {
        if(!_audioSource.isPlaying)
            _audioSource.PlayOneShot(_hitClip);
    }

    public void PlayGuardSound()
    {
        if(!_audioSource.isPlaying)
            _audioSource.PlayOneShot(_guardClip);
    }

    public void Parried()
    {
        _audioSource.PlayOneShot(_parryClip);
        OnParried?.Invoke();
    }
}
