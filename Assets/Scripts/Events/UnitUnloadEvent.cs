using Gumiho_Rts.EventBus;
using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Events
{
    public struct UnitUnloadEvent : IEvents
    {
        public ITransportable Unit { get; private set; }
        public ITransporter Transporter { get; private set; }

        public UnitUnloadEvent(ITransportable unit, ITransporter transporter)
        {
            Unit = unit;
            Transporter = transporter;
        }
    }
}
