using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;
namespace Gumiho_Rts.Behavoir
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Behavior/Event Channels/Load Unit Event Channel")]
#endif
    [Serializable, GeneratePropertyBag]
    [EventChannelDescription(name: "Load Unit Event Channel", message: "[Self] loads [TargetGameObject] into itself.", category: "Events", id: "36401951260b257866559cfb86d4ecfc")]
    public sealed partial class LoadUnitEventChannel : EventChannel<GameObject, GameObject> { }
}


