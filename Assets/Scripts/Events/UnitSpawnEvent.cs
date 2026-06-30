using Gumiho_Rts.EventBus;
using Gumiho_Rts.Units;

namespace Gumiho_Rts.Events
{
    public struct UnitSpawnEvent : IEvents
    {
        public AbstractUnit Unit { get; private set; }
        public UnitSpawnEvent(AbstractUnit unit)
        {
            this.Unit = unit;
        }
    }
}

