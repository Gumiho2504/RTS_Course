
using System;
using System.Collections.Generic;
using Gumiho_Rts.Units;
using TMPro;
using UnityEngine;

namespace Gumiho_Rts.UI.Components
{
    public class UnitTransportUI : MonoBehaviour, IUIElement<ITransporter>
    {
        [SerializeField] private UIUnitButton[] loadedUnitButton;
        [SerializeField] private TextMeshProUGUI capacityText;

        private ITransporter transporter;
        private const string CAPACITY_TEXT = "{0} / {1}";
        public void Disable()
        {
            gameObject.SetActive(false);
            foreach (var unitUI in loadedUnitButton)
            {
                unitUI.Disable();
            }
        }

        public void EnableFor(ITransporter item)
        {
            gameObject.SetActive(true);
            transporter = item;

            capacityText.SetText(string.Format(CAPACITY_TEXT, transporter.Capacity, transporter.UsedCapacity));

            List<ITransportable> loadedUnits = item.GetLoadedUnits();
            for (int i = 0; i < loadedUnits.Count; i++)
            {
                int index = i;

                ITransportable unit = loadedUnits[index];
                UIUnitButton unitButton = loadedUnitButton[index];

                unitButton.EnableFor(unit, () => HandleClick(unit, index));


            }
        }

        private void HandleClick(ITransportable unit, int index)
        {
           if(transporter.Unload(unit))
            {
                loadedUnitButton[index].Disable();
            }
        }
    }
}