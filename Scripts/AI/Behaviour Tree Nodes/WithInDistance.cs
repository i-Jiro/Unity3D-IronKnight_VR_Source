using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class WithInDistance : Conditional
{
    public SharedGameObject PlayerTarget;
    public float DistanceRange = 1f;
    public bool IgnoreHeight = false;

    public override TaskStatus OnUpdate()
    {
        if (PlayerTarget == null)
        {
            Debug.LogWarning($"{gameObject.name}: player was null." );
            return TaskStatus.Failure;
        }
        
        Vector3 fromTo = PlayerTarget.Value.transform.position - transform.position;
        if(IgnoreHeight)
            fromTo.y = 0;
        float distance = fromTo.magnitude;
        return distance < DistanceRange ? TaskStatus.Success : TaskStatus.Failure;
    }
}
