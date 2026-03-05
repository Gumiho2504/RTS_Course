using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
namespace Gumiho_Rts.Behavoir
{

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Agent Move To TargetLocation", story: "[Agent] move to [TargetLocation]", category: "Action", id: "e1454718747986f26c81e4968595955e")]
    public partial class AgentMoveToTargetLocationAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
        private NavMeshAgent agent;

        protected override Status OnStart()
        {
            if (!Agent.Value.TryGetComponent<NavMeshAgent>(out agent))
            {
                return Status.Failure;
            }
            if (Vector3.Distance(agent.transform.position, TargetLocation.Value) <= agent.stoppingDistance)
            {
                return Status.Success;
            }
            agent.SetDestination(TargetLocation.Value);
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                return Status.Success;
            }
            return Status.Success;
        }

        protected override void OnEnd()
        {
        }
    }


}