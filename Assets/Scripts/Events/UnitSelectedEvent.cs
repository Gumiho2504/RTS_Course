using Gumiho_Rts.Units;
using Gumiho_Rts.EventBus;

namespace Gumiho_Rts.Events
{
    public struct UnitSelectedEvent  : IEvents
    {
        public ISelectable Unit { get; private set; }
        public UnitSelectedEvent(ISelectable unit)
        {
            Unit = unit;
        }
    }
}
