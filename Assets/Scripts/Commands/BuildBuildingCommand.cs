using System.Collections;
using UnityEngine;
using Gumiho_Rts.Units;
using System.Linq;
namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Build Building", menuName = "Units/Commands/Build Building")]
    public class BuildBuildingCommand : ActionBase
    {
        [field: SerializeField] public BuildingUnitSO BuildingSO { get; private set; }
        [field: SerializeField] public BuildingRestrictionSO[] Restrictions { get; private set; }
        public override bool CanHandle(CommandContext context)
        {
            if (context.Commandable is not IBuildingBuilder) return false;
            if (context.Hit.collider != null)
            {
                return context.Hit.collider.TryGetComponent(out BaseBuilding building)
                 && BuildingSO == building.BuildingSO
                 && (building.Progress.State == BuildingProgress.BuildingState.Paused || building.Progress.State == BuildingProgress.BuildingState.Destroy);
            }
            return AllRestrictionsPass(context.Hit.point);
        }

        public override void Handle(CommandContext context)
        {
            IBuildingBuilder builder = context.Commandable as IBuildingBuilder;
            if (context.Hit.collider != null && context.Hit.collider.TryGetComponent(out BaseBuilding building))
            {
                Debug.Log("Resume Building");
                builder.ResumeBuilding(building);
            }
            else if (AllRestrictionsPass(context.Hit.point))
            {
                Debug.Log("Build Building");
                builder.Build(BuildingSO, context.Hit.point);
            }
        }
        private bool AllRestrictionsPass(Vector3 point) => Restrictions.Length == 0 || Restrictions.All(restriction => restriction.CanPlace(point));
    }
}