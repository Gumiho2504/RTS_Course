using Gumiho_Rts.EventBus;
using Gumiho_Rts.Units;
using UnityEngine;
namespace Gumiho_Rts.Events
{
    public class BuildingSpawnEvent : IEvents
    {
        public BaseBuilding Unit { get; private set; }
        public Owner Owner {get;private set;}
        public BuildingSpawnEvent(Owner owner,BaseBuilding unit)
        {
            Unit = unit;
            Owner = owner;
        }
    }
}