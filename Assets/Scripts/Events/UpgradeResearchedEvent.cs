using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.TechTree;
using Gumiho_Rts.Units;

namespace RTS_Course.Assets.Scripts.Events
{
    public struct UpgradeResearchedEvent : IEvents
    {
        public Owner Owner{get;private set;}
        public UpgradeSO Upgrade {get;private set;}

        public UpgradeResearchedEvent(Owner owner,UpgradeSO upgrade)
        {
            Owner = owner;
            Upgrade = upgrade;
        }
    }
}
