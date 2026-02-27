using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using UnityEngine;
using UnityEngine.AI;


namespace Gumiho_Rts.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class AbstractUnit : AbstractCommandable, IMoveable
    {
        private NavMeshAgent navMeshAgent;
        public float AgentRadius => navMeshAgent.radius;


        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>(); ;
        }
        protected override void Start()
        {
            base.Start();
            Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
        }



        public void Move(Vector3 target)
        {
            navMeshAgent.SetDestination(target);
        }


    }


}

