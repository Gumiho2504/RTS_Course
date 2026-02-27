
using UnityEngine;
using System;

namespace Gumiho_Rts.UI.Components
{
    public class Progressbar : MonoBehaviour
    {
        [SerializeField] private Vector2 padding = new(9, 8);
        [SerializeField] private RectTransform mask;
        //  [SerializeField][Range(0, 1)] private float progress;
        private RectTransform maskParentRect;


        private void Awake()
        {
            if (!mask)
            {
                Debug.LogError($"Progressbar {name} does not have a mask");
                return;
            }
            maskParentRect = mask.parent.GetComponent<RectTransform>();
        }


        public void SetProgress(float progress)
        {

            Vector2 parentSize = maskParentRect.sizeDelta;
            Vector2 targetSize = parentSize;

            targetSize.x *= Mathf.Clamp01(progress);
            mask.offsetMin = padding;
            mask.offsetMax = new Vector2(targetSize.x + padding.x - parentSize.x, -padding.y);
        }
    }



}