using System;
using Gumiho_Rts.Environment;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
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
        public static int Minerals { get; private set; }
        public static int Gas { get; private set; }
        public static int Population { get; set; }
        public static int PopulationLimit { get; private set; }

        private void Awake()
        {
            Bus<SupplyEvent>.OnEvent += HandleSupplyEvent;
        }

        private void OnDestroy()
        {
            Bus<SupplyEvent>.OnEvent -= HandleSupplyEvent;
        }
        private void HandleSupplyEvent(SupplyEvent args)
        {
            if (args.Supply.Equals(mineralSO))
            {
                Minerals += args.Amount;
                mineralsText.SetText(Minerals.ToString());
            }
            else if (args.Supply.Equals(gasSO))
            {
                Gas += args.Amount;
                gasText.SetText(Gas.ToString());
            }
        }
    }
}