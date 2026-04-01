using Gumiho_Rts.Units;
using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

namespace Gumiho_Rts.Behavoir
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Behavior/Event Channels/Building Event Channel")]
#endif
    [Serializable, GeneratePropertyBag]
    [EventChannelDescription(name: "Building Event Channel", message: "[Self] [BuildingEventType] on [BaseBuilding] .", category: "Events", id: "119423f0d0d6dfcc29455aa07cc51c17")]
    public sealed partial class BuildingEventChannel : EventChannel<GameObject, BuildingEventType, BaseBuilding> { }
}

