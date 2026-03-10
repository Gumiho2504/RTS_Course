using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
namespace Gumiho_Rts.Behavoir
{


    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Set Agent Avoidance", story: "Set [Agent]  avoidance quality to [AvoidanceQuality]", category: "Action/Navigation", id: "b7ea3447348093c3acb8dbb6cfc00919")]
    public partial class SetAgentAvoidanceAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<int> AvoidanceQuality;

        protected override Status OnStart()
        {
            if (!Agent.Value.TryGetComponent(out NavMeshAgent agent) || AvoidanceQuality.Value < 0 || AvoidanceQuality.Value > 4)
            {
                return Status.Failure;
            }
            agent.obstacleAvoidanceType = (ObstacleAvoidanceType)AvoidanceQuality.Value;
            return Status.Success;
        }

    }


}