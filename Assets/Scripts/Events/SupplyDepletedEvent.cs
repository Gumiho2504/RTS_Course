using Gumiho_Rts.Environment;
using Gumiho_Rts.EventBus;

namespace Gumiho_Rts.Events
{
    public struct SupplyDepletedEvent :IEvents
    {
        public GatherableSupply Supply {get;private set;}
    
    public SupplyDepletedEvent(GatherableSupply supply)
        {
            Supply = supply ;
        }
    }
}