
using System;
using System.Collections.Generic;
using System.Linq;
using Gumiho_Rts.Commands;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.Units;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Gumiho_Rts
{


    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private Rigidbody targetCamera;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private new Camera camera;
        [SerializeField] private CameraConfig cameraConfig;
        [SerializeField] private LayerMask selectableUnityLayerMask;
        [SerializeField] private LayerMask floorLayerMask;
        [SerializeField] private LayerMask interactableLayerMask;
        [SerializeField] private RectTransform selectionBox;
        [SerializeField][ColorUsage(showAlpha: true, hdr: true)] private Color errorTintColor = Color.red;
        [SerializeField][ColorUsage(showAlpha: true, hdr: true)] private Color errorFresnelColor = new(4, 1.7f, 0, 2);
        [SerializeField][ColorUsage(showAlpha: true, hdr: true)] private Color availableToPlaceTintColor = new(0.2f, 0.65f, 1, 2);
        [SerializeField][ColorUsage(showAlpha: true, hdr: true)] private Color availableToPlaceFresnelColor = new(4, 1.7f, 0, 2);

        private CinemachineFollow cinemachineFollow;
        private float zoomStartTime;
        private float rotationStartTime;
        private Vector3 startingFollowOffset;
        private Vector2 startingMousePosition;
        private float minRotationAmount;
        public HashSet<AbstractUnit> AliveUnits = new(100);
        private HashSet<AbstractUnit> addedUnits = new(24);
        private List<ISelectable> selectableUnits = new(12);
        private BaseCommand activeCommand;
        private bool wasMouseDownOnUI;

        private GameObject ghostInstance;
        private MeshRenderer ghostRenderer;
        private static readonly int TINT = Shader.PropertyToID("_Tint");
        private static readonly int FRESNEL = Shader.PropertyToID("_FresnelColor");

        [Space(10)]
    
        [SerializeField] private bool IsHandlePanning = false;

        private void Awake()
        {
            if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
            {
                Debug.LogError("Cinemachine Camera does not have a CinemachineFollow component.");
            }
            startingFollowOffset = cinemachineFollow.FollowOffset;
            minRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);

            Bus<UnitSelectedEvent>.OnEvent[Owner.Player1] += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent[Owner.Player1] += HandleUnitDeselected;
            Bus<UnitSpawnEvent>.OnEvent[Owner.Player1] += HandleUnitSpawned;
            Bus<CommandSelectedEvent>.OnEvent[Owner.Player1] += HandleActionSelected;
            Bus<UnitDeathEvent>.OnEvent[Owner.Player1] += HandleUnitDeath;

        }



        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent[Owner.Player1] -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent[Owner.Player1] -= HandleUnitDeselected;
            Bus<UnitSpawnEvent>.OnEvent[Owner.Player1] -= HandleUnitSpawned;
            Bus<CommandSelectedEvent>.OnEvent[Owner.Player1] -= HandleActionSelected;
            Bus<UnitDeathEvent>.OnEvent[Owner.Player1] -= HandleUnitDeath;


        }



        private void HandleActionSelected(CommandSelectedEvent args)
        {
            activeCommand = args.Command;
            //Debug.Log($"<color=green> {activeCommand.Name}</color>");
            if (!activeCommand.RequiresClickToActivate)
            {
                ActivateAction(new RaycastHit());
            }
            else if (activeCommand.GhostPrefab != null)
            {
                ghostInstance = Instantiate(activeCommand.GhostPrefab);
                ghostRenderer = ghostInstance.GetComponentInChildren<MeshRenderer>();
            }
        }

        private void HandleUnitSpawned(UnitSpawnEvent args)
        {
            AliveUnits.Add(args.Unit);

        }
        private void HandleUnitDeselected(UnitDeselectedEvent args)
        {
            selectableUnits.Remove(args.Unit);
        }


        private void HandleUnitSelected(UnitSelectedEvent evt)
        {

            if (!selectableUnits.Contains(evt.Unit))
            {
                selectableUnits.Add(evt.Unit);
            }

        }

        private void HandleUnitDeath(UnitDeathEvent args)
        {
            selectableUnits.Remove(args.Unit);
            if (args.Unit is AbstractUnit unit)
            {
                AliveUnits.Remove(unit);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (IsHandlePanning) HandlePanning();
            HandleZooming();
            HandleRotation();
            HandleGhostAction();
            HandleRightMuseClick();
            HandleDragSelection();
        }
        private void HandleGhostAction()
        {
            if (ghostInstance == null) return;
            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                //print("Activating");
                Destroy(ghostInstance);
                ghostInstance = null;
                activeCommand = null;
                return;
            }
            var mouseVector = Mouse.current.position.ReadValue();
            Ray ray = camera.ScreenPointToRay(mouseVector);
            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, floorLayerMask))
            {
                ghostInstance.transform.position = hit.point;
                bool allRestrictionsPass = activeCommand.AllRestrictionsPass(hit.point);
                ghostRenderer.material.SetColor(TINT, allRestrictionsPass ? availableToPlaceTintColor : errorTintColor);
                ghostRenderer.material.SetColor(FRESNEL, allRestrictionsPass ? availableToPlaceFresnelColor : errorFresnelColor);


            }


        }

        private void HandleDragSelection()
        {

            if (selectionBox == null) return;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleMouseDown();

            }
            else if (Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleDrag();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleMouseUp();
            }
        }

        private void HandleMouseUp()
        {
            if (!Keyboard.current.shiftKey.isPressed && activeCommand == null && !wasMouseDownOnUI)
            {
                DeselectAllUnits();
            }

            HandleLeftMouseClick();
            foreach (AbstractUnit unit in addedUnits)
            {
                unit.Select();
            }
            selectionBox.gameObject.SetActive(false);
        }

        private void HandleDrag()
        {
            if (activeCommand != null || wasMouseDownOnUI) return;
            Bounds selectionBounds = ResizeSelectedBox();
            foreach (AbstractUnit unit in AliveUnits)
            {
                if (!unit.gameObject.activeInHierarchy) continue;
                Vector2 unitPosition = camera.WorldToScreenPoint(unit.transform.position);
                if (selectionBounds.Contains(unitPosition))
                {
                    addedUnits.Add(unit);
                }
            }
        }

        private void HandleMouseDown()
        {
            selectionBox.sizeDelta = Vector2.zero;
            selectionBox.gameObject.SetActive(true);
            startingMousePosition = Mouse.current.position.ReadValue();
            addedUnits.Clear();
            wasMouseDownOnUI = EventSystem.current.IsPointerOverGameObject();
        }

        private void DeselectAllUnits()
        {
            ISelectable[] currentlySelectedUnits = selectableUnits.ToArray();
            foreach (ISelectable selectable in currentlySelectedUnits)
            {
                selectable.Deselect();
            }
        }

        private Bounds ResizeSelectedBox()
        {
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            float width = currentMousePosition.x - startingMousePosition.x;
            float height = currentMousePosition.y - startingMousePosition.y;

            selectionBox.anchoredPosition = startingMousePosition + new Vector2(width, height) / 2f;
            selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
            return new Bounds(selectionBox.anchoredPosition, selectionBox.sizeDelta);
        }

        private void HandleRightMuseClick()
        {
            if (selectableUnits.Count == 0) return;
            // if (activeCommand == null && !wasMouseDownOnUI) return;

            // Debug.Log($"<color=blue>Handle Right Click {activeCommand.name}</color>");


            Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Mouse.current.rightButton.wasReleasedThisFrame && Physics.Raycast(ray, out RaycastHit hit, maxDistance: float.MaxValue, layerMask: floorLayerMask | interactableLayerMask))
            {

                // find applicable command

                // issue command to all units

                print("Clicked on " + hit.transform.name);
                List<AbstractUnit> abstractUnits = new(selectableUnits.Count);
                foreach (ISelectable selectable in selectableUnits)
                {
                    if (selectable is AbstractUnit abstractUnit)
                    {
                        abstractUnits.Add(abstractUnit);
                    }
                }


                for (int i = 0; i < abstractUnits.Count; i++)
                {
                    CommandContext context = new CommandContext(abstractUnits[i], hit, i, MouseButton.Right);

                    foreach (var command in GetAvailableCommands(abstractUnits[i]))
                    {
                        if (command.CanHandle(context))
                        {
                            command.Handle(context);
                            if (command.IsSingleUnitCommand)
                            {
                                return;
                            }
                            break;
                        }
                    }



                }
            }
        }
        private List<BaseCommand> GetAvailableCommands(AbstractUnit unit)
        {
            OverrideCommandsCommand[] overrideCommandsCommands = unit.AvailableCommands
                .Where(command => command is OverrideCommandsCommand)
                .Cast<OverrideCommandsCommand>()
                .ToArray();
            List<BaseCommand> allAvailableCommands = new();
            foreach (OverrideCommandsCommand overrideCommand in overrideCommandsCommands)
            {
                allAvailableCommands.AddRange(overrideCommand.commands.Where(command => command is not OverrideCommandsCommand));
            }
            allAvailableCommands.AddRange(unit.AvailableCommands.Where(command => command is not OverrideCommandsCommand));
            return allAvailableCommands;
        }


        private void HandleLeftMouseClick()
        {
            if (camera == null)
            {
                Debug.LogError("Camera reference is not set on PlayerInput.");
                return;
            }
            var mouseVector = Mouse.current.position.ReadValue();
            Ray ray = camera.ScreenPointToRay(mouseVector);
            //  Debug.Log($"{Physics.Raycast(ray, out RaycastHit ht, float.MaxValue, layerMask: floorLayerMask | interactableLayerMask)} | {ht.transform.name} | {ht.transform.TryGetComponent(out GatherableSupply s)} | {s}");

            if (activeCommand == null && Physics.Raycast(ray, out RaycastHit hit, maxDistance: 100f, layerMask: selectableUnityLayerMask | interactableLayerMask)
            && hit.transform.TryGetComponent(out ISelectable selectable))
            {

                selectable.Select();
            }
            else if (activeCommand != null
            && !EventSystem.current.IsPointerOverGameObject()
            && Physics.Raycast(ray, out hit, float.MaxValue, layerMask: floorLayerMask | interactableLayerMask))
            {
                ActivateAction(hit);
                // foreach(var selectableUnit  in selectableUnits)
                // {
                //     if(selectableUnit is AbstractUnit unit)
                //     {
                //         abstractUnits.Add(unit);
                //     }
                // }
            }

        }

        private void ActivateAction(RaycastHit hit)
        {
            if (ghostInstance != null)
            {
                Destroy(ghostInstance);
                ghostInstance = null;
            }
            List<AbstractCommandable> abstractCommandable = selectableUnits
                .Where(selectableUnit => selectableUnit is AbstractCommandable)
                .Cast<AbstractCommandable>()
                .ToList();

            for (int i = 0; i < abstractCommandable.Count; i++)
            {
                CommandContext context = new CommandContext(abstractCommandable[i], hit, i);
                if (activeCommand.CanHandle(context))
                {
                    activeCommand.Handle(context);
                    if (activeCommand.IsSingleUnitCommand)
                    {
                        break;
                    }
                }

            }
            activeCommand = null;
        }

        private void HandleRotation()
        {
            if (ShouldSetStartTimeForRotation())
            {
                rotationStartTime = Time.time;
            }
            float rotationTime = Mathf.Clamp01((Time.time - rotationStartTime) * cameraConfig.RotationSpeed);
            Vector3 targetRotation;
            if (Keyboard.current.qKey.isPressed)
            {
                targetRotation = new Vector3(
                    minRotationAmount,
                    cinemachineFollow.FollowOffset.y,
                     0
                );
            }
            else if (Keyboard.current.wKey.isPressed)
            {
                targetRotation = new Vector3(
                    -minRotationAmount,
                    cinemachineFollow.FollowOffset.y,
                     0
                );
            }
            else
            {
                targetRotation = new Vector3(
                    startingFollowOffset.x,
                    cinemachineFollow.FollowOffset.y,
                     startingFollowOffset.z
                );
            }

            cinemachineFollow.FollowOffset = Vector3.Slerp(cinemachineFollow.FollowOffset, targetRotation, rotationTime);
        }

        private bool ShouldSetStartTimeForRotation()
        {
            return Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame
                || Keyboard.current.qKey.wasReleasedThisFrame || Keyboard.current.wKey.wasReleasedThisFrame
            ;
        }

        private void HandleZooming()
        {
            if (ShouldSetStartTimeForZoom())
            {
                zoomStartTime = Time.time;
            }


            float zoomTime = Mathf.Clamp01((Time.time - zoomStartTime) * cameraConfig.ZoomSpeed);

            Vector3 targetOffset;
            if (Keyboard.current.escapeKey.isPressed)
            {
                targetOffset = new Vector3(
                 cinemachineFollow.FollowOffset.x,
                cameraConfig.MinZoomDistance,
                cinemachineFollow.FollowOffset.z
            );
            }
            else
            {
                targetOffset = new Vector3(
                        cinemachineFollow.FollowOffset.x,
                          startingFollowOffset.y,
                           cinemachineFollow.FollowOffset.z
                       );
            }

            cinemachineFollow.FollowOffset = Vector3.Slerp(cinemachineFollow.FollowOffset, targetOffset, zoomTime);
        }

        private bool ShouldSetStartTimeForZoom()
        {
            return Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasReleasedThisFrame;
        }

        private void HandlePanning()
        {
            Vector2 moveData = GetKeyboardMoveData();
            Vector2 mouseData = GetMouseMoveData();
            //  print($"Move Data: {moveData}, Mouse Data: {mouseData}");
            moveData += mouseData;

            // moveData *= Time.deltaTime;
            targetCamera.linearVelocity = new Vector3(moveData.x, 0, moveData.y);
        }
        private Vector2 GetMouseMoveData()
        {
            Vector2 mouseData = Vector2.zero;
            if (!cameraConfig.EnableEdgePanning) return mouseData;

            Vector2 mousePosition = Mouse.current.position.ReadValue();


            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            if (mousePosition.x <= cameraConfig.EdgePanSize)
            {
                mouseData.x -= cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.x >= screenWidth - cameraConfig.EdgePanSize)
            {
                mouseData.x += cameraConfig.MousePanSpeed;
            }

            if (mousePosition.y >= screenHeight - cameraConfig.EdgePanSize)
            {
                mouseData.y += cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.y <= cameraConfig.EdgePanSize)
            {
                mouseData.y -= cameraConfig.MousePanSpeed;
            }

            return mouseData;
        }
        private Vector2 GetKeyboardMoveData()
        {
            Vector2 moveData = Vector2.zero;

            if (Keyboard.current.upArrowKey.isPressed)
            {
                moveData.y -= cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.downArrowKey.isPressed)
            {
                moveData.y -= cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.leftArrowKey.isPressed)
            {
                moveData.x += cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.rightArrowKey.isPressed)
            {
                moveData.x += cameraConfig.KeyboardPanSpeed;
            }
            return moveData;
        }
    }



}