namespace Gumiho_Rts
{
    using System;
    using Unity.Cinemachine;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private Rigidbody targetCamera;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private CameraConfig cameraConfig;

        private CinemachineFollow cinemachineFollow;
        private float zoomStartTime;
        private float rotationStartTime;
        private Vector3 startingFollowOffset;
        private float minRotationAmount;

        private void Awake()
        {
            if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
            {
                Debug.LogError("Cinemachine Camera does not have a CinemachineFollow component.");
            }
            startingFollowOffset = cinemachineFollow.FollowOffset;
            minRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);
        }



        // Update is called once per frame
        void Update()
        {
            HandlePanning();
            HandleZooming();
            HandleRotation();
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
                print($"Mouse Position: {mousePosition}. edgePanSize: {cameraConfig.EdgePanSize}");
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
                moveData.y += cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.leftArrowKey.isPressed)
            {
                moveData.x -= cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.rightArrowKey.isPressed)
            {
                moveData.x += cameraConfig.KeyboardPanSpeed;
            }
            return moveData;
        }
    }



}