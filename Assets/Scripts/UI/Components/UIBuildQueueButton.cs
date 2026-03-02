using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Gumiho_Rts.UI.Components
{
    [RequireComponent(typeof(Button))]
    public class UIBuildQueueButton : MonoBehaviour, IUIElement<UnitSO, UnityAction>
    {

        [SerializeField] private Image icon;
        private Button button;
        private void Awake()
        {
            button = GetComponent<Button>();
            Disable();
        }

        public void EnableFor(UnitSO unit, UnityAction onClick)
        {
            button.onClick.RemoveAllListeners();
            button.interactable = true;
            button.onClick.AddListener(onClick);
            icon.gameObject.SetActive(true);
            icon.sprite = unit.Icon;
        }

        public void Disable()
        {
            button.interactable = false;
            button.onClick.RemoveAllListeners();
            icon.gameObject.SetActive(false);
        }
        void SetIcon(Sprite icon)
        {
            if (!icon) this.icon.enabled = false;
            else this.icon.enabled = true;
            this.icon.sprite = icon;
        }
    }

}