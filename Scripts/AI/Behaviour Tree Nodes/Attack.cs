using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.AI
{
    public class Attack : Action
    {
        public string AttackTriggerName = "";
        
        private NPCBrain _npcBrain;
        private static int _attackString;
        private bool _isActive = false;
        public SharedGameObject playerTarget;
        public float turnDuration = 0.5f;

        public override void OnAwake()
        {
            _npcBrain = GetComponent<NPCBrain>();
            _attackString = Animator.StringToHash(AttackTriggerName);
        }

        public override void OnStart()
        {
            _isActive = true;
            if (_npcBrain == null) return;
            _npcBrain.OnAttackEnd += AttackEnd;
            _npcBrain.OnAttackStart += AttackStart;
            _npcBrain.Weapon.OnParried += Parried;
            
            Physics.IgnoreLayerCollision(7, 9, true);
            //Aim IK needs to be disabled for attack animation to correctly play.
            _npcBrain.LookController.AimAt(null);
            _npcBrain.Animator.SetTrigger(_attackString);

            transform.DOLookAt(playerTarget.Value.transform.position, turnDuration, AxisConstraint.Y)
                .SetUpdate(UpdateType.Normal);
        }

        public override TaskStatus OnUpdate()
        {
            if (_npcBrain == null)
            {
                Debug.LogWarning($"{gameObject.name}: NPC brain was null/not found.");
                return TaskStatus.Failure;
            } 
            return !_isActive ? TaskStatus.Success : TaskStatus.Running;
        }

        public override void OnEnd()
        {
            _npcBrain.OnAttackEnd -= AttackEnd;
            _npcBrain.OnAttackStart -= AttackStart;
            _npcBrain.Weapon.OnParried -= Parried;
            Physics.IgnoreLayerCollision(7, 9, false);
        }

        public void AttackStart()
        {
            //TODO: Activate weapon hitbox here.  
        }
        
        private void AttackEnd()
        {
            _isActive = false;
            //TODO: Deactivate weapon hitbox here.  
        }

        private void Parried()
        {
            AttackEnd();
            _npcBrain.SendEvent("Parried");
        }
    }
}
