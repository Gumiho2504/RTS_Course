using System;
using System.Collections.Generic;
using Gumiho_Rts.Environment;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.Units;
using TMPro;
using UnityEngine;

namespace Gumiho_Rts.Player
{
    public class Supplies : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mineralsText;
        [SerializeField] private TextMeshProUGUI gasText;
        [SerializeField] private TextMeshProUGUI populationText;
        [SerializeField] private SupplySO mineralSO;
        [SerializeField] private SupplySO gasSO;
        public static Dictionary<Owner, int> Minerals { get; private set; }
        public static Dictionary<Owner, int> Gas { get; private set; }
        public static Dictionary<Owner, int> Population { get; set; }
        public static Dictionary<Owner, int> PopulationLimit { get; private set; }

        private void Awake()
        {
            Minerals = new Dictionary<Owner, int>();
            Gas = new Dictionary<Owner, int>();
            Population = new Dictionary<Owner, int>();
            PopulationLimit = new Dictionary<Owner, int>();

            foreach (Owner owner in Enum.GetValues(typeof(Owner)))
            {
                Minerals.Add(owner, 0);
                Gas.Add(owner, 0);
                Population.Add(owner, 0);
                PopulationLimit.Add(owner, 0);

            }

            Bus<SupplyEvent>.RegisterForAll(HandleSupplyEvent);
            Bus<PopulationEvent>.RegisterForAll(HandlePopulationEvent);
        }

        private void OnDestroy()
        {

            Bus<SupplyEvent>.UnregisterForAll(HandleSupplyEvent);
            Bus<PopulationEvent>.UnregisterForAll(HandlePopulationEvent);
        }
        private void HandleSupplyEvent(SupplyEvent args)
        {
            if (args.Supply.Equals(mineralSO))
            {
                Minerals[args.Owner] += args.Amount;
                if (Owner.Player1 == args.Owner)
                    mineralsText.SetText(Minerals[args.Owner].ToString());
            }
            else if (args.Supply.Equals(gasSO))
            {
                Gas[args.Owner] += args.Amount;
                if (Owner.Player1 == args.Owner)
                    gasText.SetText(Gas[args.Owner].ToString());
            }
        }
        private static readonly string POPULATION_FORMAT = "{0}/{1}";
        private static readonly string ERROR_POPULATION_FORMAT = "<color=red>{0}</color>/{1}";
        private void HandlePopulationEvent(PopulationEvent args)
        {
            Population[args.Owner] += args.PopulationChange;
            PopulationLimit[args.Owner] += args.PopulationLimitChange;
            if (Owner.Player1 == args.Owner)
            {
                int currentPopulation = Population[args.Owner];
                int maxPopulation = PopulationLimit[args.Owner];
                if(currentPopulation <= maxPopulation)
                {
                    populationText.SetText(string.Format(POPULATION_FORMAT, currentPopulation, maxPopulation));
                }
                else
                {
                    populationText.SetText(string.Format(ERROR_POPULATION_FORMAT, currentPopulation.ToString(), maxPopulation.ToString()));
                }
            }
                
        }
    }
}