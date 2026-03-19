using System;
using Gumiho_Rts.Behavoir;
using Gumiho_Rts.Commands;
using Gumiho_Rts.Environment;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
namespace Gumiho_Rts.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Worker : AbstractUnit, IBuildingBuilder
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
        [SerializeField] private ActionBase CancelBuildingCommand;
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

        public GameObject Build(BuildingUnitSO building, Vector3 position)
        {
            var instance = Instantiate(building.Prefab, position, Quaternion.identity);
            if (!instance.TryGetComponent(out BaseBuilding _))
            {
                Debug.LogError($"Missing Building Prefab on BuildingSO name:{building.name}! Can not build!");
                return null;
            }

            behaviorGraphAgent.SetVariableValue(BUILDINGSO, building);
            behaviorGraphAgent.SetVariableValue(TARGET_LOCATION, position);
            behaviorGraphAgent.SetVariableValue(GHOST, instance);
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.BuildBuilding);

            SetCommandOverride(new ActionBase[] { CancelBuildingCommand });
            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
            Bus<SupplyEvent>.Raise(new SupplyEvent(-building.Cost.Minerals, building.Cost.MineralsSO));
            Bus<SupplyEvent>.Raise(new SupplyEvent(-building.Cost.Gas, building.Cost.GasSO));

            return instance;
        }

        public void CancelBuilding()
        {
            if (behaviorGraphAgent.GetVariable(GHOST, out BlackboardVariable<GameObject> ghostVariable) && ghostVariable.Value != null)
            {
                Destroy(ghostVariable.Value);
            }
            if (behaviorGraphAgent.GetVariable(BUILDING_UNDER_CONSTRUCTION, out BlackboardVariable<BaseBuilding> building) && building.Value != null)
            {
                Destroy(building.Value.gameObject);

                BuildingUnitSO buildingUnitSO = building.Value.BuildingSO;
                Bus<SupplyEvent>.Raise(new SupplyEvent(Mathf.FloorToInt(buildingUnitSO.Cost.Minerals * 0.75f), buildingUnitSO.Cost.MineralsSO));
                Bus<SupplyEvent>.Raise(new SupplyEvent(Mathf.FloorToInt(buildingUnitSO.Cost.Gas * 0.75f), buildingUnitSO.Cost.GasSO));
            }
            SetCommandOverride(Array.Empty<ActionBase>());

            Stop();
        }

        public void ResumeBuilding(BaseBuilding building)
        {
            Debug.Log("Resume Building" + building.name);
            behaviorGraphAgent.SetVariableValue(TARGET_LOCATION, building.transform.position);
            behaviorGraphAgent.SetVariableValue(BUILDING_UNDER_CONSTRUCTION, building);
            behaviorGraphAgent.SetVariableValue<GameObject>(GHOST, null);
            behaviorGraphAgent.SetVariableValue(BUILDINGSO, building.BuildingSO);

            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.BuildBuilding);

            SetCommandOverride(new ActionBase[] { CancelBuildingCommand });
            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));

        }
    }
}
