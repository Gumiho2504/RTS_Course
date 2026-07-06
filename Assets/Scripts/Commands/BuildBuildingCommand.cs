using UnityEngine;
using Gumiho_Rts.Units;
using Gumiho_Rts.Player;
using UnityEngine.InputSystem.LowLevel;
namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Build Building", menuName = "Units/Commands/Build Building")]
    public class BuildBuildingCommand : BaseCommand
    {
        [field: SerializeField] public BuildingUnitSO BuildingSO { get; private set; }

        public override bool CanHandle(CommandContext context)
        {
            if (context.Commandable is not IBuildingBuilder builder || builder.IsBuilding) return false;
            if (context.Hit.collider != null && context.MouseButton == MouseButton.Right)
            {
                return context.Hit.collider.TryGetComponent(out BaseBuilding building) && BuildingSO == building.BuildingSO
                 && (building.Progress.State == BuildingProgress.BuildingState.Paused || building.Progress.State == BuildingProgress.BuildingState.Destroy);
            }
            return AllRestrictionsPass(context.Hit.point) && HasEnoughSupply(context);
        }

        public override void Handle(CommandContext context)
        {
            IBuildingBuilder builder = context.Commandable as IBuildingBuilder;
            if (context.Hit.collider != null && context.Hit.collider.TryGetComponent(out BaseBuilding building))
            {
                Debug.Log("Resume Building");
                builder.ResumeBuilding(building);
            }
            else if (AllRestrictionsPass(context.Hit.point) && HasEnoughSupply(context))
            {
                Debug.Log("Build Building");
                builder.Build(BuildingSO, context.Hit.point);
            }
        }

        public override bool IsLocked(CommandContext context) => !HasEnoughSupply(context);

        private bool HasEnoughSupply(CommandContext context)
        {
            return BuildingSO.Cost.Minerals <= Supplies.Minerals[context.Owner] && BuildingSO.Cost.Gas <= Supplies.Gas[context.Owner];
        } 
    }
}