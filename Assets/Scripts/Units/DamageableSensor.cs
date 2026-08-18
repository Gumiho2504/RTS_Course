using System.Collections.Generic;
using System.Linq;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.Player;
using UnityEngine;

namespace Gumiho_Rts.Units
{
    [RequireComponent(typeof(SphereCollider))]
    public class DamageableSensor : MonoBehaviour
    {
        private HashSet<IDamageable> visitableDamageables = new();
        public List<IDamageable> Damageables
        {
            get
            {
                PruneDestroyed();
                return visitableDamageables.ToList();
            }
        }
        private HashSet<IDamageable> allDamageables = new();
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
            if (other != null && other.TryGetComponent(out IDamageable damageable) && damageable.Owner != Owner)
            {
                allDamageables.Add(damageable);

                if (collider.TryGetComponent(out IHideable hideable))
                {
                    hideable.OnVisibilityChange += HandleVisibilityChange;
                    if (hideable.IsVisitable)
                    {
                        if (visitableDamageables.Add(damageable))
                        {
                            OnUnitEnter?.Invoke(damageable);
                        }
                    }
                }
                else
                {
                    if (visitableDamageables.Add(damageable))
                    {

                        OnUnitEnter?.Invoke(damageable);
                    }
                }

            }
            if (allDamageables.Count == 1)
            {
                Bus<UnitDeathEvent>.RegisterForAll(HandleUnitDeath);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other != null && other.TryGetComponent(out IDamageable damageable) && visitableDamageables.Remove(damageable)
            && allDamageables.Remove(damageable))
            {
                OnUnitExit?.Invoke(damageable);
                if(collider.TryGetComponent(out IHideable hideable))
                {
                    hideable.OnVisibilityChange -= HandleVisibilityChange;
                }
                if (allDamageables.Count == 1)
                {
                    Bus<UnitDeathEvent>.UnregisterForAll(HandleUnitDeath);
                }
            }
        }
        private void HandleVisibilityChange(IHideable hideable, bool isVisible)
        {
            IDamageable damageable = hideable.Transform.GetComponent<IDamageable>();
            if (damageable != null)
            {
                if (isVisible)
                {
                    visitableDamageables.Add(damageable);
                    OnUnitEnter?.Invoke(damageable);
                }
                else
                {
                    visitableDamageables.Remove(damageable);
                    OnUnitExit?.Invoke(damageable);
                }
            }
        }

        private void HandleUnitDeath(UnitDeathEvent args)
        {
            if (args.Unit == null)
                return;

            else
            {
                visitableDamageables.Remove(args.Unit);
            }

            if (allDamageables.Remove(args.Unit))
            {
                OnUnitExit?.Invoke(args.Unit);
                if (allDamageables.Count == 0)
                {
                    Bus<UnitDeathEvent>.UnregisterForAll(HandleUnitDeath);
                }
            }
        }

        private void PruneDestroyed()
        {
            visitableDamageables.RemoveWhere(IsDestroyed);
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

            foreach (var damageable in allDamageables)
            {
                if (collider.TryGetComponent(out IHideable hideable))
                {
                    hideable.OnVisibilityChange -= HandleVisibilityChange;
                }
            }
            Bus<UnitDeathEvent>.UnregisterForAll(HandleUnitDeath);


        }
    }
}
