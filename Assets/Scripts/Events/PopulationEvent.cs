using Gumiho_Rts.EventBus;
using Gumiho_Rts.Units;

namespace Gumiho_Rts.Events
{
    public struct PopulationEvent : IEvents
    {
        public Owner Owner { get; private set; }
        public int PopulationChange { get; private set; }
        public int PopulationLimitChange { get; private set; }
        /// <summary>
        /// Event to update the population of a player
        /// </summary>
        /// <param name="owner">The owner of the population</param>
        /// <param name="populationChange">The change in population</param>
        /// <param name="populationLimitChange">The change in population limit</param>
        public PopulationEvent(Owner owner, int populationChange, int populationLimitChange)
        {
            Owner = owner;
            PopulationChange = populationChange;
            PopulationLimitChange = populationLimitChange;
        }
    }
}