using System;
using System.Collections.Generic;
using System.Linq;
using Gumiho_Rts.Commands;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.TechTree;
using Gumiho_Rts.UI.Components;
using Gumiho_Rts.Units;
using UnityEngine;
using UnityEngine.Events;

namespace Gumiho_Rts.UI.Containers
{
    public class ActionUI : MonoBehaviour, IUIElement<HashSet<AbstractCommandable>>
    {
        [SerializeField] private UIActionButton[] actionButtons;
        private HashSet<BaseBuilding> selectedBuildings = new();


        public void EnableFor(HashSet<AbstractCommandable> selectedUnits)
        {
            RefreshButtons(selectedUnits);

            foreach (BaseBuilding building in selectedBuildings)
            {
                building.OnQueueUpdated -= OnBuildingQueueUpdate;
            }

            selectedBuildings = selectedUnits.Where(selectedUnit => selectedUnit is BaseBuilding).Cast<BaseBuilding>().ToHashSet();

            foreach (BaseBuilding building in selectedBuildings)
            {
                building.OnQueueUpdated += OnBuildingQueueUpdate;
            }
        }



        public void Disable()
        {
            foreach (UIActionButton button in actionButtons)
            {
                button.Disable();
            }

            foreach (BaseBuilding building in selectedBuildings)
            {
                building.OnQueueUpdated -= OnBuildingQueueUpdate;
            }

            selectedBuildings.Clear();
        }


        private void RefreshButtons(HashSet<AbstractCommandable> selectedUnits)
        {

            IEnumerable<BaseCommand> availableCommands = selectedUnits.Count > 0 ? selectedUnits.ElementAt(0).AvailableCommands : Array.Empty<BaseCommand>();
            if (availableCommands != null)
                availableCommands = availableCommands.
                                                                                                        Where(action =>
                                                                                                                                    action.IsAvailable(
                                                                                                                                                    new CommandContext(Owner.Player1,
                                                                                                                                                    selectedUnits.FirstOrDefault(),
                                                                                                                                                    new RaycastHit())));
            else availableCommands = Array.Empty<BaseCommand>();
            
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

        private void OnBuildingQueueUpdate(UnlockableSO[] unitsInQueue)
        {
            RefreshButtons(selectedBuildings.Cast<AbstractCommandable>().ToHashSet());
        }

        private UnityAction HandleClick(BaseCommand action)
        {
            return () => Bus<CommandSelectedEvent>.Raise(Owner.Player1, new CommandSelectedEvent(action));
        }


    }
}