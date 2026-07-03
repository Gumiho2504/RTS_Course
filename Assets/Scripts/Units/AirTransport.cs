
using System;
using System.Collections.Generic;
using System.Linq;
using Gumiho_Rts.Behavoir;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace Gumiho_Rts.Units
{
    public class AirTransport : AbstractUnit, ITransporter
    {
        public int Capacity => unitSO.TransportConfig.Capacity;
        [field: SerializeField] public int UsedCapacity { get; private set; }

        private List<ITransportable> loadedUnits = new(8);

        public List<ITransportable> GetLoadedUnits() => loadedUnits.ToList();


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
            NavMeshQueryFilter queryFilter = new()
            {
                areaMask = unit.Agent.areaMask,
                agentTypeID = unit.Agent.agentTypeID
            };

            if (Physics.Raycast(transform.position,
                    Vector3.down,
                    out RaycastHit raycastHit,
                    float.MaxValue,
                    unitSO.TransportConfig.SafeDropLayer)
                && NavMesh.SamplePosition(raycastHit.point, out NavMeshHit hit, 1f, queryFilter))
            {
                UsedCapacity -= unit.TransportCapacityUsage;
                unit.Transform.SetParent(null);
                unit.Transform.gameObject.SetActive(true);

                unit.Agent.Warp(hit.position); //urgent movement (teleport)

                if (unit is IMoveable moveable)
                {
                    moveable.Move(hit.position);
                }

                loadedUnits.Remove(unit);
                Bus<UnitUnloadEvent>.Raise(new UnitUnloadEvent(unit,this));
                return true;

            }
            return false;
        }

        public bool UnloadAll()
        {
            for (int i = loadedUnits.Count - 1; i >= 0; i--)
            {
                Unload(loadedUnits[i]);
            }
            return true;
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

            loadedUnits.Add(transportable);
            Bus<UnitLoadEvent>.Raise(new UnitLoadEvent(transportable,this));

            if (behaviorGraphAgent.GetVariable("LoadUnitTargets", out BlackboardVariable<List<GameObject>> loadUnitTargetsVariable))
            {
                loadUnitTargetsVariable.Value.Remove(targetGameObject);
                behaviorGraphAgent.SetVariableValue("LoadUnitTargets", loadUnitTargetsVariable.Value);
            }

            if (UsedCapacity >= Capacity)
            {
                behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Stop);
                behaviorGraphAgent.SetVariableValue("LoadUnitTargets", new List<GameObject>(unitSO.TransportConfig.Capacity));
            }
        }
    }
}
