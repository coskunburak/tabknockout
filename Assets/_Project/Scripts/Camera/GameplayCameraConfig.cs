using UnityEngine;

namespace TapKnockout.Camera
{
    [CreateAssetMenu(fileName = "GameplayCameraConfig", menuName = "Tap Knockout/Camera/Gameplay Camera Config")]
    public sealed class GameplayCameraConfig : ScriptableObject
    {
        [Header("Follow")]
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 12.5f, -8.75f);
        [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 0.8f, 4.25f);
        [SerializeField, Min(0.01f)] private float positionSmoothTime = 0.1f;
        [SerializeField, Min(0.01f)] private float rotationSharpness = 16f;
        [SerializeField] private bool snapOnEnable = true;

        [Header("Projection")]
        [SerializeField] private bool useOrthographic = true;
        [SerializeField, Range(30f, 70f)] private float fieldOfView = 42f;
        [SerializeField, Min(1f)] private float orthographicSize = 7.25f;
        [SerializeField, Min(0.01f)] private float nearClipPlane = 0.1f;
        [SerializeField, Min(1f)] private float farClipPlane = 120f;

        [Header("Portrait Framing")]
        [SerializeField, Range(0.4f, 1.2f)] private float minimumSupportedAspect = 0.46f;
        [SerializeField, Range(0.4f, 1.2f)] private float maximumSupportedAspect = 0.75f;

        public Vector3 FollowOffset => followOffset;
        public Vector3 LookAtOffset => lookAtOffset;
        public float PositionSmoothTime => positionSmoothTime;
        public float RotationSharpness => rotationSharpness;
        public bool SnapOnEnable => snapOnEnable;
        public bool UseOrthographic => useOrthographic;
        public float FieldOfView => fieldOfView;
        public float OrthographicSize => orthographicSize;
        public float NearClipPlane => nearClipPlane;
        public float FarClipPlane => farClipPlane;
        public float MinimumSupportedAspect => minimumSupportedAspect;
        public float MaximumSupportedAspect => maximumSupportedAspect;

        private void OnValidate()
        {
            positionSmoothTime = Mathf.Max(0.01f, positionSmoothTime);
            rotationSharpness = Mathf.Max(0.01f, rotationSharpness);
            fieldOfView = Mathf.Clamp(fieldOfView, 30f, 70f);
            orthographicSize = Mathf.Max(1f, orthographicSize);
            nearClipPlane = Mathf.Max(0.01f, nearClipPlane);
            farClipPlane = Mathf.Max(nearClipPlane + 1f, farClipPlane);
            minimumSupportedAspect = Mathf.Clamp(minimumSupportedAspect, 0.4f, maximumSupportedAspect);
            maximumSupportedAspect = Mathf.Clamp(maximumSupportedAspect, minimumSupportedAspect, 1.2f);
        }
    }
}
