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


        private void RefreshButtons(HashSet<AbstractCommandable> selectedUnits)
        {

            IEnumerable<BaseCommand> availableCommands = selectedUnits.Count > 0 ? selectedUnits.ElementAt(0).AvailableCommands : Array.Empty<BaseCommand>();
            foreach (AbstractCommandable commandable in selectedUnits)
            {
                //  availableCommands.UnionWith(commandable.AvailableCommands);
                if (commandable.AvailableCommands != null)
                {
                    availableCommands = availableCommands.Intersect(commandable.AvailableCommands);
                }
            }
            for (int i = 0; i < actionButtons.Length; i++)
            {
                BaseCommand actionBaseForSlot = availableCommands.Where(action => action.Slot == i).FirstOrDefault();
                if (actionBaseForSlot != null)
                {
                    actionButtons[i].EnableFor(actionBaseForSlot, selectedUnits, HandleClick(actionBaseForSlot));
                }
                else
                {
                    actionButtons[i].Disable();
                }
            }
        }

        private UnityAction HandleClick(BaseCommand action)
        {
            return () => Bus<CommandSelectedEvent>.Raise(Owner.Player1, new CommandSelectedEvent(action));
        }


    }
}