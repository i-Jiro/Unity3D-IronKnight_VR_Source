using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Core.AI
{
    public class GetPlayer : Action
    {
        public SharedGameObject playerTarget;

        public override TaskStatus OnUpdate()
        {
            if (AIDirector.Instance == null)
            {
                Debug.LogWarning("Could not find AI director.");
                return TaskStatus.Failure;
            }
            
            playerTarget.Value =  AIDirector.Instance.Player.Body.Camera.gameObject;;
            return TaskStatus.Success;
        }
    }
}
