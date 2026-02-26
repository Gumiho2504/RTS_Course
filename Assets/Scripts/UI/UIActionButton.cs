using UnityEngine.UI;
using UnityEngine;

namespace Gumiho_Rts.UI
{
    public class UIActionButton : MonoBehaviour
    {
        [SerializeField] private Image icon;
        public void SetIcon(Sprite icon)
        {
            if(!icon) this.icon.enabled = false; 
            else this.icon.enabled = true;
            this.icon.sprite = icon;
        }
    }
}