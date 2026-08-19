
using Gumiho_Rts.EventBus;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Gumiho_Rts.Events
{
    public struct MinimapClickEvent : IEvents
    {
        public MouseButton Button { get; private set; }
        public RaycastHit Hit { get; private set; }
        public MinimapClickEvent(MouseButton button, RaycastHit hit)
        {
            Button = button;
            Hit = hit;
        }
    }
}