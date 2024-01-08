using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Core.AI
{
    public class EnemyAction : Action
    {
        protected NPCBrain brain;

        public override void OnAwake()
        {
            brain = GetComponent<NPCBrain>();
            base.OnAwake();
        }

        public override void OnStart()
        {
            if (brain == null)
            {
                Debug.LogWarning($"{transform.gameObject.name}: NPC Brain is null!");
            }
            base.OnStart();
        }
    }
}
