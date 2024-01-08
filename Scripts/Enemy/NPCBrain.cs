using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using MEC;
using RootMotion.Demos;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

//Ties in all components of the AI to be used in the behaviour tree.
[RequireComponent(typeof(BehaviorTree))]
public class NPCBrain : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private DefenseStance _defense;
    [SerializeField] private Tracker _tracker;
    [SerializeField] private Animator _animator;
    [SerializeField] private TwoHandWeaponHold _ikController;
    [SerializeField] private LookController _lookController;
    [SerializeField]
    private PuppetMaster _puppetMaster;
    private BehaviorTree _behaviorTree;
    [SerializeField] private HealthHandler _healthHandler;
    [SerializeField] private BehaviourPuppet _behaviourPuppet;
    [SerializeField] private AudioSource _explosionAudio;

    public Animator Animator => _animator;
    public NavMeshAgent Agent => _agent;
    public DefenseStance DefenseStance => _defense;
    public Tracker Tracker => _tracker;
    public HealthHandler HealthHandler => _healthHandler;
    public LookController LookController => _lookController;
    
    [Header("Properties")]
    public string MuscleTag = "Enemy";
    [SerializeField] private Booster booster;
    public readonly List<DamageReceiver> DamageReceivers = new List<DamageReceiver>();
    [SerializeField] private EnemyWeaponController _enemyWeapon;

    public EnemyWeaponController Weapon => _enemyWeapon;
    
    [Header("On Death Settings")]
    [Tooltip("Settings for killing and freezing the puppet.")]
    [SerializeField] private PuppetMaster.StateSettings _puppetStateSettings = PuppetMaster.StateSettings.Default;
    [SerializeField] private ParticleSystem _explosionVFX;

    [Tooltip("Time until to remove body.")]
    public float TimeToRemove = 5f;

    public delegate void AttackStartEventHandler();
    public event AttackStartEventHandler OnAttackStart;
    public delegate void AttackEndEventHandler();
    public event AttackEndEventHandler OnAttackEnd;

    private void Awake()
    {
        if(_agent == null)
            _agent = GetComponent<NavMeshAgent>();
        if (_defense == null)
            _defense = GetComponent<DefenseStance>();
        _behaviorTree = GetComponent<BehaviorTree>();
    }

    private void Start()
    {
        if (_agent == null)
        {
            Debug.LogWarning($"{gameObject.name}:Nav Mesh Agent is null!");
            return;
        }
        
        if (_behaviorTree == null)
        {
            Debug.LogWarning($"{gameObject.name}: Behaviour tree is null!");
            return;
        }

        if (AIDirector.Instance != null)
        {
            AIDirector.Instance.RegisterNPC(this);
        }
        else 
            Debug.LogWarning($"{gameObject.name}: Could not find AI director instance.");

        if (_enemyWeapon)
        {
            EquipWeapon(_enemyWeapon);
        }
        
        InitializeDamageReceivers();
        
        _animator.Play("Ready Stance");
        _ikController.ReleaseLeftHand();
        _ikController.weight = 0.50f;
        //_behaviorTree.EnableBehavior();
    }

    private void InitializeDamageReceivers()
    {
        if (_puppetMaster == null)
        {
            Debug.LogWarning($"{gameObject.name}: Puppet master was null.\n Unable to add damage receivers.");
            return;
        }
        foreach (var muscle in _puppetMaster.muscles)
        {
            //Exclude props.
            if (muscle.rigidbody.gameObject.name.Contains("Prop"))
            {
                continue;
            }
            muscle.rigidbody.gameObject.tag = MuscleTag;
            var reciever = muscle.rigidbody.AddComponent<DamageReceiver>();
            reciever.healthHandler = _healthHandler;
            _healthHandler.OnDeath += Kill;
            DamageReceivers.Add(reciever);
        }
    }

    public void Activate()
    {
        Timing.RunCoroutine(_Activate(), Segment.Update);
    }
    private IEnumerator<float> _Activate()
    {
        _animator.SetBool("IsReady", true);
        _ikController.SetWeight(1,0.25f);
        _ikController.TightenLeftHand();
        yield return Timing.WaitForSeconds(2f);
        _ikController.AimEnabled = true;
        _ikController.LookEnabled = true;
        _behaviorTree.EnableBehavior();
    }
    
    public void Kill()
    {
        _behaviorTree.DisableBehavior();
        _puppetMaster.Kill(_puppetStateSettings);
        Timing.RunCoroutine(_Kill(), Segment.Update);
    }

    //Reactivate
    public void Resurrect()
    {
        gameObject.SetActive(true);
        _behaviorTree.EnableBehavior();
        _healthHandler.Initialize();
    }
    
    public void OnFall()
    {
        throw new NotImplementedException();
    }

    public void OnGetUp()
    {
        throw new NotImplementedException();
    }

    public void EquipWeapon(EnemyWeaponController weapon)
    {
        if (_puppetMaster.propMuscles.Length <= 0)
        {
            Debug.LogWarning("No prop muscles found to equip weapon!");
        }
        
        //Assuming there's only 1 prop muscle on the character for now.
        _puppetMaster.propMuscles[0].currentProp = _enemyWeapon.PuppetMasterProp;
        if (_enemyWeapon.PuppetMasterProp is PuppetMasterPropMelee)
        {
            weapon.DamageProvider = _puppetMaster.propMuscles[0].GetComponent<EnemyMeleeDamageProvider>();
        }
        _enemyWeapon = weapon;
    }
    
    //Called by AI Director 
    public void SendEvent(string eventName)
    {
        _behaviorTree.SendEvent(eventName);
    }

    public void AttackAnimStart()
    {
        //_behaviourPuppet.BoostImmunity(100f);
        _behaviourPuppet.deactivated = true;
        booster.Boost(_behaviourPuppet);
        _enemyWeapon.StartMeleeAttack();
        OnAttackStart?.Invoke();
    }

    public void AttackAnimEnd()
    {
        _behaviourPuppet.deactivated = false;
        _enemyWeapon.EndMeleeAttack();
        OnAttackEnd?.Invoke();
    }

    private IEnumerator<float> _Kill()
    {
        yield return Timing.WaitForSeconds(TimeToRemove);
        _explosionVFX.transform.position = gameObject.transform.position;
        _explosionVFX.Play();
        _explosionAudio.Play();
        yield return Timing.WaitForSeconds(0.10f);
        //TODO: Pooling
        gameObject.SetActive(false);
        _puppetMaster.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if(_healthHandler)
            _healthHandler.OnDeath -= Kill;
    }
}
