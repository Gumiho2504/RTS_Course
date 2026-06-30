using Gumiho_Rts.Units;
using System;
using Unity.Behavior;
using UnityEngine;

namespace Gumiho_Rts.Behavoir
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "Building is in Progress", story: "[BaseBuilding] is begin build.", category: "Conditions", id: "a85295f5d2ad38cda95ed64fdc8dbff7")]
    public partial class BuildingIsInProgressCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<BaseBuilding> BaseBuilding;

        public override bool IsTrue()
        {
            return BaseBuilding.Value != null && BaseBuilding.Value.Progress.State == BuildingProgress.BuildingState.Building;
        }

       
    }
}
