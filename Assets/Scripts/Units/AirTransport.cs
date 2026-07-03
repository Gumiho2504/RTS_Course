
using System;
using System.Collections.Generic;
using Gumiho_Rts.Behavoir;
using Unity.Behavior;
using UnityEngine;

namespace Gumiho_Rts.Units
{
    public class AirTransport : AbstractUnit, ITransporter
    {
        public int Capacity => unitSO.TransportConfig.Capacity;
        [field: SerializeField] public int UsedCapacity { get; private set; }

        public List<ITransportable> GetLoadedUnits()
        {
            throw new NotImplementedException();
        }

        public void Load(ITransportable unit)
        {
            if (UsedCapacity + unit.TransportCapacityUsage > Capacity) return;

            if (behaviorGraphAgent.GetVariable("LoadUnitTargets", out BlackboardVariable<List<GameObject>> loadUnitsTargetsVariable))
            {
                loadUnitsTargetsVariable.Value.Add(unit.Transform.gameObject);
                behaviorGraphAgent.SetVariableValue("LoadUnitTargets", loadUnitsTargetsVariable.Value);
            }


            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.LoadUnits);
        }

        public void Load(ITransportable[] units)
        {
            throw new NotImplementedException();
        }

        public bool Unload(ITransportable unit)
        {
            throw new NotImplementedException();
        }

        public bool UnloadAll()
        {
            throw new NotImplementedException();
        }

        protected override void Start()
        {
            base.Start();
            if (behaviorGraphAgent.GetVariable("LoadUnitEventChannel", out BlackboardVariable<LoadUnitEventChannel> eventChannelVariable)
                && eventChannelVariable != null)
            {
                eventChannelVariable.Value.Event += HandleLoadUnit;
            }
        }

        private void HandleLoadUnit(GameObject self, GameObject targetGameObject)
        {
            Debug.Log($"<color=yellow> Load unit {targetGameObject.name} </color>");

            targetGameObject.SetActive(false);
            targetGameObject.transform.SetParent(self.transform);
            ITransportable transportable = targetGameObject.GetComponent<ITransportable>();
            UsedCapacity += transportable.TransportCapacityUsage;

            if (behaviorGraphAgent.GetVariable("LoadUnitTargets", out BlackboardVariable<List<GameObject>> loadUnitTargetsVariable))
            {
                loadUnitTargetsVariable.Value.Remove(targetGameObject);
                behaviorGraphAgent.SetVariableValue("LoadUnitTargets", loadUnitTargetsVariable.Value);
            }

            if (UsedCapacity >= Capacity)
            {
                behaviorGraphAgent.SetVariableValue(COMMAND,UnitCommand.Stop);
                behaviorGraphAgent.SetVariableValue("LoadUnitTargets",new List<GameObject>(unitSO.TransportConfig.Capacity));
            }
        }
    }
}
