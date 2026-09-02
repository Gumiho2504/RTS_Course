using Gumiho_Rts.UI.Components;
using Gumiho_Rts.Units;
using TMPro;
using UnityEngine;

namespace Gumiho_Rts.UI.Containers
{
    public class SingleUnitSelectedUI : MonoBehaviour, IUIElement<AbstractCommandable>
    {

        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private StatIcon damageIcon;
        public void EnableFor(AbstractCommandable item)
        {
            gameObject.SetActive(true);

            unitNameText.SetText(item.UnitSO.Name);
            damageIcon.EnableFor(item);
        }
        public void Disable()
        {
            damageIcon.Disable();
            gameObject.SetActive(false);

        }


    }
}