using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Core.AI
{
    public class LookAt : EnemyAction
    {
        public SharedGameObject lookTarget;

        public override TaskStatus OnUpdate()
        {
            if (lookTarget.Value == null)
            {
                Debug.LogWarning($"{gameObject.name}: Look target was null.");
                return TaskStatus.Failure;
            }
            brain.LookController.LookAt(lookTarget.Value.gameObject);
            return TaskStatus.Success;
        }
    }
}
