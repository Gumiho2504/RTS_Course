using UnityEngine.UI;
using UnityEngine;

using Gumiho_Rts.Commands;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using Gumiho_Rts.Units;

namespace Gumiho_Rts.UI.Components
{
    [RequireComponent(typeof(Button))]
    public class UIActionButton : MonoBehaviour, IUIElement<BaseCommand, UnityAction>, IPointerEnterHandler, IPointerExitHandler
    {

        [SerializeField] private Image icon;
        [SerializeField] private Tooltip tooltip;
        private RectTransform rectTransform;
        private Button button;

        private bool isActive = false;
        private void Awake()
        {
            button = GetComponent<Button>();
            rectTransform = GetComponent<RectTransform>();
        }

        public void EnableFor(BaseCommand command, UnityAction onClick)
        {
            SetIcon(command.Icon);
            button.interactable = !command.IsLocked(new CommandContext());
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
            string tooltipText = command.Name + "\n";
            SupplyCostSO supplyCostSO = null;
            if (command is BuildBuildingCommand buildBuildingCommand)
            {
                supplyCostSO = buildBuildingCommand.BuildingSO.Cost;

            }
            else if (command is BuildUnitCommand buildUnitCommand)
            {
                supplyCostSO = buildUnitCommand.Unit.Cost;
            }
            if (supplyCostSO != null)
            {
                if (supplyCostSO.Minerals > 0)
                {
                    tooltipText += $"{supplyCostSO.Minerals}  Minerals. ";
                }
                if (supplyCostSO.Gas > 0)
                {
                    tooltipText += $"{supplyCostSO.Gas}  Gas. ";
                }
            }
            return tooltipText;
        }
    }

}