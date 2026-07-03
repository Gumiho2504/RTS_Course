using Gumiho_Rts.Units;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gumiho_Rts.UI.Components
{
    [RequireComponent(typeof(Button))]
    public class UIUnitButton : MonoBehaviour, IUIElement<ITransportable, UnityAction>
    {
        [SerializeField] private Image icon;
        private Button button;
        private void Awake()
        {
            button = GetComponent<Button>();
            Disable();
        }
        public void Disable()
        {
            button.onClick.RemoveAllListeners();
            gameObject.SetActive(false);
        }

        public void EnableFor(ITransportable item, UnityAction callback)
        {
            gameObject.SetActive(true);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(callback);

            icon.sprite = item.Icon;
        }
    }
}