using Gumiho_Rts.Environment;
using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

namespace Gumiho_Rts.Behavoir
{
   #if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/GatherSuppliesEventChannel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "GatherSuppliesEventChannel", message: "[Seft] gathers [Amount] [Supplies]", category: "Events", id: "329bfffa27d3d84fb158a542db7228b4")]
public sealed partial class GatherSuppliesEventChannel : EventChannel<GameObject, int, SupplySO> { }


}