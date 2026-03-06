using Gumiho_Rts.Environment;
using UnityEngine;
using UnityEngine.AI;
namespace Gumiho_Rts.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Worker : AbstractUnit
    {
        public void Gather(GatherableSupply supply)
        {
            behaviorGraphAgent.SetVariableValue(SUPPLY, supply);
            behaviorGraphAgent.SetVariableValue(TARGET_LOCATION, supply.transform.position);
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Gather);

        }
    }
}
