using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using Gumiho_Rts.Environment;
using System.Linq;
using Gumiho_Rts.Utilities;
namespace Gumiho_Rts.Behavoir

{


    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Move to GatherableSupply", story: "[Agent] move to [Supply] or nearby not busy supply", category: "Action/Navigation", id: "0c05e8523d99f411e8b1e0a90b4a38f3")]
    public partial class MoveToGatherableSupplyAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GatherableSupply> Supply;
        [SerializeReference] public BlackboardVariable<float> SearchRadius = new(7f);

        private NavMeshAgent agent;
        private LayerMask supplyLayerMask;
        private SupplySO supplySO;
        private const string SUPPLY_MASK = "Supplies";
        protected override Status OnStart()
        {
            supplyLayerMask = LayerMask.GetMask(SUPPLY_MASK);

            if (!HasValidInput()) return Status.Failure;

            Vector3 targetPosition = GetTargetPosition();





            agent.SetDestination(targetPosition);
            return Status.Running;



        }

        private bool HasValidInput()
        {
            if (!Agent.Value.TryGetComponent(out agent) || Supply.Value == null && supplySO == null)
            {
                return false;
            }
            if (Supply.Value != null) { supplySO = Supply.Value.Supply; }

            else
            {
                Collider[] colliders = FindNearbyNotBusyCollider();
                if (colliders.Length > 0)
                {
                    Array.Sort(colliders, new ClosetColliderCompare(agent.transform.position));
                    Supply.Value = colliders[0].GetComponent<GatherableSupply>();
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        protected override Status OnUpdate()
        {
            if (agent.remainingDistance >= agent.stoppingDistance)
            {
                return Status.Running;

            }
            if (!Supply.Value.IsBusy && Supply.Value.Amount > 0)
            {
                return Status.Success;
            }
            Collider[] colliders = FindNearbyNotBusyCollider();
            if (colliders.Length > 0)
            {
                Array.Sort(colliders, new ClosetColliderCompare(agent.transform.position));
                Supply.Value = colliders[0].GetComponent<GatherableSupply>();
                agent.SetDestination(GetTargetPosition());
                return Status.Running;

            }
            return Status.Failure;
        }

        private Collider[] FindNearbyNotBusyCollider()
        {
            return Physics.OverlapSphere(agent.transform.position, SearchRadius, supplyLayerMask)
                .Where(collider => collider.TryGetComponent(
                    out GatherableSupply supply)
                     && !supply.IsBusy
                     && supply.Supply.Equals(Supply.Value.Supply))
                     .ToArray();
        }

        private Vector3 GetTargetPosition()
        {
            Vector3 targetPosition;
            if (Supply.Value.TryGetComponent(out Collider collider))
            {
                targetPosition = collider.ClosestPoint(agent.transform.position);
            }
            else
            {
                targetPosition = Supply.Value.transform.position;
            }

            return targetPosition;
        }
    }


}