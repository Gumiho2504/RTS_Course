using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;


namespace Gumiho_Rts.Units
{
    [RequireComponent(typeof(NavMeshAgent), typeof(BehaviorGraphAgent))]
    public abstract class AbstractUnit : AbstractCommandable, IMoveable, IAttacker
    {
        private NavMeshAgent navMeshAgent;
        protected BehaviorGraphAgent behaviorGraphAgent;
        public float AgentRadius => navMeshAgent.radius;

        [SerializeField] private DamageableSensor DamageableSensor;

        protected const string TARGET_LOCATION = "TargetLocation";
        protected const string COMMAND = "Command";
        protected const string SUPPLY = "Supply";
        protected const string TARGET_GAME_OBJECT = "TargetGameObject";
        protected const string GATHER_SUPPLIES_EVENT = "GatherSuppliesEventChannel";
        protected const string BUILDING_EVENT_CHANNEL = "BuildingEventChannel";
        protected const string SUPPLY_AMOUNT_HELD = "SupplyAmountHeld";
        protected const string GHOST = "Ghost";
        protected const string BUILDING_SO = "BuildingSO";
        protected const string BUILDING_UNDER_CONSTRUCTION = "BuildingUnderConstruction";

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Stop);
        }
        protected override void Start()
        {
            base.Start();
            CurrentHealth = UnitSO.Health;
            MaxHealth = UnitSO.Health;
            Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));

            if (DamageableSensor != null)
            {
                DamageableSensor.OnUnitEnter += HandleUnitEnter;
                DamageableSensor.OnUnitExit += HandleUnitExit;
            }
        }



        public void Move(Vector3 target)
        {
            SetCommandOverride(null);
            //navMeshAgent.SetDestination(target);
            behaviorGraphAgent.SetVariableValue(TARGET_LOCATION, target);
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Move);
        }

        public void Stop()
        {
            SetCommandOverride(null);
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Stop);
        }


        public void HandleUnitEnter(IDamageable damageable)
        {
            Debug.Log($"Detected unit enter! {DamageableSensor.Damageables.Count} nearby damageables");
        }

        public void HandleUnitExit(IDamageable damageable)
        {
            Debug.Log($"Detected unit exit! {DamageableSensor.Damageables.Count} nearby damageables");
        }



        public void Attack(IDamageable damageable)
        {
            behaviorGraphAgent.SetVariableValue("TargetGameObject", damageable.Transform.gameObject);
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Attack);
        }
        public void Attack(Vector3 location)
        {

        }


        private void OnDestroy()
        {
            Bus<UnitDeathEvent>.Raise(new UnitDeathEvent(this));

            if (DamageableSensor != null)
            {
                DamageableSensor.OnUnitEnter -= HandleUnitEnter;
                DamageableSensor.OnUnitExit -= HandleUnitExit;
            }
        }



    }


}

