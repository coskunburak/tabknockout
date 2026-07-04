using UnityEngine;

namespace TapKnockout.Camera
{
    [CreateAssetMenu(fileName = "GameplayCameraConfig", menuName = "Tap Knockout/Camera/Gameplay Camera Config")]
    public sealed class GameplayCameraConfig : ScriptableObject
    {
        [Header("Composition")]
        [SerializeField, Range(35f, 75f)] private float pitchDegrees = 58f;
        [SerializeField, Range(-35f, 35f)] private float yawDegrees;
        [SerializeField, Min(1f)] private float cameraDistance = 24f;
        [SerializeField] private Vector3 followOffset = Vector3.zero;
        [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 0.85f, 0f);
        [SerializeField] private Vector2 playerViewportAnchor = new Vector2(0.5f, 0.44f);
        [SerializeField, Min(0f)] private float forwardLookAhead = 0.25f;

        [Header("Motion")]
        [SerializeField, Min(0.01f)] private float positionSmoothTime = 0.08f;
        [SerializeField, Min(0.01f)] private float rotationSharpness = 18f;
        [SerializeField] private bool snapOnEnable = true;

        [Header("Projection")]
        [SerializeField] private bool useOrthographic = true;
        [SerializeField, Range(30f, 70f)] private float fieldOfView = 42f;
        [SerializeField, Min(1f)] private float orthographicSize = 16.5f;
        [SerializeField, Min(0.01f)] private float nearClipPlane = 0.1f;
        [SerializeField, Min(1f)] private float farClipPlane = 180f;

        [Header("Portrait Framing")]
        [SerializeField, Range(0.4f, 1.2f)] private float minimumSupportedAspect = 0.46f;
        [SerializeField, Range(0.4f, 1.2f)] private float maximumSupportedAspect = 0.75f;

        public float PitchDegrees => pitchDegrees;
        public float YawDegrees => yawDegrees;
        public float CameraDistance => cameraDistance;
        public Vector3 FollowOffset => followOffset;
        public Vector3 LookAtOffset => lookAtOffset;
        public Vector2 PlayerViewportAnchor => playerViewportAnchor;
        public float ForwardLookAhead => forwardLookAhead;
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
            pitchDegrees = Mathf.Clamp(pitchDegrees, 35f, 75f);
            yawDegrees = Mathf.Clamp(yawDegrees, -35f, 35f);
            cameraDistance = Mathf.Max(1f, cameraDistance);
            playerViewportAnchor = new Vector2(
                Mathf.Clamp(playerViewportAnchor.x, 0.35f, 0.65f),
                Mathf.Clamp(playerViewportAnchor.y, 0.25f, 0.55f));
            forwardLookAhead = Mathf.Max(0f, forwardLookAhead);
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
