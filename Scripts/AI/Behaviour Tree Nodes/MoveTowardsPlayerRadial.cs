using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using BehaviorDesigner.Runtime;
using Core.AI;
using DG.Tweening;
using RootMotion.FinalIK;
using Unity.Mathematics;

public class MoveTowardsPlayerRadial : EnemyAction
{
    public SharedGameObject Target;
    public float turnDuration = 0.75f;
    public float RadiusFromTarget;
    [BehaviorDesigner.Runtime.Tasks.Tooltip("Enables finding a position within radius.")]
    public bool WithInRadiusRange = false;
    [BehaviorDesigner.Runtime.Tasks.Tooltip("Used only when above is enabled.")]
    public float MinRadius = 1f;

    public bool OverrideSpeed = false;
    public float Speed = 3.5f;
    private float _originalSpeed;

    private static readonly int _moving = Animator.StringToHash(_isMoving);
    private const string _isMoving = "IsMoving";
    
    private RotationLimitHinge _rotationLimitHinge;

    public override void OnAwake()
    {
        _rotationLimitHinge = GetComponent<RotationLimitHinge>();
        base.OnAwake();
    }

    public override void OnStart()
    {
        if (brain.Agent == null) return;
        if (brain.DefenseStance == null) return;
            
        //brain.DefenseStance.DisableTracking();

        var direction = (Target.Value.transform.position - transform.position).normalized;
        var forwardDot = Vector3.Dot(Target.Value.transform.forward, direction);
        var rightDot =  Vector3.Dot(Target.Value.transform.right, direction);
        Vector3 destination;

        float randRange;
        //Find a random point along the unit circle relative to directon.
        if (forwardDot >= 0)
        {
            randRange = rightDot >= 0f
                ? UnityEngine.Random.Range(0, Mathf.PI * 0.5f) //Q1
                : UnityEngine.Random.Range(Mathf.PI * 0.5f, Mathf.PI); //Q2
        }
        else
        {
            randRange = rightDot >= 0f
                ? UnityEngine.Random.Range((3 * Mathf.PI) * 0.5f, 2 * Mathf.PI) //Q3
                : UnityEngine.Random.Range(Mathf.PI, (3 * Mathf.PI) * 0.5f); //Q4
        }
        
        var targetPosition = Target.Value.transform.position;
        if (WithInRadiusRange)
        {
            var randRadius = UnityEngine.Random.Range(MinRadius, RadiusFromTarget);
            destination =
                new Vector3(targetPosition.x + randRadius * Mathf.Cos(randRange),
                    targetPosition.y,
                    targetPosition.z + randRadius * Mathf.Sin( randRange));
        }
        else
        {
            destination =
                new Vector3(targetPosition.x + RadiusFromTarget * Mathf.Cos(randRange),
                    targetPosition.y,
                    targetPosition.z + RadiusFromTarget * Mathf.Sin( randRange));
        }

        if (OverrideSpeed)
        {
            _originalSpeed = brain.Agent.speed;
            var relativeAnimSpeed = Speed / _originalSpeed;
            brain.Animator.speed = relativeAnimSpeed;
            brain.Agent.speed = Speed;
        }

        //_rotationLimitHinge.useLimits = false;
        brain.Agent.isStopped = false;
        brain.Agent.updateRotation = false;
        transform.DOLookAt(Target.Value.transform.position, turnDuration, AxisConstraint.Y)
            .SetUpdate(UpdateType.Normal);
        brain.Animator.applyRootMotion = false;
        brain.Agent.SetDestination(destination);
    }

    //TODO: if the agent can't reach the exact point, task will not complete. Multiple agents for example.
    public override TaskStatus OnUpdate()
    {
        if (brain.Agent == null)
        {
            Debug.LogWarning($"{this.transform.name}: Nav Mesh Agent was not found!");
            return TaskStatus.Failure;
        }
            
        if (brain.Agent.pathPending) return TaskStatus.Running;
        if (brain.Agent.isOnOffMeshLink) return TaskStatus.Running;

        brain.Animator.SetBool(_moving, brain.Agent.velocity.magnitude > 0.01f);
        if (brain.Agent.remainingDistance <= brain.Agent.stoppingDistance)
        {
            //brain.DefenseStance.EnableTracking();
            brain.Agent.isStopped = true;
            brain.Agent.updateRotation = true;
            brain.Animator.SetBool(_moving, false);
            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }
    
    public override void OnEnd()
    {
        if (OverrideSpeed)
        {
            brain.Animator.speed = 1;
            brain.Agent.speed = _originalSpeed;
        }
        brain.Agent.updateRotation = true;
        brain.Animator.applyRootMotion = true;
        brain.Animator.SetBool(_moving, false);
        //_rotationLimitHinge.useLimits = true;
        base.OnEnd();
    }
}
