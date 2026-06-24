using UnityEngine;

namespace TapKnockout.Camera
{
    [DisallowMultipleComponent]
    public sealed class CameraBounds : MonoBehaviour
    {
        [SerializeField] private Vector3 center;
        [SerializeField] private Vector3 size = new Vector3(12f, 0f, 18f);
        [SerializeField] private Color gizmoColor = new Color(0.2f, 0.65f, 1f, 0.35f);

        public Bounds WorldBounds => new Bounds(
            transform.TransformPoint(center),
            new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z)));

        public Vector3 Center
        {
            get => center;
            set => center = value;
        }

        public Vector3 Size
        {
            get => size;
            set => size = new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        }

        private void OnValidate()
        {
            Size = size;
        }

        public Vector3 ClampPosition(Vector3 position)
        {
            return CameraFramingUtility.ClampPositionToBounds(position, WorldBounds);
        }

        private void OnDrawGizmosSelected()
        {
            var bounds = WorldBounds;
            Gizmos.color = gizmoColor;
            Gizmos.DrawCube(bounds.center, bounds.size);
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}
