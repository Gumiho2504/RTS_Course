using UnityEngine.UI;
using UnityEngine;

using Gumiho_Rts.Commands;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using Gumiho_Rts.Units;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gumiho_Rts.TechTree;
using UnityEngine.InputSystem;

namespace Gumiho_Rts.UI.Components
{
    [RequireComponent(typeof(Button))]
    public class UIActionButton : MonoBehaviour, IUIElement<BaseCommand, IEnumerable<AbstractCommandable>, UnityAction>, IPointerEnterHandler, IPointerExitHandler
    {

        [SerializeField] private Image icon;
        [SerializeField] private Tooltip tooltip;
        private RectTransform rectTransform;
        private Button button;

        private static readonly string MINERALS_FORMAT = "{0}  <color=#00ACFF>Minerals</color>. ";
        private static readonly string GAS_FORMAT = "{0}  <color=#3BEA60>Gas</color>. ";
        private static readonly string DEPENDENCY_FORMAT_NO_COMMA = "<color=#AC0000>{0}</color>. ";
        private static readonly string DEPENDENCY_FORMAT_COMMA = "<color=#AC0000>{0}</color>, ";
        private static readonly string POPULATION_FORMAT = "{0} <color=#eeeeee>Population</color>  ";
        private static readonly string HOTKEY_FORMAT = "(<color=#FFFF00>{0}</color>)\n";


        private bool wasAssignedThisFrame;
        private bool isActive = false;
        private Key hotKey;
        private void Awake()
        {
            button = GetComponent<Button>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (button.interactable
                    && !wasAssignedThisFrame
                    && hotKey != Key.None
                    && Keyboard.current[hotKey].wasReleasedThisFrame
            )
            {
                button.onClick?.Invoke();
            }

            wasAssignedThisFrame = false;
        }

        public void EnableFor(BaseCommand command, IEnumerable<AbstractCommandable> selectedUnits, UnityAction onClick)
        {
            SetIcon(command.Icon);

            hotKey = command.HotKey;
            wasAssignedThisFrame = true;

            button.interactable = selectedUnits.Any(commandable => !command.IsLocked(new CommandContext(commandable, new RaycastHit())));
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick);
            isActive = true;
            if (tooltip != null)
            {
                tooltip.SetText(GetTooltipText(command));
            }
        }


        public void Disable()
        {
            SetIcon(null);
            button.interactable = false;
            button.onClick.RemoveAllListeners();
            isActive = false;
        }
        void SetIcon(Sprite icon)
        {
            if (!icon) this.icon.enabled = false;
            else this.icon.enabled = true;
            this.icon.sprite = icon;
            if (tooltip != null)
            {
                tooltip.Hide();
            }
            CancelInvoke();
        }

        public void OnPointerEnter(PointerEventData _)
        {
            if (isActive)
                Invoke(nameof(ShowTooltip), 0.5f);
        }

        public void ShowTooltip()
        {
            if (tooltip != null)
            {
                tooltip.Show();
                tooltip.Rect.position = new Vector2(rectTransform.position.x + rectTransform.sizeDelta.x / 2, rectTransform.position.y + rectTransform.sizeDelta.y / 2);
            }
            CancelInvoke();
        }

        public void OnPointerExit(PointerEventData _)
        {
            if (tooltip != null)
            {
                tooltip.Hide();
            }
            CancelInvoke();

        }
        private string GetTooltipText(BaseCommand command)
        {
            string tooltipText = command.Name;

            if (command.HotKey != Key.None)
            {
                tooltipText += string.Format(HOTKEY_FORMAT, command.HotKey);
            }
            else
            {
                tooltipText += "\n";
            }

            SupplyCostSO supplyCostSO = null;
            PopulationConfigSO populationConfigSO = null;

            if (command is BuildBuildingCommand buildBuildingCommand)
            {
                supplyCostSO = buildBuildingCommand.BuildingSO.Cost;
            }
            else if (command is BuildUnitCommand buildUnitCommand)
            {
                supplyCostSO = buildUnitCommand.Unit.Cost;
                populationConfigSO = buildUnitCommand.Unit.PopulationConfig;
            }
            if (supplyCostSO != null)
            {
                if (supplyCostSO.Minerals > 0)
                {
                    tooltipText += String.Format(MINERALS_FORMAT, supplyCostSO.Minerals);
                }
                if (supplyCostSO.Gas > 0)
                {
                    tooltipText += String.Format(GAS_FORMAT, supplyCostSO.Gas);
                }
            }

            if (populationConfigSO != null && populationConfigSO.PopulationCost > 0)
            {
                tooltipText += string.Format(POPULATION_FORMAT, populationConfigSO.PopulationCost);
            }

            if (command.IsLocked(new CommandContext(Owner.Player1, null, new RaycastHit()))
            && command is IUnlockableCommand unlockableCommand)
            {
                UnlockableSO[] dependencies = unlockableCommand.GetUnmetDependencies(Owner.Player1);
                if (dependencies.Count() > 0)
                {
                    tooltipText += "\nRequires: ";
                }
                for (int i = 0; i < dependencies.Length; i++)
                {
                    tooltipText += i == dependencies.Length - 1 ? String.Format(DEPENDENCY_FORMAT_NO_COMMA, dependencies[i].Name)
                     : String.Format(DEPENDENCY_FORMAT_COMMA, dependencies[i].Name);
                }

            }

            return tooltipText;
        }
    }

}