using System.Collections.Generic;
using System.Linq;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using UnityEngine;

namespace Gumiho_Rts.Units
{
    [RequireComponent(typeof(SphereCollider))]
    public class DamageableSensor : MonoBehaviour
    {
        private HashSet<IDamageable> damageables = new();
        public List<IDamageable> Damageables
        {
            get
            {
                PruneDestroyed();
                return damageables.ToList();
            }
        }
        public Owner Owner { get; set; }

        public delegate void UnitDetectionEvent(IDamageable damageable);
        public event UnitDetectionEvent OnUnitEnter;
        public event UnitDetectionEvent OnUnitExit;

        private new SphereCollider collider;

        private void Awake()
        {
            collider = GetComponent<SphereCollider>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other != null && other.TryGetComponent<IDamageable>(out IDamageable damageable) && damageable.Owner != Owner)
            {
                if (damageables.Add(damageable))
                {
                    if (damageables.Count == 1)
                    {
                        Bus<UnitDeathEvent>.RegisterForAll(HandleUnitDeath);
                    }
                    OnUnitEnter?.Invoke(damageable);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other != null && other.TryGetComponent<IDamageable>(out IDamageable damageable) && damageables.Remove(damageable))
            {
                OnUnitExit?.Invoke(damageable);
                if (damageables.Count == 0)
                {
                    Bus<UnitDeathEvent>.UnregisterForAll(HandleUnitDeath);
                }
            }
        }

        private void HandleUnitDeath(UnitDeathEvent args)
        {
            if (args.Unit == null)
                return;

            if (damageables.Remove(args.Unit))
            {
                OnUnitExit?.Invoke(args.Unit);
                if (damageables.Count == 0)
                {
                    Bus<UnitDeathEvent>.UnregisterForAll(HandleUnitDeath);
                }
            }
        }

        private void PruneDestroyed()
        {
            damageables.RemoveWhere(IsDestroyed);
        }

        private static bool IsDestroyed(IDamageable damageable)
        {
            return damageable is not Object unityObject || unityObject == null;
        }

        public void SetupFrom(AttackConfigSO attackConfig)
        {
            collider.radius = attackConfig.AttackRange;
        }

        void OnDestroy()
        {
            Bus<UnitDeathEvent>.UnregisterForAll(HandleUnitDeath);
        }
    }
}
