using System.Collections;
using Unity.Behavior;
using UnityEngine;
namespace Gumiho_Rts.Units
{
    public class Grenadier : BaseMilitaryUnit
    {
        [SerializeField] private GameObject grenade;
        [SerializeField] private ParticleSystem explosionParticle;

        private Transform grenadeParent;
        private Vector3 defaultGrenadePosition;
        private Collider[] enemyColliders;

        protected override void Awake()
        {
            base.Awake();
            if (grenade == null || explosionParticle == null)
            {
                Debug.LogError($"Grenadier {name} is missing a grenade or explosion particle! They will not work");
                return;
            }

            defaultGrenadePosition = grenade.transform.localPosition;
            grenadeParent = grenade.transform.parent;
        }

        protected override void Start()
        {
            base.Start();
            enemyColliders = new Collider[unitSO.AttackConfig.MaxEnemiesHitPerAttack];
        }

        // Animation Event
        public void OnThrowGrenade()
        {
            grenade.transform.SetParent(null);
            Vector3 startPosition = grenade.transform.position;
            Vector3 endPosition = grenade.transform.position + grenade.transform.forward * 3f;

            IDamageable damageable = null;

            if (behaviorGraphAgent.GetVariable("TargetGameObject", out BlackboardVariable<GameObject> targetVariable) && targetVariable != null)
            {
                endPosition = targetVariable.Value.transform.position + Vector3.up;
                damageable = targetVariable.Value.GetComponent<IDamageable>();
            }
            else if (behaviorGraphAgent.GetVariable("TargetLocation", out BlackboardVariable<Vector3> targetLocationVariable) && targetLocationVariable != null)
            {
                endPosition = targetLocationVariable;
            }

            StartCoroutine(AnimateGrenadeMovement(startPosition, endPosition, damageable));

        }

        IEnumerator AnimateGrenadeMovement(Vector3 startPosition, Vector3 endPosition, IDamageable damageable)
        {
            float time = 0;
            const float speed = 2;
            while (time < 1)
            {
                grenade.transform.position = Vector3.Lerp(startPosition, endPosition, time);
                time += speed * Time.deltaTime;
                yield return null;
            }


            explosionParticle.transform.SetParent(null);
            explosionParticle.transform.position = endPosition;
            explosionParticle.Play();
            ApplyDamage(endPosition, damageable);

            grenade.transform.SetParent(grenadeParent);
            grenade.transform.localPosition = defaultGrenadePosition;
        }

        private void ApplyDamage(Vector3 endPosition, IDamageable damageable)
        {
            damageable?.TakeDamage(unitSO.AttackConfig.Damage);
            if (unitSO.AttackConfig.IsAreaOfEffect)
            {
                int hits = Physics.OverlapSphereNonAlloc(endPosition, unitSO.AttackConfig.AreaOfEffectRadius, enemyColliders, unitSO.AttackConfig.DamageableLayers);
                for (int i = 0; i < hits; i++)
                {
                    if (enemyColliders[i].TryGetComponent<IDamageable>(out IDamageable nearbyEnemy) && nearbyEnemy != damageable)
                    {
                        nearbyEnemy?.TakeDamage(unitSO.AttackConfig.CalculateAreaOfEffectDamage(endPosition,nearbyEnemy.Transform.position));
                    }
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(grenade);
            Destroy(explosionParticle.gameObject);
        }
    }
}
