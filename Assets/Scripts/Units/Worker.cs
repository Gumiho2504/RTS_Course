using System;
using Gumiho_Rts.Behavoir;
using Gumiho_Rts.Environment;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
namespace Gumiho_Rts.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Worker : AbstractUnit
    {
        protected override void Start()
        {
            base.Start();
            if (behaviorGraphAgent.GetVariable(GATHER_SUPPLIES_EVENT, out BlackboardVariable<GatherSuppliesEventChannel> eventChannelVariable))
            {
                Debug.Log("Found GatherSuppliesEventChannel");
                eventChannelVariable.Value.Event += HandleGatherSupplies;
            }
        }



        public void Gather(GatherableSupply supply)
        {
            behaviorGraphAgent.SetVariableValue(SUPPLY, supply);
            behaviorGraphAgent.SetVariableValue(TARGET_GAME_OBJECT, supply.gameObject);
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Gather);

        }

        private void HandleGatherSupplies(GameObject self, int amount, SupplySO supply)
        {
            Bus<SupplyEvent>.Raise(new SupplyEvent(amount, supply));
        }
    }
}
