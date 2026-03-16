using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using Gumiho_Rts.Utilities;
namespace Gumiho_Rts.Behavior
{


    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "TranslatePosition", story: "[Self] move to [TargetLocation] at [Speed] speed.", category: "Action/Navigation", id: "3b28b7a8147f26ee35ae5dceb9395eb4")]
    public partial class TranslatePositionAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
        [SerializeReference] public BlackboardVariable<float> Speed;
        private Animator animator;
        private float endTime;
        private Vector3 direction;
        private Transform selfTransform;

        protected override Status OnStart()
        {
            if (Self.Value == null) return Status.Failure;
            animator = Self.Value.GetComponent<Animator>();
         
            selfTransform = Self.Value.transform;
            float distance = Vector3.Distance(selfTransform.position, TargetLocation.Value);
            endTime = Time.time + distance / Speed.Value;
            direction = (TargetLocation.Value - selfTransform.position).normalized;
            selfTransform.forward = direction;
            return Status.Running;
        }
        protected override Status OnUpdate()
        {
            if (Time.time > endTime) return Status.Success;
            if (animator != null) animator.SetFloat(AnimationConstants.SPEED, Speed);
            selfTransform.localPosition += direction * Speed * Time.deltaTime;
            return Status.Running;
        }
        protected override void OnEnd()
        {
            if (animator != null)
            {
                animator.SetFloat(AnimationConstants.SPEED, 0);
            }
        
        }


    }


}