using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Commands
{

    [CreateAssetMenu(fileName = "Cancel Building", menuName = "Units/Commands/Cancel Building")]
    public class CancelBuildingCommand : BaseCommand
    {
        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is IBuildingBuilder;
        }

        public override void Handle(CommandContext context)
        {
            IBuildingBuilder building = context.Commandable as IBuildingBuilder;
            building.CancelBuilding();
        }
        public override bool IsLocked(CommandContext context) => false;

    }
}