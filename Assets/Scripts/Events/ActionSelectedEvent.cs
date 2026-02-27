using System;
using Gumiho_Rts.Commands;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Units;

namespace Gumiho_Rts.Events
{
    public struct ActionSelectedEvent : IEvents
    {
        public ActionBase Action { get; private set; }

        public ActionSelectedEvent(ActionBase action)
        {
            Action = action;
        }
    }
}

