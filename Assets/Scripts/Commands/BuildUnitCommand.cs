using Gumiho_Rts.Player;
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
            return context.Commandable is BaseBuilding && HasEnoughSupply();
        }
        public override void Handle(CommandContext context)
        {
            if (!HasEnoughSupply()) return;
            // Debug.Log("Starting  Building .... Unity work in progress");
            BaseBuilding building = (BaseBuilding)context.Commandable;
            building.BuildUnit(Unit);
            //Debug.Log("Finished  Building .... Unity work in progress");
        }

        public override bool IsLocked(CommandContext context) => !HasEnoughSupply();

        private bool HasEnoughSupply() => Unit.Cost.Minerals <= Supplies.Minerals && Unit.Cost.Gas <= Supplies.Gas;

    }
}