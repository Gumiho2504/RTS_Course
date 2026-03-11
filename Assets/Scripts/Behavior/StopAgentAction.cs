using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using Gumiho_Rts.Utilities;

namespace Gumiho_Rts.Behavoir
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Stop Agent", story: "[Agent] stop move", category: "Action/Navigation", id: "a49bc1af9bed1fc806ff8d4087ebb009")]
    public partial class StopAgentAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;

        protected override Status OnStart()
        {
            if (Agent.Value.TryGetComponent(out NavMeshAgent agent))
            {
                if (agent.TryGetComponent<Animator>(out Animator animator))
                {
                    animator.SetFloat(AnimationConstants.SPEED, 0);
                }
                agent.ResetPath();
                return Status.Success;
            }
            return Status.Failure;
        }

    }
}

