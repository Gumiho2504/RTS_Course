using System.Collections.Generic;
using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Utilities
{
    public struct ClosetCommandPostCompare : IComparer<BaseBuilding>
    {
        private Vector3 targetPosition;

        public ClosetCommandPostCompare(Vector3 position)
        {
            targetPosition = position;
        }
        public int Compare(BaseBuilding x, BaseBuilding y)
        {
            return (x.transform.position - targetPosition).sqrMagnitude.CompareTo((y.transform.position - targetPosition).sqrMagnitude);
        }
    }
}