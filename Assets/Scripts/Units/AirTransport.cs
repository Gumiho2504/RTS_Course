
using System;
using Gumiho_Rts.Behavoir;
using Unity.Behavior;
using UnityEngine;

namespace Gumiho_Rts.Units
{
    public class AirTransport : AbstractUnit
    {
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
        }
    }
}
