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

        [field: SerializeField] public ActionBase[] AvailableCommands { get; private set; }
        [field: SerializeField] public int CurrentHealth { get; private set; }
        [field: SerializeField] public int MaxHealth { get; private set; }


        protected virtual void Start()
        {
            CurrentHealth = UnitSO.Health;
            MaxHealth = UnitSO.Health;
        }

        public void Select()
        {
            decalProjector.gameObject.SetActive(true);
            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }

        public void Deselect()
        {
            decalProjector.gameObject.SetActive(false);
            Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
        }
    }
}


