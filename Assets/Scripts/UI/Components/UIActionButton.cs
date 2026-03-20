using UnityEngine.UI;
using UnityEngine;

using Gumiho_Rts.Commands;
using UnityEngine.Events;

namespace Gumiho_Rts.UI.Components
{
    [RequireComponent(typeof(Button))]
    public class UIActionButton : MonoBehaviour, IUIElement<BaseCommand, UnityAction>
    {

        [SerializeField] private Image icon;
        private Button button;
        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void EnableFor(BaseCommand action, UnityAction onClick)
        {
            SetIcon(action.Icon);
            button.interactable = true;
            button.onClick.AddListener(onClick);
        }

        public void Disable()
        {
            SetIcon(null);
            button.interactable = false;
            button.onClick.RemoveAllListeners();
        }
        void SetIcon(Sprite icon)
        {
            if (!icon) this.icon.enabled = false;
            else this.icon.enabled = true;
            this.icon.sprite = icon;
        }
    }

}