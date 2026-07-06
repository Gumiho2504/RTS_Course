using Gumiho_Rts.Commands;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Gumiho_Rts.Units
{
    public abstract class AbstractCommandable : MonoBehaviour, ISelectable, IDamageable
    {
        [SerializeField] protected DecalProjector decalProjector;
        [field: SerializeField] public bool IsSelected { get; protected set; }
        [field: SerializeField] public UnitSO UnitSO { get; private set; }
        [field:SerializeField] public Owner Owner {get;  set;}
        public Transform Transform => transform;


        [field: SerializeField] public BaseCommand[] AvailableCommands { get; private set; }
        [field: SerializeField] public int CurrentHealth { get; protected set; }
        [field: SerializeField] public int MaxHealth { get; protected set; }
        [field: SerializeField] private BaseCommand[] initialCommands;

        public delegate void HeathUpdatedEvent(AbstractCommandable commandable, int lastHealth, int newHealth);
        public event HeathUpdatedEvent OnHealthUpdated;

        protected virtual void Start()
        {
            initialCommands = AvailableCommands;
        }

        public virtual void Select()
        {
            // if (!this.enabled) return;
            if (decalProjector != null)
                decalProjector.gameObject.SetActive(true);
            IsSelected = true;
            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));

        }

        public virtual void Deselect()
        {
            if (decalProjector != null)
                decalProjector.gameObject.SetActive(false);
            IsSelected = false;
            SetCommandOverride();
            Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
        }
        public void SetCommandOverride(BaseCommand[] command = null)
        {

            if (command == null || command.Length == 0)
            {
                AvailableCommands = initialCommands;
            }
            else
            {
                AvailableCommands = command;
            }
            if (IsSelected)
                Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }

        public void TakeDamage(int damage)
        {
            int lastHealth = CurrentHealth;
            
            CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, CurrentHealth);

            OnHealthUpdated?.Invoke(this, lastHealth, CurrentHealth);
            if (CurrentHealth == 0)
            {
                Die();
            }
        }
        public void Die()
        {
            Destroy(gameObject);
        }
        public void Heal(int amount)
        {
            int lastHealth = CurrentHealth;
            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
            OnHealthUpdated?.Invoke(this, lastHealth, CurrentHealth);
        }
    }


}


