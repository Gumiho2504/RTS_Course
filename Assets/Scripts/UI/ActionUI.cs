using System;
using System.Collections.Generic;
using System.Linq;
using Gumiho_Rts.Commands;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.UI
{
    public class ActionUI : MonoBehaviour
    {
        [SerializeField] private UIActionButton[] actionButtons;
        private HashSet<AbstractCommandable> selectedUnits = new(12);



        private void Awake()
        {
            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
            foreach (UIActionButton button in actionButtons)
            {
                button.SetIcon(null);
            }
        }
        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
        }

        private void HandleUnitSelected(UnitSelectedEvent evt)
        {
            if (evt.Unit is AbstractCommandable commandable)
            {
                selectedUnits.Add(commandable);
                RefreshButtons();
            }
        }


        private void HandleUnitDeselected(UnitDeselectedEvent evt)
        {
            if (evt.Unit is AbstractCommandable commandable)
            {
                selectedUnits.Remove(commandable);
                RefreshButtons();
            }
        }

        private void RefreshButtons()
        {
            HashSet<ActionBase> availableCommands = new(9);
            foreach (AbstractCommandable commandable in selectedUnits)
            {
                availableCommands.UnionWith(commandable.AvailableCommands);
            }
            for (int i = 0; i < actionButtons.Length; i++)
            {
                ActionBase actionBaseForSlot = availableCommands.Where(action => action.Slot == i).FirstOrDefault();
                if (actionBaseForSlot != null)
                {
                    actionButtons[i].SetIcon(actionBaseForSlot.Icon);
                }
                else
                {
                    actionButtons[i].SetIcon(null);
                }
            }
        }

    }
}