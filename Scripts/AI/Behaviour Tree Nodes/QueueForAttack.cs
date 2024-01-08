using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Core.AI
{
    public class QueueForAttack : Action
    {
        //Time inbetween in attempt to queue itself for attack.
        public float Cooldown = 3f;
        private float _timer;
        private NPCBrain _npcBrain;
        
        public override void OnAwake()
        {
            _npcBrain = GetComponent<NPCBrain>();
        }

        public override void OnStart()
        {
            _timer = Cooldown;
        }
        public override TaskStatus OnUpdate()
        {
            if (_npcBrain == null)
            {
                Debug.LogWarning($"{gameObject.name}: NPC brain was null!");
                return TaskStatus.Failure;
            }

            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                if (AIDirector.Instance == null)
                {
                    Debug.LogWarning($"{gameObject.name}: AI director instance was null/not found. ");
                    return TaskStatus.Failure;
                }
                if (AIDirector.Instance.TryAddToAttackQueue(_npcBrain))
                {
                    return TaskStatus.Success;
                }

                _timer = Cooldown;
            }
            
            return TaskStatus.Running;
        }
    }
}
