using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = Unity.Mathematics.Random;

namespace Core.AI
{
    public class Defend : Action
    {
        public SharedGameObject PlayerTarget;
        public SharedGameObject WeaponTarget;
        public SharedFloat MinDistance;

        private NPCBrain _npcBrain;
        private NavMeshAgent _agent;
        private DefenseStance _defenseStance;

        public bool CanDodge = true;
        private float dodgeCooldown = 10f;
        private float _dodgeTimer;

        private MechSoundController _audioController;

        public override void OnAwake()
        {
            _npcBrain = GetComponent<NPCBrain>();
            _dodgeTimer = dodgeCooldown;
            _audioController = GetComponent<MechSoundController>();
        }

        public override void OnStart()
        {
            if (_npcBrain == null) return;
            _npcBrain.DefenseStance.EnableTracking();
        }


        public override TaskStatus OnUpdate()
        {
            if (_npcBrain == null)
            {
                Debug.LogWarning($"{gameObject.name}: NPC brain was null!");
                return TaskStatus.Failure;
            }

            if(CanDodge)
                _dodgeTimer -= Time.deltaTime;
            
            if (_npcBrain.Tracker.enabled)
            {
                WeaponTarget.Value = _npcBrain.Tracker.TrackedWeapon;
                if (WeaponTarget.Value == null)
                {
                    _npcBrain.DefenseStance.RemoveTarget();
                    _npcBrain.LookController.LookAt(PlayerTarget.Value);
                    _npcBrain.LookController.AimAt(PlayerTarget.Value);
                }
                else
                {
                    if (_dodgeTimer <= 0 && CanDodge)
                    {
                        int rand = UnityEngine.Random.Range(1, 4);
                        switch (rand)
                        {
                            case(1): // Forward
                                _npcBrain.Animator.SetTrigger("Dodge1");
                                break;
                            case(2): // Left
                                _npcBrain.Animator.SetTrigger("Dodge2");
                                break;
                            case(3): // Right
                                _npcBrain.Animator.SetTrigger("Dodge3");
                                break;
                        }
                        //TODO: Do audio elsewhere.
                        _audioController.PlayDodge();
                        _dodgeTimer = dodgeCooldown;
                    }
                    else
                    {
                        _npcBrain.DefenseStance.SetTarget(WeaponTarget.Value.gameObject);
                        _npcBrain.LookController.LookAt(WeaponTarget.Value);
                        _npcBrain.LookController.AimAt(WeaponTarget.Value);
                    }
                }
            }
            
            var currentDistance = (PlayerTarget.Value.transform.position - this.transform.position).magnitude;
            return MinDistance.Value < currentDistance ?
                //Exit out of behaviour if player is out of range.
                TaskStatus.Success : TaskStatus.Running;
        }
        
        public override void OnConditionalAbort()
        {
            OnEnd();
            _npcBrain.Tracker.enabled = false;
            base.OnConditionalAbort();
        }

        public override void OnEnd()
        {
            _npcBrain.SendEvent("DefendEnd");
            _npcBrain.LookController.LookAt(PlayerTarget.Value);
            _npcBrain.LookController.AimAt(PlayerTarget.Value);
            _npcBrain.DefenseStance.DisableTracking();
        }
    }
}
