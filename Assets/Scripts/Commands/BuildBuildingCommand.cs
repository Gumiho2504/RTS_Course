using UnityEngine;
using Gumiho_Rts.Units;
using Gumiho_Rts.Player;
using UnityEngine.InputSystem.LowLevel;
using Gumiho_Rts.TechTree;
namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Build Building", menuName = "Units/Commands/Build Building")]
    public class BuildBuildingCommand : BaseCommand
    {
        [SerializeField] private TechTreeSO techTree;
        [field: SerializeField] public BuildingUnitSO BuildingSO { get; private set; }

        public override bool CanHandle(CommandContext context)
        {
           // Debug.Log("Can Handle Build Building ");
            if (context.Commandable is not IBuildingBuilder builder || builder.IsBuilding) return false;

            if (context.Hit.collider != null && context.MouseButton == MouseButton.Right)
            {
              //  Debug.Log("Can Handle Build Building 1 ");
               // Debug.Log($"{context.Hit.collider.TryGetComponent(out BaseBuilding b)}  ");
                return context.Hit.collider.TryGetComponent(out BaseBuilding building) && BuildingSO == building.BuildingSO
                 && (building.Progress.State == BuildingProgress.BuildingState.Paused || building.Progress.State == BuildingProgress.BuildingState.Destroy);
            }

            return AllRestrictionsPass(context.Hit.point) && HasEnoughSupply(context);
        }

        public override void Handle(CommandContext context)
        {
//            Debug.Log("Handle Build Building");
            IBuildingBuilder builder = context.Commandable as IBuildingBuilder;
            if (context.Hit.collider != null && context.Hit.collider.TryGetComponent(out BaseBuilding building))
            {
                builder.ResumeBuilding(building);
            }
            else if (AllRestrictionsPass(context.Hit.point) && HasEnoughSupply(context))
            {
                builder.Build(BuildingSO, context.Hit.point);
            }
        }

        public override bool IsLocked(CommandContext context) => !HasEnoughSupply(context) || !techTree.IsUnlocked(context.Owner,BuildingSO);

        private bool HasEnoughSupply(CommandContext context)
        {
            return BuildingSO.Cost.Minerals <= Supplies.Minerals[context.Owner] && BuildingSO.Cost.Gas <= Supplies.Gas[context.Owner];
        }
    }
}