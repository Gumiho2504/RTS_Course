
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
    using System.Collections.Generic;

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Attack Target", story: "[Self] attacks [target] until it dies.", category: "Action", id: "63d30f00c65d7632cb2740c44bb34697")]
    public partial class AttackTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<AttackConfigSO> AttackConfig;
        [SerializeReference] public BlackboardVariable<List<GameObject>> NearbyEnemies;

        private NavMeshAgent navMeshAgent;
        private Transform selfTransform;
        private Animator animator;

        private Transform targetTransform;
        private IDamageable targetDamageable;

        private float lastAttackTime;
        private AbstractUnit unit;
        private Collider[] enemyColliders;

        protected override Status OnStart()
        {


            if (!HasValidInput()) return Status.Failure;

            navMeshAgent = Self.Value.GetComponent<NavMeshAgent>();
            selfTransform = Self.Value.transform;

            unit = selfTransform.GetComponent<AbstractUnit>();
            animator = selfTransform.GetComponent<Animator>();

            targetTransform = Target.Value.transform;
            targetDamageable = Target.Value.GetComponent<IDamageable>();

            if (AttackConfig.Value.IsAreaOfEffect)
            {
                enemyColliders = new Collider[AttackConfig.Value.MaxEnemiesHitPerAttack];
            }

            if (!NearbyEnemies.Value.Contains(Target.Value))
            {
                navMeshAgent.SetDestination(targetTransform.position);
                navMeshAgent.isStopped = false;
                animator?.SetBool(AnimationConstants.ATTACK, false);
            }


            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Target.Value == null || targetDamageable.CurrentHealth == 0) return Status.Success;
            if (animator != null)
            {
                animator.SetFloat(AnimationConstants.SPEED, navMeshAgent.velocity.magnitude);
            }

            if (!NearbyEnemies.Value.Contains(Target.Value))
            {
                return Status.Running;
            }

            navMeshAgent.isStopped = true;

            LookAtTarget();

            animator?.SetBool(AnimationConstants.ATTACK, true);

            if (Time.time >= lastAttackTime + AttackConfig.Value.AttackDelay)
            {
                lastAttackTime = Time.time;
                ApplyDamage();

            }
            return Status.Running;
        }

        private void LookAtTarget()
        {
            Quaternion lookRotation = Quaternion.LookRotation((targetTransform.position - selfTransform.position).normalized, Vector3.up);
            selfTransform.rotation = Quaternion.Euler(selfTransform.eulerAngles.x, lookRotation.eulerAngles.y, selfTransform.eulerAngles.z);
        }

        protected override void OnEnd()
        {
            animator?.SetBool(AnimationConstants.ATTACK, false);
            if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = false;
            }
            if (unit.AttackingParticleSystem != null)
                unit.AttackingParticleSystem.Stop();
        }


        private void ApplyDamage()
        {
            if (unit.AttackingParticleSystem != null)
            {
                unit.AttackingParticleSystem.Play();
            }
            if (AttackConfig.Value.HasProjectileAttacks) return;

            targetDamageable.TakeDamage(AttackConfig.Value.Damage);
            // projectile attacks are handle by the specific subclass of AbstractUnit that shoot the projectile.



            if (!AttackConfig.Value.IsAreaOfEffect) return;

            int hits = Physics.OverlapSphereNonAlloc(targetTransform.position, AttackConfig.Value.AreaOfEffectRadius, enemyColliders, AttackConfig.Value.DamageableLayers);
            for (int i = 0; i < hits; i++)
            {
                if (enemyColliders[i].TryGetComponent<IDamageable>(out IDamageable nearbyEnemy) && nearbyEnemy != targetDamageable)
                {
                    nearbyEnemy?.TakeDamage(AttackConfig.Value.CalculateAreaOfEffectDamage(targetTransform.position, nearbyEnemy.Transform.position));
                }
            }

        }

        private bool HasValidInput() => Self.Value != null
                                        && Self.Value.TryGetComponent<NavMeshAgent>(out NavMeshAgent _)
                                        && Self.Value.TryGetComponent<AbstractUnit>(out AbstractUnit _)
                                        && Target.Value != null
                                        && Target.Value.TryGetComponent<IDamageable>(out IDamageable _)
                                        && AttackConfig.Value != null
                                        && NearbyEnemies.Value != null && 1 > 0;
    }
}


