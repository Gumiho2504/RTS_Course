using Gumiho_Rts.Player;
using Gumiho_Rts.TechTree;
using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Build Unit", menuName = " Buildings/Commands/Build Unit")]
    public class BuildUnitCommand : BaseCommand
    {
        [field: SerializeField] public UnitSO Unit { get; private set; }
        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is BaseBuilding building
                && !building.IsQueueFull
                && HasEnoughSupply(context);
        }

        public override void Handle(CommandContext context)
        {
            if (!CanHandle(context) || IsLocked(context))
                return;

            BaseBuilding building = (BaseBuilding)context.Commandable;
            building.BuildUnlockable(Unit);
        }

        public override bool IsLocked(CommandContext context)
        {
            if (Unit == null || Unit.Cost == null || Unit.TechTree == null)
                return true;

            if (context.Commandable is BaseBuilding building && building.IsQueueFull)
                return true;

            return !HasEnoughSupply(context) || !Unit.TechTree.IsUnlocked(context.Owner, Unit);
        }

        private bool HasEnoughSupply(CommandContext context)
        {
            if (Unit == null || Unit.Cost == null)
                return false;

            return Unit.Cost.Minerals <= Supplies.Minerals[context.Owner] && Unit.Cost.Gas <= Supplies.Gas[context.Owner];
        }

    }
}