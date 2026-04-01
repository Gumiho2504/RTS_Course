using Gumiho_Rts.Commands;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Gumiho_Rts.Units
{
    public abstract class AbstractCommandable : MonoBehaviour, ISelectable
    {
        [SerializeField] private DecalProjector decalProjector;
        [field: SerializeField] public UnitSO UnitSO { get; private set; }

        [field: SerializeField] public BaseCommand[] AvailableCommands { get; private set; }
        [field: SerializeField] public int CurrentHealth { get; protected set; }
        [field: SerializeField] public int MaxHealth { get; protected set; }
        [field: SerializeField] private BaseCommand[] initialCommands;

        public delegate void HeathUpdatedEvent(AbstractCommandable commandable,int lastHealth, int newHealth);
        public event HeathUpdatedEvent OnHealthUpdated;

        protected virtual void Start()
        {
            initialCommands = AvailableCommands;
        }

        public void Select()
        {
            // if (!this.enabled) return;
            decalProjector.gameObject.SetActive(true);
            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }

        public void Deselect()
        {
            decalProjector.gameObject.SetActive(false);
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
            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }
        public void Heal(int amount)
        {
            int lastHealth = CurrentHealth;
            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
            OnHealthUpdated?.Invoke(this, lastHealth, CurrentHealth);
        }
    }


}


