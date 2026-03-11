using Gumiho_Rts.Environment;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Units;

namespace Gumiho_Rts.Events
{
    public struct SupplyEvent : IEvents
    {
        public int Amount { get; private set; }
        public SupplySO Supply { get; private set; }
        public SupplyEvent(int amount, SupplySO supply) { Amount = amount; Supply = supply; }
    }
}

