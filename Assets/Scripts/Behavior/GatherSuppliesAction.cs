using Gumiho_Rts.Environment;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
namespace Gumiho_Rts.Behavoir
{


    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Gather Supplies", story: "[Unit] gathers [Amount] from [GatherableSupply]", category: "Action/Units", id: "dc62ea0f9a6429deb8d6db69ed5f6e00")]
    public partial class GatherSuppliesAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Unit;
        [SerializeReference] public BlackboardVariable<int> Amount;
        [SerializeReference] public BlackboardVariable<GatherableSupply> GatherableSupply;

        private float enterTime;

        protected override Status OnStart()
        {
            enterTime = Time.time;
            GatherableSupply.Value.BeginGather();
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (GatherableSupply.Value.Supply.BaseGatherTime + enterTime <= Time.time)
            {
                int gatheredAmount = GatherableSupply.Value.EndGather();
                Amount.Value += gatheredAmount;
                return Status.Success;
            }

            return Status.Running;

        }

        protected override void OnEnd()
        {
        }
    }


}