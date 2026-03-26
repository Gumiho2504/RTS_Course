using Gumiho_Rts.UI;
using Gumiho_Rts.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Gumiho_Rts.UI.Containers
{
    public class UnitIconUI : MonoBehaviour, IUIElement<AbstractCommandable>
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI healthText;
        private const string HEALTH_TEXT_FORMAT = "{0} / {1}";

        public void EnableFor(AbstractCommandable commandable)
        {
            gameObject.SetActive(true);
            icon.sprite = commandable.UnitSO.Icon;
            healthText.text = string.Format(HEALTH_TEXT_FORMAT, commandable.CurrentHealth, commandable.MaxHealth);

        }
        public void Disable()
        {
            gameObject.SetActive(false);

        }


    }
}