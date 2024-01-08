using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using RootMotion.Demos;
using UnityEngine;

namespace Core.AI
{
    public class Parried : Action
    {
        public string ParryAnimationTriggerName = "";
        public float Duration = 1f;
        
        private int _triggerID;
        private NPCBrain _npcBrain;
        private float timer = 0f;

        public override void OnAwake()
        {
            _npcBrain = GetComponent<NPCBrain>();
            base.OnAwake();
        }


        public override void OnStart()
        {
            _triggerID = Animator.StringToHash(ParryAnimationTriggerName);
            _npcBrain.Animator.SetTrigger(_triggerID);
            timer = Duration;
        }

        public override TaskStatus OnUpdate()
        {
            timer -= Time.deltaTime;
            
            return timer <= 0 ? TaskStatus.Success : TaskStatus.Running;
        }
    }
}
