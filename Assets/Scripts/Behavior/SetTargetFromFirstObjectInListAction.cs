using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
namespace Gumiho_Rts.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Set Target from First Object In List", story: "Set [Target] to the first item in [List] .", category: "Action/Blackboard", id: "f02c13cb805e43d5f448036a1eef0c0a")]
    public partial class SetTargetFromFirstObjectInListAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<List<GameObject>> List;

        protected override Status OnStart()
        {
            if(List.Value == null || List.Value.Count == 0) return Status.Failure;
            
            Target.Value = List.Value[0];
            
            return Status.Success;
        }

      
    }

}

