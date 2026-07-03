
using System.Collections.Generic;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.Utilities;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;


namespace Gumiho_Rts.Units
{
    [RequireComponent(typeof(NavMeshAgent), typeof(BehaviorGraphAgent))]
    public abstract class AbstractUnit : AbstractCommandable, IMoveable, IAttacker
    {
        public NavMeshAgent Agent{get; private set;}
        protected BehaviorGraphAgent behaviorGraphAgent;
        public float AgentRadius => Agent.radius;
        [field: SerializeField] public ParticleSystem AttackingParticleSystem { get; private set; }

        [SerializeField] private DamageableSensor DamageableSensor;

        protected Unit unitSO;

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

        protected virtual void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();

            unitSO = UnitSO as Unit;

            behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Stop);
            behaviorGraphAgent.SetVariableValue("AttackConfig", unitSO.AttackConfig);


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
                DamageableSensor.SetupFrom(unitSO.AttackConfig);
            }
        }



        public void Move(Vector3 target)
        {
            SetCommandOverride(null);
            //navMeshAgent.SetDestination(target);
            behaviorGraphAgent.SetVariableValue(TARGET_LOCATION, target);
            behaviorGraphAgent.SetVariableValue<GameObject>(TARGET_GAME_OBJECT, null);
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Move);
        }

        public void Move(Transform transform)
        {

            behaviorGraphAgent.SetVariableValue(TARGET_GAME_OBJECT, transform.gameObject);
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Move);
        }

        public void Stop()
        {
            SetCommandOverride(null);
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Stop);
        }


        public void HandleUnitEnter(IDamageable damageable)
        {
            List<GameObject> nearbyEnemies = SetNearbyEnemyOnBlackboard();


            if (behaviorGraphAgent.GetVariable("TargetGameObject", out BlackboardVariable<GameObject> targetGameObject)
                && targetGameObject.Value == null
                && nearbyEnemies.Count > 0)
            {
                behaviorGraphAgent.SetVariableValue("TargetGameObject", nearbyEnemies[0]);

            }
        }

        public void HandleUnitExit(IDamageable damageable)
        {
            List<GameObject> nearbyEnemies = SetNearbyEnemyOnBlackboard();
            if (!behaviorGraphAgent.GetVariable("TargetGameObject", out BlackboardVariable<GameObject> targetGameObject) || damageable.Transform.gameObject != targetGameObject.Value) return;
            if (nearbyEnemies.Count > 0)
            {
                behaviorGraphAgent.SetVariableValue("TargetGameObject", nearbyEnemies[0]);
            }
            else
            {
                behaviorGraphAgent.SetVariableValue<GameObject>("TargetGameObject", null);
                behaviorGraphAgent.SetVariableValue("TargetLocation", damageable.Transform.position);
                //Debug.Log($"<color=blue> Set TargetLocation: {damageable.Transform.position}</color>");
            }
        }

        private List<GameObject> SetNearbyEnemyOnBlackboard()
        {
            List<GameObject> nearbyEnemies = DamageableSensor.Damageables.ConvertAll(damageable => damageable.Transform.gameObject);
            nearbyEnemies.Sort(new ClosestGameObjectCompare(transform.position));

            behaviorGraphAgent.SetVariableValue("NearbyEnemies", nearbyEnemies);
            return nearbyEnemies;
        }

        public void Attack(IDamageable damageable)
        {
            //Debug.Log($"{name} should attack {damageable.Transform.name}");
            behaviorGraphAgent.SetVariableValue("TargetGameObject", damageable.Transform.gameObject);
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Attack);
        }
        public void Attack(Vector3 location)
        {
            behaviorGraphAgent.SetVariableValue<GameObject>("TargetGameObject", null);
            behaviorGraphAgent.SetVariableValue(COMMAND, UnitCommand.Attack);
            behaviorGraphAgent.SetVariableValue(TARGET_LOCATION, location);
        }


        protected virtual void OnDestroy()
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

