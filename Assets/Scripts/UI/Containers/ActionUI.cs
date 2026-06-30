using System;
using System.Collections.Generic;
using System.Linq;
using Gumiho_Rts.Commands;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.UI.Components;
using Gumiho_Rts.Units;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Gumiho_Rts.UI.Containers
{
    public class ActionUI : MonoBehaviour, IUIElement<HashSet<AbstractCommandable>>
    {
        [SerializeField] private UIActionButton[] actionButtons;
         
      

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

  
        private void RefreshButtons(HashSet<AbstractCommandable> selectedUnits)
        {

            HashSet<BaseCommand> availableCommands = new(9);
            foreach (AbstractCommandable commandable in selectedUnits)
            {
                //  availableCommands.UnionWith(commandable.AvailableCommands);
                if (commandable.AvailableCommands != null)
                {
                    availableCommands.AddRange(commandable.AvailableCommands);
                }
            }
            for (int i = 0; i < actionButtons.Length; i++)
            {
                BaseCommand actionBaseForSlot = availableCommands.Where(action => action.Slot == i).FirstOrDefault();
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

        private UnityAction HandleClick(BaseCommand action)
        {
            return () => Bus<CommandSelectedEvent>.Raise(new CommandSelectedEvent(action));
        }


    }
}