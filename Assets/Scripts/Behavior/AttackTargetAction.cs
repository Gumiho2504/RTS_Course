
namespace Gumiho_Rts.Behavoir
{
    using System;
    using Unity.Behavior;
    using UnityEngine;
    using Action = Unity.Behavior.Action;
    using Unity.Properties;
    using Gumiho_Rts.Units;
    using UnityEngine.AI;
    using Gumiho_Rts.Utilities;

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Attack Target", story: "[Self] attacks [target] until it dies.", category: "Action", id: "63d30f00c65d7632cb2740c44bb34697")]
    public partial class AttackTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<AttackConfigSO> AttackConfig;

        private NavMeshAgent navMeshAgent;
        private Transform selfTransform;
        private Animator animator;

        private Transform targetTransform;
        private IDamageable targetDamageable;

        private float lastAttackTime;

        protected override Status OnStart()
        {
            if (!HasValidInput()) return Status.Failure;

            navMeshAgent = Self.Value.GetComponent<NavMeshAgent>();
            selfTransform = Self.Value.transform;
            animator = selfTransform.GetComponent<Animator>();

            targetTransform = Target.Value.transform;
            targetDamageable = Target.Value.GetComponent<IDamageable>();

            if (animator != null)
            {
                animator.SetBool(AnimationConstants.ATTACK, true);
            }
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if(Target.Value == null|| targetDamageable.CurrentHealth == 0) return Status.Success;

            if (Vector3.Distance(targetTransform.position, selfTransform.position) <= AttackConfig.Value.AttackRange)
            {
                navMeshAgent.SetDestination(targetTransform.position);
                navMeshAgent.isStopped = false;
                return Status.Running;
            }

            navMeshAgent.isStopped = true;

            if(Time.time >= lastAttackTime + AttackConfig.Value.AttackDelay)
            {
                lastAttackTime = Time.time;
                targetDamageable.TakeDamage(AttackConfig.Value.Damage);
            }
            return Status.Running;
        }

        protected override void OnEnd()
        {
            animator.SetBool(AnimationConstants.ATTACK, false);
        }

        private bool HasValidInput() => Self.Value != null
                                        && Self.Value.TryGetComponent<NavMeshAgent>(out NavMeshAgent _)
                                        && Target.Value != null
                                        && Target.Value.TryGetComponent<IDamageable>(out IDamageable _)
                                        && AttackConfig.Value != null;
    }
}


