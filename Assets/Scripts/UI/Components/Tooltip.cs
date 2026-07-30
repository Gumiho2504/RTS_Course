using TMPro;
using UnityEngine;

namespace Gumiho_Rts.UI.Components
{
    public class Tooltip : MonoBehaviour
    {

        [field: SerializeField] public RectTransform Rect { get; private set; }
        [field: SerializeField][Range(0f, 1f)] public float HoverDelay { get; private set; } = 0.5f;
        [SerializeField] private TextMeshProUGUI text;
        void Awake() => Rect = GetComponent<RectTransform>();
        public void SetText(string text)
        {
            this.text.SetText(text);
            Vector2 preferredSize = this.text.GetPreferredValues();
            Rect.sizeDelta = new Vector2(preferredSize.x + 50f, Rect.sizeDelta.y + 25);
        }
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}