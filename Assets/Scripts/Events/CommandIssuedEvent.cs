using Gumiho_Rts.Commands;
using Gumiho_Rts.EventBus;

namespace Gumiho_Rts.Events
{
    public struct CommandIssuedEvent : IEvents
    {
        public BaseCommand Command { get; private set; }

        public CommandIssuedEvent(BaseCommand command)
        {
            Command = command;
        }
    }
}