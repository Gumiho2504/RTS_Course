using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gumiho_Rts.Player;
using Gumiho_Rts.TechTree;
using Gumiho_Rts.Units;
using Unity.VisualScripting;
using UnityEngine;

namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Research Upgrade", menuName = "Tech Tree/Research Upgrade Command", order = 140)]
    public class ResearchUpgradeCommand : BaseCommand
    {
        [field: SerializeField] public UpgradeSO Upgrade { get; private set; }
        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is BaseBuilding;
        }

        public override void Handle(CommandContext context)
        {
            BaseBuilding building = context.Commandable as BaseBuilding;
            if (HasEnoughSupply(context))
            {
                building.BuildUnlockable(Upgrade);
            }
        }

        public override bool IsLocked(CommandContext context) => !HasEnoughSupply(context) || !Upgrade.TechTree.IsUnlocked(context.Owner,Upgrade);

        private bool HasEnoughSupply(CommandContext context)
        {
            return Upgrade.Cost.Minerals <= Supplies.Minerals[context.Owner] && Upgrade.Cost.Gas <= Supplies.Gas[context.Owner];
        }

    }
}