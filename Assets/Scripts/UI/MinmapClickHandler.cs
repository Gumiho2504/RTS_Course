using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Gumiho_Rts.UI
{
    [RequireComponent(typeof(EventTrigger))]
    public class MinimapRenderer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private Transform cameraTrarget;
        [SerializeField] private LayerMask floorLayerMask;

        private bool isMouseDownOnMinimap = false;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (minimapCamera == null || cameraTrarget == null)
            {
                Debug.LogError("MinimapRenderer: Minimap camera or camera target is not set");
                enabled = false;
            }
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                isMouseDownOnMinimap = true;
                MoveVirtualCamera(eventData.position); ;
            }
        }


        public void OnPointerMove(PointerEventData eventData) => MoveVirtualCamera(eventData.position);
        public void OnPointerEnter(PointerEventData eventData)
        {

        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                isMouseDownOnMinimap = false;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isMouseDownOnMinimap = false;
        }



        private void MoveVirtualCamera(Vector2 mousePosition)
        {
            if (!isMouseDownOnMinimap) return;

            float widthMultiplier = minimapCamera.scaledPixelWidth / rectTransform.rect.width;
            float heightMultiplier = minimapCamera.scaledPixelHeight / rectTransform.rect.height;

            Vector2 convertedMousePosition = new Vector2(mousePosition.x * widthMultiplier, mousePosition.y * heightMultiplier);

            Ray ray = minimapCamera.ScreenPointToRay(convertedMousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, floorLayerMask))
            {
                cameraTrarget.position = hit.point;
            }
        }

    }
}