namespace Gumiho_Rts.Player
{
    using UnityEngine;
    [RequireComponent(typeof(LineRenderer))]
    public class CameraBoundsVisualizer : MonoBehaviour
    {
        [SerializeField] private new Camera camera;
        [SerializeField] private LayerMask floorLayerMask;
        [SerializeField] private float height = 1f; private LineRenderer lineRenderer;
        private Ray[] rays = new Ray[4];

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 4;
        }
        private void Update()
        {
            rays[0] = camera.ViewportPointToRay(new Vector3(0, 0, 0)); // bottom left
            rays[1] = camera.ViewportPointToRay(new Vector3(0, 1, 0));  // top left
            rays[2] = camera.ViewportPointToRay(new Vector3(1, 1, 0)); // top right
            rays[3] = camera.ViewportPointToRay(new Vector3(1, 0, 0)); // bottom right

            for (int i = 0; i < 4; i++)
            {
                if (Physics.Raycast(rays[i], out RaycastHit hit, 1000f, floorLayerMask))
                {
                    lineRenderer.SetPosition(i, new Vector3(hit.point.x, height, hit.point.z));
                }
            }
        }
    }
}