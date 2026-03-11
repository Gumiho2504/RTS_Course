using Gumiho_Rts.Environment;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Gumiho_Rts.Utilities;
namespace Gumiho_Rts.Behavoir
{


    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Gather Supplies", story: "[Unit] gathers [Amount] from [GatherableSupply]", category: "Action/Units", id: "dc62ea0f9a6429deb8d6db69ed5f6e00")]
    public partial class GatherSuppliesAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Unit;
        [SerializeReference] public BlackboardVariable<int> Amount;
        [SerializeReference] public BlackboardVariable<GatherableSupply> GatherableSupply;
        [SerializeReference] public BlackboardVariable<SupplySO> SupplySO;

        private float enterTime;
        private Animator animator;

        protected override Status OnStart()
        {
            if (GatherableSupply.Value == null) return Status.Failure;
            enterTime = Time.time;
            if (Unit.Value.TryGetComponent<Animator>(out animator))
            {
                animator.SetBool(AnimationConstants.IS_GATHERING, true);
            }

            GatherableSupply.Value.BeginGather();
            SupplySO.Value = GatherableSupply.Value.Supply;
            //Debug.Log($"Start Success - ${GatherableSupply.Value.IsBusy}- ${Time.time.ToString()}");
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (GatherableSupply.Value.Supply.BaseGatherTime + enterTime <= Time.time)
            {
                //Debug.Log($"End Success - ${GatherableSupply.Value.IsBusy}- ${Time.time.ToString()}");
                return Status.Success;
            }

            return Status.Running;

        }

        protected override void OnEnd()
        {
            if (animator != null) animator.SetBool(AnimationConstants.IS_GATHERING, false);
            if (GatherableSupply.Value == null) return;
            if (CurrentStatus == Status.Success)
            {
                Amount.Value = GatherableSupply.Value.EndGather();
            }
            else
            {
                GatherableSupply.Value.AbortGather();
            }
        }
    }


}