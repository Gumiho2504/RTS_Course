using Gumiho_Rts.Commands;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

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
        private BaseCommand activeCommand;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (minimapCamera == null || cameraTrarget == null)
            {
                Debug.LogError("MinimapRenderer: Minimap camera or camera target is not set");
                enabled = false;
            }

            Bus<CommandSelectedEvent>.OnEvent[Units.Owner.Player1] += HandleCommandSelected;
            Bus<CommandIssuedEvent>.OnEvent[Units.Owner.Player1] += HandleCommandIssued;

        }

        private void HandleCommandSelected(CommandSelectedEvent evt) => activeCommand = evt.Command;
        private void HandleCommandIssued(CommandIssuedEvent evt) => activeCommand = null; 


        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && activeCommand != null)
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
                  RaisClickEvent(eventData.position, MouseButton.Left);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                RaisClickEvent(eventData.position, MouseButton.Right);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isMouseDownOnMinimap = false;
        }



        private void MoveVirtualCamera(Vector2 mousePosition)
        {
            if (!isMouseDownOnMinimap) return;

            if (RaycastFromMousePostion(mousePosition, out RaycastHit hit))
            {
                cameraTrarget.position = hit.point;
            }
        }

        private bool RaycastFromMousePostion(Vector2 mousePosition, out RaycastHit hit)
        {
            float widthMultiplier = minimapCamera.scaledPixelWidth / rectTransform.rect.width;
            float heightMultiplier = minimapCamera.scaledPixelHeight / rectTransform.rect.height;

            Vector2 convertedMousePosition = new Vector2(mousePosition.x * widthMultiplier, mousePosition.y * heightMultiplier);

            Ray ray = minimapCamera.ScreenPointToRay(convertedMousePosition);
            return Physics.Raycast(ray, out hit, float.MaxValue, floorLayerMask);

        }

        private void RaisClickEvent(Vector2 mousePosition, MouseButton button)
        {
            if(RaycastFromMousePostion(mousePosition, out RaycastHit hit))
            {
                Bus<MinimapClickEvent>.Raise(Units.Owner.Player1,new MinimapClickEvent(button,hit));
            }
        }

    }
}