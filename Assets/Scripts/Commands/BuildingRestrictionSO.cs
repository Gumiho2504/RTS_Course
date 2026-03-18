using UnityEngine;
using UnityEngine.AI;

namespace Gumiho_Rts.Commands
{
    public class BuildingRestrictionSO : ScriptableObject
    {
        [field:SerializeField] public bool MustBeFullOnNavMesh {get;private set;} = true;
        [field:SerializeField ]public int NavMeshAgentTypeId {get;private set;}
        [field:SerializeField] public float NavMeshTolerance {get;private set;} = 0.1f;
        [field:SerializeField] public Vector3 Extents {get;private set;} = Vector3.one;
        public bool CanPlace(Vector3 position)
        {
            bool isOnNavMesh = true;
            if (MustBeFullOnNavMesh)
            {
                NavMeshQueryFilter navMeshQueryFilter = new();
                navMeshQueryFilter.areaMask = NavMesh.AllAreas;
                navMeshQueryFilter.agentTypeID = NavMeshAgentTypeId;
                isOnNavMesh = IsFullyOnNavMesh(position, navMeshQueryFilter);
                return isOnNavMesh;
            }
            return true;
        }

        private bool IsFullyOnNavMesh(Vector3 position, NavMeshQueryFilter navMeshQueryFilter)
        {
            bool isOnNavMesh = NavMesh.SamplePosition(position + new Vector3(Extents.x, 0, Extents.z), out NavMeshHit _, NavMeshTolerance, navMeshQueryFilter);
            isOnNavMesh = NavMesh.SamplePosition(position + new Vector3(Extents.x, 0, -Extents.z), out NavMeshHit _, NavMeshTolerance, navMeshQueryFilter);
            isOnNavMesh = NavMesh.SamplePosition(position + new Vector3(-Extents.x, 0, -Extents.z), out NavMeshHit _, NavMeshTolerance, navMeshQueryFilter);
            isOnNavMesh = NavMesh.SamplePosition(position + new Vector3(-Extents.x, 0, Extents.z), out NavMeshHit _, NavMeshTolerance, navMeshQueryFilter);
            return isOnNavMesh;
        }
    }
}