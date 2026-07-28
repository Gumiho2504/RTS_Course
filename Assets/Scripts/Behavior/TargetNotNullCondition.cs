using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Target Not Null", story: "[Taget] Not null", category: "Conditions", id: "c899fb816b4cce5854ea9d56ef9dc08e")]
public partial class TargetNotNullCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Taget;

    public override bool IsTrue()
    {
        return Taget.Value != null || Taget != null;
    }


}
