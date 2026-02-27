using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(menuName = " Building/Command/Build Unit", fileName = "Build Unit")]
    public class BuildUnitCommand : ActionBase
    {
        [field: SerializeField] public UnitSO Unit { get; private set; }
        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is BaseBuilding;
        }

        public override void Handle(CommandContext context)
        {
            Debug.Log("Starting  Building .... Unity work in progress");
            BaseBuilding building = (BaseBuilding)context.Commandable;
            building.BuildUnit(Unit);
            Debug.Log("Finished  Building .... Unity work in progress");
        }
    }
}