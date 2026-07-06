using UnityEngine;

namespace TapKnockout.Camera
{
    [CreateAssetMenu(fileName = "GameplayCameraConfig", menuName = "Tap Knockout/Camera/Gameplay Camera Config")]
    public sealed class GameplayCameraConfig : ScriptableObject
    {
        [Header("Composition")]
        [SerializeField, Range(35f, 75f)] private float pitchDegrees = 52f;
        [SerializeField, Range(-35f, 35f)] private float yawDegrees;
        [SerializeField, Min(1f)] private float cameraDistance = 18f;
        [SerializeField] private Vector3 followOffset = Vector3.zero;
        [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 0.65f, 0f);
        [SerializeField] private Vector2 playerViewportAnchor = new Vector2(0.5f, 0.49f);
        [SerializeField, Min(0f)] private float forwardLookAhead = 0.65f;
        [SerializeField] private bool enableMovementLookAhead = true;
        [SerializeField, Min(0f)] private float movementLookAheadStrength = 0.9f;
        [SerializeField, Min(0f)] private float maxMovementLookAhead = 1.4f;
        [SerializeField] private bool enableDashLookAhead = true;
        [SerializeField, Min(0f)] private float dashLookAheadMultiplier = 1.45f;
        [SerializeField, Range(0.03f, 0.4f)] private float dashLookAheadDuration = 0.14f;

        [Header("Motion")]
        [SerializeField, Min(0.01f)] private float positionSmoothTime = 0.08f;
        [SerializeField, Min(0.01f)] private float rotationSharpness = 18f;
        [SerializeField] private bool snapOnEnable = true;

        [Header("Projection")]
        [SerializeField] private bool useOrthographic = true;
        [SerializeField, Range(30f, 70f)] private float fieldOfView = 42f;
        [SerializeField, Min(1f)] private float orthographicSize = 11.25f;
        [SerializeField, Min(0.01f)] private float nearClipPlane = 0.1f;
        [SerializeField, Min(1f)] private float farClipPlane = 220f;

        [Header("Desktop Framing")]
        [SerializeField, Range(0.4f, 3f)] private float minimumSupportedAspect = 1.33f;
        [SerializeField, Range(0.4f, 3f)] private float maximumSupportedAspect = 2.4f;

        public float PitchDegrees => pitchDegrees;
        public float YawDegrees => yawDegrees;
        public float CameraDistance => cameraDistance;
        public Vector3 FollowOffset => followOffset;
        public Vector3 LookAtOffset => lookAtOffset;
        public Vector2 PlayerViewportAnchor => playerViewportAnchor;
        public float ForwardLookAhead => forwardLookAhead;
        public bool EnableMovementLookAhead => enableMovementLookAhead;
        public float MovementLookAheadStrength => movementLookAheadStrength;
        public float MaxMovementLookAhead => maxMovementLookAhead;
        public bool EnableDashLookAhead => enableDashLookAhead;
        public float DashLookAheadMultiplier => dashLookAheadMultiplier;
        public float DashLookAheadDuration => dashLookAheadDuration;
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
                Mathf.Clamp(playerViewportAnchor.y, 0.35f, 0.6f));
            forwardLookAhead = Mathf.Max(0f, forwardLookAhead);
            movementLookAheadStrength = Mathf.Max(0f, movementLookAheadStrength);
            maxMovementLookAhead = Mathf.Max(0f, maxMovementLookAhead);
            dashLookAheadMultiplier = Mathf.Max(0f, dashLookAheadMultiplier);
            dashLookAheadDuration = Mathf.Clamp(dashLookAheadDuration, 0.03f, 0.4f);
            positionSmoothTime = Mathf.Max(0.01f, positionSmoothTime);
            rotationSharpness = Mathf.Max(0.01f, rotationSharpness);
            fieldOfView = Mathf.Clamp(fieldOfView, 30f, 70f);
            orthographicSize = Mathf.Max(1f, orthographicSize);
            nearClipPlane = Mathf.Max(0.01f, nearClipPlane);
            farClipPlane = Mathf.Max(nearClipPlane + 1f, farClipPlane);
            minimumSupportedAspect = Mathf.Clamp(minimumSupportedAspect, 0.4f, maximumSupportedAspect);
            maximumSupportedAspect = Mathf.Clamp(maximumSupportedAspect, minimumSupportedAspect, 3f);
        }
    }
}
