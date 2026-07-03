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
    [NodeDescription(name: "Move to Target GameObject", story: "[Agent] move to [TargetGameObject]", category: "Action/Units", id: "29086b4da406687f92704f83a65f14f7")]
    public partial class MoveToTargetGameObjectAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> TargetGameObject;
        [SerializeReference] public BlackboardVariable<float> MoveThreshold = new(0.25f);
        private Animator animator;
        private NavMeshAgent agent;
        private Vector3 lastPosition;

        protected override Status OnStart()
        {


            if (!Agent.Value.TryGetComponent(out agent) || TargetGameObject.Value == null)
            {
                // Debug.Log($"Move To Target GameObject Failed {agent.gameObject.name} {TargetGameObject.Value}");
                return Status.Failure;
            }
            //Debug.Log("Already there On Start");
            agent.TryGetComponent<Animator>(out animator);


            Vector3 targetPosition = GetTargetPosition();
            if (Vector3.Distance(agent.transform.position, targetPosition) <= agent.stoppingDistance)
            {
                //Debug.Log("Already there On Start");
                return Status.Success;
            }

            agent.SetDestination(targetPosition);
            ///Debug.Log("Moving to " + targetPosition.ToString());
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (animator != null) animator.SetFloat(AnimationConstants.SPEED, agent.velocity.magnitude);

            Vector3 targetPosition = GetTargetPosition();
            if (Vector3.Distance(targetPosition, lastPosition) >= MoveThreshold.Value)
            {
                agent.SetDestination(targetPosition);
                lastPosition = agent.destination;
            }

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                //Debug.Log("Already there On Update Success");
                //Debug.Log($"Already there On Update - ${Time.time} - agent.remainingDistance  {agent.remainingDistance} -distance :  {Vector3.Distance(agent.transform.position, targetPosition)}");
                return Status.Success;
            }
            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (animator != null) animator.SetFloat(AnimationConstants.SPEED, 0);
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