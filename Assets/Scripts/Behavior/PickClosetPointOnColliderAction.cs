using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
namespace Gumiho_Rts.Behavoir
{


    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Pick Closet Point on Collider", story: "Set [TargetLocation] on closet point to [Target] on [collider]", category: "Action", id: "ad087f4b9b93022ea2cd0c8f0eb7be75")]
    public partial class PickClosetPointOnColliderAction : Action
    {
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<GameObject> Collider;

        protected override Status OnStart()
        {
            if (Target.Value == null || Collider.Value == null || !Collider.Value.TryGetComponent(out Collider collider)) return Status.Failure;
            TargetLocation.Value = collider.ClosestPoint(Target.Value.transform.position);
            return Status.Success;
        }


    }


}