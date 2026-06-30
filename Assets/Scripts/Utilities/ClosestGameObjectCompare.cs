using System.Collections.Generic;
using UnityEngine;

namespace Gumiho_Rts.Utilities
{
    public struct ClosestGameObjectCompare : IComparer<GameObject>
    {
        private Vector3 targetPosition;

        public ClosestGameObjectCompare(Vector3 position)
        {
            targetPosition = position;
        }
        public int Compare(GameObject x, GameObject y)
        {
            return (x.transform.position - targetPosition).sqrMagnitude.CompareTo((y.transform.position - targetPosition).sqrMagnitude);
        }
    }
}