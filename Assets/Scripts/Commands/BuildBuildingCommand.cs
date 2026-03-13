using System.Collections;
using UnityEngine;
using Gumiho_Rts.Units;
namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName ="Build Building",menuName = "Units/Commands/Build Building")]
    public class BuildBuildingCommand : ActionBase
    {
        [field: SerializeField] public BuildingUnitSO BuildingSO { get; private set; }
        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is IBuildingBuilder;
        }

        public override void Handle(CommandContext context)
        {
           IBuildingBuilder builder = context.Commandable as IBuildingBuilder;
            builder.Build(BuildingSO, context.Hit.point);
        }
    }
}