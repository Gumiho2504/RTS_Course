namespace Gumiho_Rts
{
    using UnityEngine;
    [System.Serializable]
    public class CameraConfig
    {
        [field: SerializeField] public  bool EnableEdgePanning { get; set; } = true;
        [field: SerializeField] public float MousePanSpeed { get; private set; } = 5f;
        [field: SerializeField] public float EdgePanSize { get; private set; } = 50f;
        [field: SerializeField] public float MouseSensitivity { get; private set; } = 100f;
        [field: SerializeField] public float KeyboardPanSpeed { get; private set; } = 10f;
        [field: SerializeField] public float MinZoomDistance { get; private set; } = 7.5f;
        [field: SerializeField] public float ZoomSpeed { get; private set; } = 1f;
        [field: SerializeField] public float RotationSpeed { get; private set; } = 1f;
    }
}
