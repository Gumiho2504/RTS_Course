using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;


namespace Gumiho_Rts.Units
{
    [RequireComponent(typeof(NavMeshAgent), typeof(BehaviorGraphAgent))]
    public abstract class AbstractUnit : AbstractCommandable, IMoveable
    {
        private NavMeshAgent navMeshAgent;
        protected BehaviorGraphAgent behaviorGraphAgent;
        public float AgentRadius => navMeshAgent.radius;


        protected const string TARGET_LOCATION = "TargetLocation";
        protected const string COMMAND = "Command";
        protected const string SUPPLY = "Supply";
        protected const string TARGET_GAME_OBJECT = "TargetGameObject";
        protected const string GATHER_SUPPLIES_EVENT = "GatherSuppliesEventChannel";
        protected const string SUPPLY_AMOUNT_HELD = "SupplyAmountHeld";

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Stop);
        }
        protected override void Start()
        {
            base.Start();
            Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
        }



        public void Move(Vector3 target)
        {
            //navMeshAgent.SetDestination(target);
            behaviorGraphAgent.SetVariableValue(TARGET_LOCATION, target);
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Move);
        }

        public void Stop()
        {
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Stop);
        }
    }


}

