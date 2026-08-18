using Gumiho_Rts.EventBus;
using Gumiho_Rts.Player;

namespace Gumiho_Rts.Events
{
    public class PlaceholderSpawnEvent : IEvents
    {
        public Placeholder Placeholder { get; private set; }
        public PlaceholderSpawnEvent(Placeholder placeholder)
        {
            Placeholder = placeholder;
        }
    }
}