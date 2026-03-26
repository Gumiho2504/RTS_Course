using Gumiho_Rts.Units;
using TMPro;
using UnityEngine;

namespace Gumiho_Rts.UI.Containers
{
    public class SingleUnitSelectedUI : MonoBehaviour, IUIElement<AbstractCommandable>
    {

        [SerializeField] private TextMeshProUGUI unitNameText;
        public void EnableFor(AbstractCommandable item)
        {
            gameObject.SetActive(true);
            unitNameText.SetText(item.UnitSO.Name);
        }
        public void Disable()
        {
            gameObject.SetActive(false);
        }


    }
}