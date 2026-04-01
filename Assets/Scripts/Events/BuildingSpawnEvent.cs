using Gumiho_Rts.EventBus;
using Gumiho_Rts.Units;
using UnityEngine;
namespace Gumiho_Rts.Events
{
    public class BuildingSpawnEvent : IEvents
    {
        public BaseBuilding Unit { get; private set; }
        public BuildingSpawnEvent(BaseBuilding unit)
        {
            Unit = unit;
        }
    }
}