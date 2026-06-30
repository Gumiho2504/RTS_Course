using UnityEngine;
using UnityEngine.AI;

namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Building Restriction", menuName = "Buildings/Restriction")]
    public class BuildingRestrictionSO : ScriptableObject
    {
        [field:SerializeField] public float Radius {get;private set;} = 1f;
        [field:SerializeField] public LayerMask LayerMask {get;private set;} 
        [field:SerializeField] public OverlapStyle HitDetectionStyle {get;private set;} = OverlapStyle.Sphere;
        [field:SerializeField] public bool MustBeFullOnNavMesh {get;private set;} = true;
        [field:SerializeField ]public int NavMeshAgentTypeId {get;private set;}
        [field:SerializeField] public float NavMeshTolerance {get;private set;} = 0.1f;
        [field:SerializeField] public Vector3 Extents {get;private set;} = Vector3.one;
        private Collider[] hitColliders = new Collider[1];

        public bool CanPlace(Vector3 position)
        {
           
           int hits = HitDetectionStyle switch
           {
               OverlapStyle.Sphere => Physics.OverlapSphereNonAlloc(position, Radius, hitColliders, LayerMask),
               OverlapStyle.Box => Physics.OverlapBoxNonAlloc(position, Extents, hitColliders, Quaternion.identity, LayerMask),
               _ => throw new System.NotImplementedException(),
           };
            if (MustBeFullOnNavMesh)
            {
                NavMeshQueryFilter navMeshQueryFilter = new();
                navMeshQueryFilter.areaMask = NavMesh.AllAreas;
                navMeshQueryFilter.agentTypeID = NavMeshAgentTypeId;
               bool  isOnNavMesh = IsFullyOnNavMesh(position, navMeshQueryFilter);
                return isOnNavMesh && hits == 0;
            }
            return hits == 0 ;
        }

        private bool IsFullyOnNavMesh(Vector3 position, NavMeshQueryFilter navMeshQueryFilter)
        {
            bool isOnNavMesh = NavMesh.SamplePosition(position + new Vector3(Extents.x, 0, Extents.z), out NavMeshHit _, NavMeshTolerance, navMeshQueryFilter);
            isOnNavMesh = NavMesh.SamplePosition(position + new Vector3(Extents.x, 0, -Extents.z), out NavMeshHit _, NavMeshTolerance, navMeshQueryFilter);
            isOnNavMesh = NavMesh.SamplePosition(position + new Vector3(-Extents.x, 0, -Extents.z), out NavMeshHit _, NavMeshTolerance, navMeshQueryFilter);
            isOnNavMesh = NavMesh.SamplePosition(position + new Vector3(-Extents.x, 0, Extents.z), out NavMeshHit _, NavMeshTolerance, navMeshQueryFilter);
            return isOnNavMesh;
        }
        public enum OverlapStyle
        {
            Sphere,Box
        }
    }
}