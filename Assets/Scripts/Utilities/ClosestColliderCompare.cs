using System.Collections.Generic;
using UnityEngine;

namespace Gumiho_Rts.Utilities
{
    public struct ClosetColliderCompare : IComparer<Collider>
    {
        private Vector3 targetPosition;

        public ClosetColliderCompare(Vector3 position)
        {
            targetPosition = position;
        }
        public int Compare(Collider x, Collider y)
        {
            return (x.transform.position - targetPosition).sqrMagnitude.CompareTo((y.transform.position - targetPosition).sqrMagnitude);
        }
    }
}