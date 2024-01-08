using System.Collections;
using System.Collections.Generic;
using Core.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Core.AI
{
    public class AimAt : EnemyAction
    {
        public SharedGameObject aimTarget;

        public override TaskStatus OnUpdate()
        {
            if (aimTarget.Value == null)
            {
                Debug.LogWarning($"{gameObject.name}: Aim target was null.");
                return TaskStatus.Failure;
            }
            brain.LookController.AimAt(aimTarget.Value.gameObject);
            return TaskStatus.Success;
        }
    }
}
