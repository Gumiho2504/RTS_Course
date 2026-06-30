using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Gumiho_Rts.Units
{
    [RequireComponent(typeof(Collider))]
    public class DamageableSensor : MonoBehaviour
    {
        private HashSet<IDamageable> damageables = new();
        public List<IDamageable> Damageables => damageables.ToList();

        public delegate void UnitDetectionEvent(IDamageable damageable);
        public event UnitDetectionEvent OnUnitEnter;
        public event UnitDetectionEvent OnUnitExit;
        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageables.Add(damageable);
                OnUnitEnter.Invoke(damageable);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageables.Remove(damageable);
                OnUnitExit.Invoke(damageable);
            }
        }
    }
}

