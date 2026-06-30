using UnityEngine;

namespace Gumiho_Rts.Units
{
    public interface IDamageable
    {
        public int MaxHealth { get; }
        public int CurrentHealth { get; }
        public Transform Transform { get; }
        public void TakeDamage(int damage);
        public void Die();
    }
}