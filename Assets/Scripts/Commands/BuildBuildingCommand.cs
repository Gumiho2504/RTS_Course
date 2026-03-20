using System.Collections;
using UnityEngine;
using Gumiho_Rts.Units;
using System.Linq;
using Gumiho_Rts.Player;
namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Build Building", menuName = "Units/Commands/Build Building")]
    public class BuildBuildingCommand : BaseCommand
    {
        [field: SerializeField] public BuildingUnitSO BuildingSO { get; private set; }

        public override bool CanHandle(CommandContext context)
        {
            if (context.Commandable is not IBuildingBuilder) return false;
            if (context.Hit.collider != null)
            {
                return context.Hit.collider.TryGetComponent(out BaseBuilding building)
                 && BuildingSO == building.BuildingSO
                 && (building.Progress.State == BuildingProgress.BuildingState.Paused || building.Progress.State == BuildingProgress.BuildingState.Destroy);
            }
            return AllRestrictionsPass(context.Hit.point) && HasEnoughSupply();
        }

        public override void Handle(CommandContext context)
        {
            IBuildingBuilder builder = context.Commandable as IBuildingBuilder;
            if (context.Hit.collider != null && context.Hit.collider.TryGetComponent(out BaseBuilding building))
            {
                Debug.Log("Resume Building");
                builder.ResumeBuilding(building);
            }
            else if (AllRestrictionsPass(context.Hit.point) && HasEnoughSupply())
            {
                Debug.Log("Build Building");
                builder.Build(BuildingSO, context.Hit.point);
            }
        }

        public override bool IsLocked(CommandContext context) => !HasEnoughSupply();

        private bool HasEnoughSupply() => BuildingSO.Cost.Minerals <= Supplies.Minerals && BuildingSO.Cost.Gas <= Supplies.Gas;
    }
}