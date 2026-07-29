using Gumiho_Rts.EventBus;
using Gumiho_Rts.Units;
using UnityEngine;
namespace Gumiho_Rts.Events
{
    public class BuildingDeathEvent : IEvents
    {
        public BaseBuilding Unit { get; private set; }
        public Owner Owner {get;private set;}
        public BuildingDeathEvent(Owner owner,BaseBuilding unit)
        {
            Unit = unit;
            Owner = owner;
        }
    }
}