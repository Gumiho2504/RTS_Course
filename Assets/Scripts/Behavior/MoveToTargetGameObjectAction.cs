using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
namespace Gumiho_Rts.Behavoir
{


    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Move to Target GameObject", story: "[Agent] move to [TargetGameObject]", category: "Action/Units", id: "29086b4da406687f92704f83a65f14f7")]
    public partial class MoveToTargetGameObjectAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> TargetGameObject;
        private NavMeshAgent agent;
        protected override Status OnStart()
        {
            if (!Agent.Value.TryGetComponent(out agent))
            {
                return Status.Failure;
            }
            Vector3 targetPosition = GetTargetPosition();
            if (Vector3.Distance(agent.transform.position, targetPosition) <= agent.stoppingDistance)
            {
                return Status.Success;
            }
            agent.SetDestination(targetPosition);
            return Status.Running;
        }



        protected override Status OnUpdate()
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                // Debug.Log($"{agent == null}");
                return Status.Success;
            }
            return Status.Running;
        }

        private Vector3 GetTargetPosition()
        {
            Vector3 targetPosition;
            if (TargetGameObject.Value.TryGetComponent(out Collider collider))
            {
                targetPosition = collider.ClosestPoint(agent.transform.position);
            }
            else
            {
                targetPosition = TargetGameObject.Value.transform.position;
            }

            return targetPosition;
        }
    }


}