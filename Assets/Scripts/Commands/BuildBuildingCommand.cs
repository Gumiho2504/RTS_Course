using System.Collections;
using UnityEngine;
using Gumiho_Rts.Units;
namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Build Building", menuName = "Units/Commands/Build Building")]
    public class BuildBuildingCommand : ActionBase
    {
        [field: SerializeField] public BuildingUnitSO BuildingSO { get; private set; }
        [field: SerializeField] public BuildingRestrictionSO Restriction { get; private set; }
        public override bool CanHandle(CommandContext context)
        {
            if (context.Commandable is not IBuildingBuilder) return false;
            if (context.Hit.collider != null)
            {
                return context.Hit.collider.TryGetComponent(out BaseBuilding building)
                 && BuildingSO == building.BuildingSO
                 && (building.Progress.State == BuildingProgress.BuildingState.Paused || building.Progress.State == BuildingProgress.BuildingState.Destroy);
            }
            Debug.Log($"Hit {context.Hit.collider.gameObject.name}");
            return true;
        }

        public override void Handle(CommandContext context)
        {
            IBuildingBuilder builder = context.Commandable as IBuildingBuilder;
            if (context.Hit.collider != null && context.Hit.collider.TryGetComponent(out BaseBuilding building))
            {
                Debug.Log("Resume Building");
                builder.ResumeBuilding(building);
            }
            else
            {
                Debug.Log("Build Building");
                builder.Build(BuildingSO, context.Hit.point);
            }
        }
    }
}