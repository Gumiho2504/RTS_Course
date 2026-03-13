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
    public class Worker : AbstractUnit,IBuildingBuilder
    {
        public bool HasSupplies
        {
            get
            {
                if (behaviorGraphAgent != null && behaviorGraphAgent.GetVariable(SUPPLY_AMOUNT_HELD, out BlackboardVariable<int> supplyAmountVariable))
                {
                    return supplyAmountVariable.Value > 0;
                }
                return false;
            }
        }
        protected override void Start()
        {
            base.Start();
            if (behaviorGraphAgent.GetVariable(GATHER_SUPPLIES_EVENT, out BlackboardVariable<GatherSuppliesEventChannel> eventChannelVariable))
            {
                eventChannelVariable.Value.Event += HandleGatherSupplies;
            }
        }



        public void Gather(GatherableSupply supply)
        {
            behaviorGraphAgent.SetVariableValue(SUPPLY, supply);
            behaviorGraphAgent.SetVariableValue(TARGET_GAME_OBJECT, supply.gameObject);
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Gather);

        }
        public void ReturnSupplies(GameObject commandPost)
        {
            Debug.Log("Worker Return Supplies");
            behaviorGraphAgent.SetVariableValue(COMMAND, commandPost);
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.ReturnSupplies);
        }

        private void HandleGatherSupplies(GameObject self, int amount, SupplySO supply)
        {
            Bus<SupplyEvent>.Raise(new SupplyEvent(amount, supply));
        }

        public void Build(BuildingUnitSO building, Vector3 position)
        {
           var instance = Instantiate(building.Prefab,position,Quaternion.identity);
            if(instance.TryGetComponent(out BaseBuilding baseBuilding))
            {
                baseBuilding.ShowBuildingVisualEffect();
            }
            else
            {
                Debug.LogError($"Missing Building Prefab on BuildingSO name:{building.name}! Can not build!");
                return;
            }
        }
    }
}
