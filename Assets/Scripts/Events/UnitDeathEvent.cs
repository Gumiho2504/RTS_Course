using Gumiho_Rts.EventBus;
using Gumiho_Rts.Units;

namespace Gumiho_Rts.Events
{
    public struct UnitDeathEvent : IEvents
    {
        public AbstractUnit Unit { get; private set; }
        public UnitDeathEvent(AbstractUnit unit)
        {
            this.Unit = unit;
        }
    }
}

