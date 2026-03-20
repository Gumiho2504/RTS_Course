using System;
using Gumiho_Rts.Commands;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Units;

namespace Gumiho_Rts.Events
{
    public struct CommandSelectedEvent : IEvents
    {
        public BaseCommand Command { get; private set; }

        public CommandSelectedEvent(BaseCommand command)
        {
            Command = command;
        }
    }
}

