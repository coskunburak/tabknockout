using TapKnockout.Camera;
using UnityEngine;

namespace TapKnockout.Room
{
    [DisallowMultipleComponent]
    public sealed class RoomBounds : MonoBehaviour
    {
        [SerializeField] private Vector3 center;
        [SerializeField] private Vector3 size = new Vector3(12f, 0f, 18f);
        [SerializeField] private Vector3 cameraTargetCenter;
        [SerializeField] private Vector3 cameraTargetSize = new Vector3(2f, 0f, 10f);
        [SerializeField] private bool useBossCameraOverride;
        [SerializeField] private Vector3 bossCameraTargetCenter;
        [SerializeField] private Vector3 bossCameraTargetSize = new Vector3(2f, 0f, 8f);
        [SerializeField] private CameraBounds cameraBounds;

        public Vector3 WorldCenter => transform.TransformPoint(center);
        public Vector3 WorldSize => new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
        public Bounds WorldBounds => new Bounds(WorldCenter, WorldSize);
        public Vector2 MinXZ => new Vector2(WorldBounds.min.x, WorldBounds.min.z);
        public Vector2 MaxXZ => new Vector2(WorldBounds.max.x, WorldBounds.max.z);
        public Bounds CameraTargetBounds => new Bounds(transform.TransformPoint(cameraTargetCenter), Abs(cameraTargetSize));
        public Bounds BossCameraTargetBounds => new Bounds(transform.TransformPoint(bossCameraTargetCenter), Abs(bossCameraTargetSize));
        public bool UseBossCameraOverride => useBossCameraOverride;
        public CameraBounds CameraBounds => cameraBounds != null ? cameraBounds : GetComponent<CameraBounds>();

        private void Reset()
        {
            cameraBounds = GetComponent<CameraBounds>();
        }

        private void OnValidate()
        {
            size = Abs(size);
            cameraTargetSize = Abs(cameraTargetSize);
            bossCameraTargetSize = Abs(bossCameraTargetSize);
        }

        public void SetBounds(Vector3 localCenter, Vector3 localSize)
        {
            center = localCenter;
            size = Abs(localSize);
        }

        public void SetCameraTargetBounds(Vector3 localCenter, Vector3 localSize)
        {
            cameraTargetCenter = localCenter;
            cameraTargetSize = Abs(localSize);
        }

        public void SetBossCameraOverride(Vector3 localCenter, Vector3 localSize)
        {
            useBossCameraOverride = true;
            bossCameraTargetCenter = localCenter;
            bossCameraTargetSize = Abs(localSize);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.45f, 0.25f);
            Gizmos.DrawCube(WorldBounds.center, WorldBounds.size);
            Gizmos.color = new Color(0.2f, 0.8f, 0.45f, 1f);
            Gizmos.DrawWireCube(WorldBounds.center, WorldBounds.size);
        }

        private static Vector3 Abs(Vector3 value)
        {
            return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        }
    }
}
