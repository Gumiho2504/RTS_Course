
namespace Gumiho_Rts.EventBus
{
    public static class Bus<T> where T : IEvents
    {
        public delegate void Event(T args);
        public static Event OnEvent;
        public static void Raise(T evt) => OnEvent?.Invoke(evt);
    }

}