namespace Gumiho_Rts.UI.Containers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Gumiho_Rts.UI.Components;
    using Gumiho_Rts.Units;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ControlGroupUI : MonoBehaviour, IUIElement<HashSet<AbstractCommandable>>
    {
        [SerializeField] private ControlGroupKeyboardHotkey[] controlGroupHotKeys;
        private HashSet<AbstractCommandable> selectedUnits;

        private void Update()
        {
            if (!Keyboard.current.ctrlKey.isPressed)
            {
                return;
            }

            foreach (var groupHotKey in controlGroupHotKeys)
            {
                if (Keyboard.current[groupHotKey.Key].wasReleasedThisFrame && selectedUnits.Count > 0)
                {
                    groupHotKey.Group.EnableFor(selectedUnits, groupHotKey.Key, SelectedUnits);
                }
            }
        }

        private void SelectedUnits(HashSet<AbstractCommandable> units)
        {
            foreach (ISelectable selectable in selectedUnits.ToList())
            {
                selectable.Deselect();
            }

            foreach (ISelectable selectable in units)
            {
                selectable.Select();
            }
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void EnableFor(HashSet<AbstractCommandable> items)
        {
            selectedUnits = items;
        }

        [System.Serializable]
        private struct ControlGroupKeyboardHotkey
        {
            [field: SerializeField] public Key Key { get; private set; }
            [field: SerializeField] public ControlGroup Group { get; private set; }
        }
    }
}