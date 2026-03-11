using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using Gumiho_Rts.Units;
namespace Gumiho_Rts.Behavoir
{


    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Find Closest Command Post", story: "[Unit] finds nearest [CommandPost]", category: "Action/Units", id: "cef7d8472034049889e3c656ffad80af")]
    public partial class FindClosestCommandPostAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Unit;
        [SerializeReference] public BlackboardVariable<GameObject> CommandPost;
        [SerializeReference] public BlackboardVariable<float> SearchRadius = new(10);
        [SerializeReference] public BlackboardVariable<UnitSO> CommandPostBuilding;

        protected override Status OnStart()
        {
            Collider[] colliders = Physics.OverlapSphere(Unit.Value.transform.position, SearchRadius, LayerMask.GetMask("Buildings"));
            List<BaseBuilding> nearbyCommandPost = new();

            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent(out BaseBuilding building) && building.UnitSO.Equals(CommandPostBuilding.Value))
                {
                    nearbyCommandPost.Add(building);
                }

            }
            if (nearbyCommandPost.Count == 0)
            {
                return Status.Failure;
            }
            CommandPost.Value = nearbyCommandPost[0].gameObject;
          //  Debug.Log("Found Command Post " + CommandPost.Value.name);
            return Status.Success;
        }


    }


}