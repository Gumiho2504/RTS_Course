using System;
using Unity.Behavior;
using UnityEngine;
namespace Gumiho_Rts.Behavoir
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "Is Target Null", story: "[Target] is null.", category: "Conditions", id: "1d1ce151a83d5f2c5e8adc5047a98fb8")]
    public partial class IsGameObjectNullCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<GameObject> Target;

        public override bool IsTrue()
        {
            return Target == null || Target.Value == null;
        }

    }

}
