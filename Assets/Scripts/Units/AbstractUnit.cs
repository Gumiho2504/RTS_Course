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
        private BehaviorGraphAgent behaviorGraphAgent;
        public float AgentRadius => navMeshAgent.radius;


        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
            Move(transform.position);
        }
        protected override void Start()
        {
            base.Start();
            Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
            Move(transform.position);
        }



        public void Move(Vector3 target)
        {
            //navMeshAgent.SetDestination(target);
            behaviorGraphAgent.SetVariableValue("TargetLocation", target);
        }


    }


}

