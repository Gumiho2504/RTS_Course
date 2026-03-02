using System;
using System.Collections.Generic;
using System.Linq;
using Gumiho_Rts.Commands;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.UI.Components;
using Gumiho_Rts.Units;
using UnityEngine;
using UnityEngine.Events;

namespace Gumiho_Rts.UI.Containers
{
    public class ActionUI : MonoBehaviour, IUIElement<HashSet<AbstractCommandable>>
    {
        [SerializeField] private UIActionButton[] actionButtons;
        //  private HashSet<AbstractCommandable> selectedUnits = new(12);



        // private void Awake()
        // {
        //     Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
        //     Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
        // }

        public void EnableFor(HashSet<AbstractCommandable> item)
        {
            RefreshButtons(item);
        }

        public void Disable()
        {
            foreach (UIActionButton button in actionButtons)
            {
                button.Disable();
            }
        }

        // private void Start()
        // {
        //     foreach (UIActionButton button in actionButtons)
        //     {
        //         button.Disable();
        //     }
        // }
        // private void OnDestroy()
        // {
        //     Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
        //     Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
        // }

        // private void HandleUnitSelected(UnitSelectedEvent evt)
        // {
        //     if (evt.Unit is AbstractCommandable commandable)
        //     {
        //         selectedUnits.Add(commandable);
        //         RefreshButtons();
        //     }
        // }


        // private void HandleUnitDeselected(UnitDeselectedEvent evt)
        // {
        //     if (evt.Unit is AbstractCommandable commandable)
        //     {
        //         selectedUnits.Remove(commandable);
        //         RefreshButtons();
        //     }
        // }

        private void RefreshButtons(HashSet<AbstractCommandable> selectedUnits)
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
                    actionButtons[i].EnableFor(actionBaseForSlot, HandleClick(actionBaseForSlot));
                }
                else
                {
                    actionButtons[i].Disable();
                }
            }
        }

        private UnityAction HandleClick(ActionBase action)
        {
            return () => Bus<ActionSelectedEvent>.Raise(new ActionSelectedEvent(action));
        }


    }
}