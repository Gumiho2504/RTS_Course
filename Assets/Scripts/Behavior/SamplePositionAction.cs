using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
namespace Gumiho_Rts.Behavior
{


    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Sample Position", story: "Set [TargetLocation] to the closet point on the NavMesh to [Target]", category: "Action/Navigation", id: "76963cc9a453e55e1b5ae0c72da2d2ab")]
    public partial class SamplePositionAction : Action
    {
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<float> Radius = new(7f);

        protected override Status OnStart()
        {
            if (Target.Value == null || !Target.Value.TryGetComponent(out NavMeshAgent agent)) return Status.Failure;

            NavMeshQueryFilter navMeshQueryFilter = new();
            navMeshQueryFilter.agentTypeID = agent.agentTypeID;
            navMeshQueryFilter.areaMask = agent.areaMask;

            if (NavMesh.SamplePosition(Target.Value.transform.position, out NavMeshHit hit, Radius.Value, navMeshQueryFilter))
            {
                TargetLocation.Value = hit.position;
                return Status.Success;
            }
            return Status.Failure;

        }


    }


}