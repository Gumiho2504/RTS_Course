using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Gumiho_Rts.Units
{
    [RequireComponent(typeof(SphereCollider))]
    public class DamageableSensor : MonoBehaviour
    {
        private HashSet<IDamageable> damageables = new();
        public List<IDamageable> Damageables => damageables.ToList();

        public delegate void UnitDetectionEvent(IDamageable damageable);
        public event UnitDetectionEvent OnUnitEnter;
        public event UnitDetectionEvent OnUnitExit;

        private new SphereCollider collider;

        private void Awake() {
            collider = GetComponent<SphereCollider>();
        }
        void OnTriggerEnter(Collider other)
        {
            if (other != null && other.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageables.Add(damageable);
                OnUnitEnter?.Invoke(damageable);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other != null && other.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageables.Remove(damageable);
                OnUnitExit?.Invoke(damageable);
            }
        }

        public void SetupFrom(AttackConfigSO attackConfig)
        {
            collider.radius = attackConfig.AttackRange;
        }
    }
}

