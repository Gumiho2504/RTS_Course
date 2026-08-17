using Gumiho_Rts.Environment;
using Gumiho_Rts.EventBus;

namespace Gumiho_Rts.Events
{
    public struct SupplySpawnEvent : IEvents
    {
        public GatherableSupply Supply {get;private set;}
    
    public SupplySpawnEvent(GatherableSupply supply)
        {
            Supply = supply ;
        }
    }
}