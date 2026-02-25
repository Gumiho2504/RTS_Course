using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.Units;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace Gumiho_Rts.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class AbstractUnit : MonoBehaviour, IMoveable, ISelectable
    {
        private NavMeshAgent navMeshAgent;
        public float AgentRadius => navMeshAgent.radius;
        [SerializeField] private DecalProjector decalProjector;

 
        private void Start()
        {
              Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
        }

        public void Deselect()
        {
            decalProjector.gameObject.SetActive(false);
            Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
        }

        public void Move(Vector3 target)
        {
            navMeshAgent.SetDestination(target);
        }


        public void Select()
        {
            decalProjector.gameObject.SetActive(true);
            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }



        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

    }
}

