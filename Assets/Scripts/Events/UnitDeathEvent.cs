using Gumiho_Rts.EventBus;
using Gumiho_Rts.Units;

namespace Gumiho_Rts.Events
{
    public struct UnitDeathEvent : IEvents
    {
        public AbstractCommandable Unit { get; private set; }
        public UnitDeathEvent(AbstractCommandable unit)
        {
            this.Unit = unit;
        }
    }
}
