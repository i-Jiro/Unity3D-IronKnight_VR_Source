using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

namespace Core.AI
{
    public class MoveTowardsPlayer : EnemyAction
    {
        public SharedGameObject Target;
        public float StoppingDistance;

        private float _originalStopDistance;
        private static readonly int _moving = Animator.StringToHash(_isMoving);
        private const string _isMoving = "IsMoving";
        public float TurnDuration = 1f;
        public bool Follow = false;
        
        public override void OnStart()
        {
            if (brain.Agent == null) return;
            if (brain.DefenseStance == null) return;
            
            //brain.DefenseStance.DisableTracking();

            brain.Animator.applyRootMotion = false;
            _originalStopDistance = brain.Agent.stoppingDistance;
            brain.Agent.stoppingDistance = StoppingDistance;
            brain.Agent.updateRotation = false;
            brain.Agent.isStopped = false;
            brain.Agent.SetDestination(Target.Value.transform.position);
            transform.DOLookAt(Target.Value.transform.position, TurnDuration, AxisConstraint.Y)
                .SetUpdate(UpdateType.Normal);
        }

        public override TaskStatus OnUpdate()
        {
            if (brain.Agent == null)
            {
                Debug.LogWarning($"{this.transform.name}: Nav Mesh Agent was not found!");
                return TaskStatus.Failure;
            }

            if (Follow)
            {
                brain.Agent.SetDestination(Target.Value.transform.position);
            }
            
            brain.Animator.SetBool(_moving, brain.Agent.velocity.magnitude > 0.01f);
            
            if (brain.Agent.pathPending) return TaskStatus.Running;
            if (brain.Agent.isOnOffMeshLink) return TaskStatus.Running;

            if (brain.Agent.remainingDistance <= brain.Agent.stoppingDistance)
            {
                brain.Agent.isStopped = true;
                brain.Agent.stoppingDistance = _originalStopDistance;
                brain.Animator.SetBool(_moving, false);
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

        public override void OnConditionalAbort()
        {
            OnEnd();
            base.OnConditionalAbort();
        }

        public override void OnEnd()
        {
            brain.Animator.SetBool(_moving, false);
            brain.Animator.applyRootMotion = true;
            brain.Agent.updateRotation = true;
            base.OnEnd();
        }
    }
}
