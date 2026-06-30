using Unity.Behavior;
using UnityEngine;

namespace Gumiho_Rts.Units
{
    [BlackboardEnum]
    public enum BuildingEventType
    {
        ArrivedAt,
        Begin,
        Cancel,
        Abort,
        Competed

    }
}