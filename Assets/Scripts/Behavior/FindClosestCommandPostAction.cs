using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using Gumiho_Rts.Units;
using System.Linq;
using Gumiho_Rts.Utilities;
namespace Gumiho_Rts.Behavoir
{


    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Find Closest Command Post", story: "[Unit] finds nearest [CommandPost]", category: "Action/Units", id: "cef7d8472034049889e3c656ffad80af")]
    public partial class FindClosestCommandPostAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Unit;
        [SerializeReference] public BlackboardVariable<GameObject> CommandPost;
        [SerializeReference] public BlackboardVariable<float> SearchRadius = new(10f);
        [SerializeReference] public BlackboardVariable<BuildingUnitSO> CommandPostBuilding;

        protected override Status OnStart()
        {
            Collider[] colliders = Physics.OverlapSphere(Unit.Value.transform.position, SearchRadius, LayerMask.GetMask("Buildings"));
            List<BaseBuilding> nearbyCommandPost = new();
            Debug.Log("Found Command Post " + colliders.Length);
            foreach (var collider in colliders)
            {

                Debug.Log("Checking Command Post " + (collider.TryGetComponent(out BaseBuilding b)).ToString() + " isCommandPostBuilding " + b.UnitSO.Equals(CommandPostBuilding.Value).ToString() + " is b.unitSo null " + (b.UnitSO == null).ToString() + " is CommandPostBuilding null " + (CommandPostBuilding.Value == null).ToString() );

                if (collider.TryGetComponent(out BaseBuilding building) && building.UnitSO.Equals(CommandPostBuilding.Value) && building.Progress.State == BuildingProgress.BuildingState.Completed)
                {


                    nearbyCommandPost.Add(building);
                    Debug.Log("Found Command Post Add ");
                }

            }
            if (nearbyCommandPost.Count == 0)
            {
                Debug.Log("Found Command Post Fail");
                return Status.Failure;
            }
            nearbyCommandPost.Sort(new ClosetCommandPostCompare(Unit.Value.transform.position));
            CommandPost.Value = nearbyCommandPost[0].gameObject;
            Debug.Log("Found Command Post Success " + CommandPost.Value.name);
            return Status.Success;
        }


    }


}