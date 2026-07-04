using UnityEngine;

namespace TapKnockout.Camera
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class GameplayCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameplayCameraConfig config;
        [SerializeField] private Transform followTarget;
        [SerializeField] private CameraBounds cameraBounds;

        [Header("Fallback Values")]
        [SerializeField, Range(35f, 75f)] private float fallbackPitchDegrees = 58f;
        [SerializeField, Range(-35f, 35f)] private float fallbackYawDegrees;
        [SerializeField, Min(1f)] private float fallbackCameraDistance = 24f;
        [SerializeField] private Vector3 fallbackFollowOffset = Vector3.zero;
        [SerializeField] private Vector3 fallbackLookAtOffset = new Vector3(0f, 0.85f, 0f);
        [SerializeField] private Vector2 fallbackPlayerViewportAnchor = new Vector2(0.5f, 0.44f);
        [SerializeField, Min(0f)] private float fallbackForwardLookAhead = 0.25f;
        [SerializeField, Min(0.01f)] private float fallbackPositionSmoothTime = 0.08f;
        [SerializeField, Min(0.01f)] private float fallbackRotationSharpness = 18f;
        [SerializeField] private bool fallbackSnapOnEnable = true;
        [SerializeField] private bool fallbackUseOrthographic = true;
        [SerializeField, Range(30f, 70f)] private float fallbackFieldOfView = 42f;
        [SerializeField, Min(1f)] private float fallbackOrthographicSize = 16.5f;
        [SerializeField, Min(0.01f)] private float fallbackNearClipPlane = 0.1f;
        [SerializeField, Min(1f)] private float fallbackFarClipPlane = 180f;
        [SerializeField, Range(0.4f, 1.2f)] private float fallbackMinimumSupportedAspect = 0.46f;
        [SerializeField, Range(0.4f, 1.2f)] private float fallbackMaximumSupportedAspect = 0.75f;

        [Header("Debug")]
        [SerializeField] private bool logSetupWarnings = true;

        private UnityEngine.Camera cachedCamera;
        private Vector3 followVelocity;
        private bool loggedMissingTarget;

        public Transform FollowTarget => followTarget;
        public CameraBounds Bounds => cameraBounds;
        public GameplayCameraConfig Config => config;

        private float PitchDegrees => config != null ? config.PitchDegrees : fallbackPitchDegrees;
        private float YawDegrees => config != null ? config.YawDegrees : fallbackYawDegrees;
        private float CameraDistance => config != null ? config.CameraDistance : fallbackCameraDistance;
        private Vector3 FollowOffset => config != null ? config.FollowOffset : fallbackFollowOffset;
        private Vector3 LookAtOffset => config != null ? config.LookAtOffset : fallbackLookAtOffset;
        private Vector2 PlayerViewportAnchor => config != null ? config.PlayerViewportAnchor : fallbackPlayerViewportAnchor;
        private float ForwardLookAhead => config != null ? config.ForwardLookAhead : fallbackForwardLookAhead;
        private float PositionSmoothTime => config != null ? config.PositionSmoothTime : fallbackPositionSmoothTime;
        private float RotationSharpness => config != null ? config.RotationSharpness : fallbackRotationSharpness;
        private bool SnapOnEnable => config != null ? config.SnapOnEnable : fallbackSnapOnEnable;
        private bool UseOrthographic => config != null ? config.UseOrthographic : fallbackUseOrthographic;
        private float FieldOfView => config != null ? config.FieldOfView : fallbackFieldOfView;
        private float OrthographicSize => config != null ? config.OrthographicSize : fallbackOrthographicSize;
        private float NearClipPlane => config != null ? config.NearClipPlane : fallbackNearClipPlane;
        private float FarClipPlane => config != null ? config.FarClipPlane : fallbackFarClipPlane;
        private float MinimumSupportedAspect => config != null ? config.MinimumSupportedAspect : fallbackMinimumSupportedAspect;
        private float MaximumSupportedAspect => config != null ? config.MaximumSupportedAspect : fallbackMaximumSupportedAspect;

        private void Reset()
        {
            cachedCamera = GetComponent<UnityEngine.Camera>();
            cachedCamera.tag = "MainCamera";
            cachedCamera.usePhysicalProperties = false;
            ApplyProjectionSettings();
        }

        private void Awake()
        {
            cachedCamera = GetComponent<UnityEngine.Camera>();
            ApplyProjectionSettings();
        }

        private void OnEnable()
        {
            if (SnapOnEnable)
            {
                SnapToTarget();
            }
        }

        private void OnValidate()
        {
            fallbackPitchDegrees = Mathf.Clamp(fallbackPitchDegrees, 35f, 75f);
            fallbackYawDegrees = Mathf.Clamp(fallbackYawDegrees, -35f, 35f);
            fallbackCameraDistance = Mathf.Max(1f, fallbackCameraDistance);
            fallbackPlayerViewportAnchor = new Vector2(
                Mathf.Clamp(fallbackPlayerViewportAnchor.x, 0.35f, 0.65f),
                Mathf.Clamp(fallbackPlayerViewportAnchor.y, 0.25f, 0.55f));
            fallbackForwardLookAhead = Mathf.Max(0f, fallbackForwardLookAhead);
            fallbackPositionSmoothTime = Mathf.Max(0.01f, fallbackPositionSmoothTime);
            fallbackRotationSharpness = Mathf.Max(0.01f, fallbackRotationSharpness);
            fallbackFieldOfView = Mathf.Clamp(fallbackFieldOfView, 30f, 70f);
            fallbackOrthographicSize = Mathf.Max(1f, fallbackOrthographicSize);
            fallbackNearClipPlane = Mathf.Max(0.01f, fallbackNearClipPlane);
            fallbackFarClipPlane = Mathf.Max(fallbackNearClipPlane + 1f, fallbackFarClipPlane);
            fallbackMinimumSupportedAspect = Mathf.Clamp(fallbackMinimumSupportedAspect, 0.4f, fallbackMaximumSupportedAspect);
            fallbackMaximumSupportedAspect = Mathf.Clamp(fallbackMaximumSupportedAspect, fallbackMinimumSupportedAspect, 1.2f);

            if (cachedCamera == null)
            {
                cachedCamera = GetComponent<UnityEngine.Camera>();
            }

            ApplyProjectionSettings();
        }

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                if (logSetupWarnings && !loggedMissingTarget)
                {
                    loggedMissingTarget = true;
                    Debug.LogWarning($"{nameof(GameplayCameraController)} on {name} has no follow target assigned.", this);
                }

                return;
            }

            ApplyProjectionSettings();
            Follow(Time.deltaTime);
        }

        public void SetFollowTarget(Transform target, bool snapImmediately)
        {
            followTarget = target;
            loggedMissingTarget = false;

            if (snapImmediately)
            {
                SnapToTarget();
            }
        }

        public void SetBounds(CameraBounds bounds)
        {
            cameraBounds = bounds;
        }

        public void SetRoomBounds(CameraBounds bounds)
        {
            SetBounds(bounds);
        }

        public void SnapToTarget()
        {
            if (followTarget == null)
            {
                return;
            }

            var targetPosition = ResolveDesiredPosition();
            transform.SetPositionAndRotation(targetPosition, ResolveDesiredRotation(targetPosition));
            followVelocity = Vector3.zero;
        }

        public void ApplyProjectionSettings()
        {
            if (cachedCamera == null)
            {
                return;
            }

            cachedCamera.orthographic = UseOrthographic;
            cachedCamera.fieldOfView = FieldOfView;
            cachedCamera.orthographicSize = EffectiveOrthographicSize();
            cachedCamera.nearClipPlane = NearClipPlane;
            cachedCamera.farClipPlane = FarClipPlane;
        }

        private void Follow(float deltaTime)
        {
            var desiredPosition = ResolveDesiredPosition();
            var smoothedPosition = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref followVelocity,
                PositionSmoothTime,
                float.PositiveInfinity,
                deltaTime);

            var desiredRotation = ResolveDesiredRotation(smoothedPosition);
            var rotationT = CameraFramingUtility.ResolveRotationInterpolation(RotationSharpness, deltaTime);
            var smoothedRotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationT);

            transform.SetPositionAndRotation(smoothedPosition, smoothedRotation);
        }

        private Vector3 ResolveDesiredPosition()
        {
            var rotation = ResolveRigRotation();
            var viewHeight = CameraFramingUtility.ResolveViewHeight(
                UseOrthographic,
                EffectiveOrthographicSize(),
                FieldOfView,
                CameraDistance);
            var centerPosition = CameraFramingUtility.ResolveAnchoredCenterPosition(
                followTarget.position,
                rotation,
                PlayerViewportAnchor,
                viewHeight,
                ResolveCameraAspect(),
                LookAtOffset,
                ForwardLookAhead,
                FollowOffset);
            var desiredPosition = CameraFramingUtility.ResolveCameraPosition(centerPosition, rotation, CameraDistance);
            return cameraBounds != null ? cameraBounds.ClampPosition(desiredPosition) : desiredPosition;
        }

        private Quaternion ResolveDesiredRotation(Vector3 cameraPosition)
        {
            return ResolveRigRotation();
        }

        private Quaternion ResolveRigRotation()
        {
            return CameraFramingUtility.ResolveRigRotation(PitchDegrees, YawDegrees);
        }

        private float EffectiveOrthographicSize()
        {
            return CameraFramingUtility.ResolveAspectSafeOrthographicSize(
                OrthographicSize,
                ResolveCameraAspect(),
                MinimumSupportedAspect,
                MaximumSupportedAspect);
        }

        private float ResolveCameraAspect()
        {
            return cachedCamera != null && cachedCamera.aspect > 0f ? cachedCamera.aspect : 9f / 19.5f;
        }
    }
}
