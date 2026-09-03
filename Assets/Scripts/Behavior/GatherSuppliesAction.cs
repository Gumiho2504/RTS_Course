using Gumiho_Rts.Environment;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Gumiho_Rts.Utilities;
using GameDevTV.RTS.Units;
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
        [SerializeReference] public BlackboardVariable<GameObject> HeldSupply;
        [SerializeReference] public BlackboardVariable<ParticleSystem> ParticleSystem;

        private float enterTime;
        private Animator animator;
        //private ParticleSystem particleSystem;

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
            ParticleSystem.Value.gameObject.SetActive(true);
            //Debug.Log($"Start Success - ${GatherableSupply.Value.IsBusy}- ${Time.time.ToString()}");
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            Quaternion lookRotation = Quaternion.LookRotation((GatherableSupply.Value.transform.position - Unit.Value.transform.position).normalized);
            lookRotation = Quaternion.Euler(0,lookRotation.eulerAngles.y,0);
            Unit.Value.transform.rotation = lookRotation;

            if (GatherableSupply.Value.Supply.BaseGatherTime + enterTime <= Time.time)
            {
                //                Debug.Log($"End Success - ${GatherableSupply.Value.IsBusy}- ${Time.time.ToString()}");
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
                ParticleSystem.Value.gameObject.SetActive(false);
                Amount.Value = GatherableSupply.Value.EndGather();
                GameObject heldModel = GameObject.Instantiate(GatherableSupply.Value.HeldPrefab, Unit.Value.transform, false);
                heldModel.transform.localPosition = new Vector3(0, 1.25f, 0.32f);
                HeldSupply.Value = heldModel;

                if(Unit.Value.TryGetComponent(out HoldGunIK holdGunIK))
                {
                    holdGunIK.leftHandIKTarget = heldModel.transform.Find("LeftHandTarget");
                    holdGunIK.rightHandIKTarget = heldModel.transform.Find("RightHandTarget");
                    holdGunIK.leftElbowIKTarget =heldModel.transform.Find("LeftElbowTarget");
                    holdGunIK.rightElbowIKTarget = heldModel.transform.Find("RightElbowTarget");

                    holdGunIK.elbowIKAmount = 1;
                    holdGunIK.handIKAmount = 1;
                }

            }
            else
            {
                GatherableSupply.Value.AbortGather();
            }
        }
    }


}