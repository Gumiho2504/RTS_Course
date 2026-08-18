using Gumiho_Rts.EventBus;
using Gumiho_Rts.Player;

namespace Gumiho_Rts.Events
{
    public class PlaceholderDestroyEvent : IEvents
    {
        public Placeholder Placeholder { get; private set; }
        public PlaceholderDestroyEvent(Placeholder placeholder)
        {
            Placeholder = placeholder;
        }
    }
}