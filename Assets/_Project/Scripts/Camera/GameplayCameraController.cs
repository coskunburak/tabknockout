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
        [SerializeField] private Transform targetOverride;
        [SerializeField] private CameraBounds cameraBounds;

        [Header("Fallback Values")]
        [SerializeField, Range(35f, 75f)] private float fallbackPitchDegrees = 52f;
        [SerializeField, Range(-35f, 35f)] private float fallbackYawDegrees;
        [SerializeField, Min(1f)] private float fallbackCameraDistance = 18f;
        [SerializeField] private Vector3 fallbackFollowOffset = Vector3.zero;
        [SerializeField] private Vector3 fallbackLookAtOffset = new Vector3(0f, 0.65f, 0f);
        [SerializeField] private Vector2 fallbackPlayerViewportAnchor = new Vector2(0.5f, 0.49f);
        [SerializeField, Min(0f)] private float fallbackForwardLookAhead = 0.65f;
        [SerializeField] private bool fallbackEnableMovementLookAhead = true;
        [SerializeField, Min(0f)] private float fallbackMovementLookAheadStrength = 0.9f;
        [SerializeField, Min(0f)] private float fallbackMaxMovementLookAhead = 1.4f;
        [SerializeField] private bool fallbackEnableDashLookAhead = true;
        [SerializeField, Min(0f)] private float fallbackDashLookAheadMultiplier = 1.45f;
        [SerializeField, Range(0.03f, 0.4f)] private float fallbackDashLookAheadDuration = 0.14f;
        [SerializeField, Min(0.01f)] private float fallbackPositionSmoothTime = 0.08f;
        [SerializeField, Min(0.01f)] private float fallbackRotationSharpness = 18f;
        [SerializeField] private bool fallbackSnapOnEnable = true;
        [SerializeField] private bool fallbackUseOrthographic = true;
        [SerializeField, Range(30f, 70f)] private float fallbackFieldOfView = 42f;
        [SerializeField, Min(1f)] private float fallbackOrthographicSize = 11.25f;
        [SerializeField, Min(0.01f)] private float fallbackNearClipPlane = 0.1f;
        [SerializeField, Min(1f)] private float fallbackFarClipPlane = 220f;
        [SerializeField, Range(0.4f, 3f)] private float fallbackMinimumSupportedAspect = 1.33f;
        [SerializeField, Range(0.4f, 3f)] private float fallbackMaximumSupportedAspect = 2.4f;

        [Header("Debug")]
        [SerializeField] private bool logSetupWarnings = true;

        private UnityEngine.Camera cachedCamera;
        private Vector3 followVelocity;
        private Vector3 lastTargetPosition;
        private Vector3 movementLookAhead;
        private Vector3 dashLookAheadDirection = Vector3.forward;
        private float dashLookAheadRemaining;
        private float dashLookAheadDuration;
        private float dashLookAheadMultiplier = 1f;
        private bool loggedMissingTarget;
        private bool hasLastTargetPosition;

        public Transform FollowTarget => followTarget;
        public Transform TargetOverride => targetOverride;
        public Transform ActiveFollowTarget => ResolveActiveTarget();
        public CameraBounds Bounds => cameraBounds;
        public GameplayCameraConfig Config => config;

        private float PitchDegrees => config != null ? config.PitchDegrees : fallbackPitchDegrees;
        private float YawDegrees => config != null ? config.YawDegrees : fallbackYawDegrees;
        private float CameraDistance => config != null ? config.CameraDistance : fallbackCameraDistance;
        private Vector3 FollowOffset => config != null ? config.FollowOffset : fallbackFollowOffset;
        private Vector3 LookAtOffset => config != null ? config.LookAtOffset : fallbackLookAtOffset;
        private Vector2 PlayerViewportAnchor => config != null ? config.PlayerViewportAnchor : fallbackPlayerViewportAnchor;
        private float ForwardLookAhead => config != null ? config.ForwardLookAhead : fallbackForwardLookAhead;
        private bool EnableMovementLookAhead => config != null ? config.EnableMovementLookAhead : fallbackEnableMovementLookAhead;
        private float MovementLookAheadStrength => config != null ? config.MovementLookAheadStrength : fallbackMovementLookAheadStrength;
        private float MaxMovementLookAhead => config != null ? config.MaxMovementLookAhead : fallbackMaxMovementLookAhead;
        private bool EnableDashLookAhead => config != null ? config.EnableDashLookAhead : fallbackEnableDashLookAhead;
        private float DashLookAheadMultiplier => config != null ? config.DashLookAheadMultiplier : fallbackDashLookAheadMultiplier;
        private float DashLookAheadDuration => config != null ? config.DashLookAheadDuration : fallbackDashLookAheadDuration;
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
                Mathf.Clamp(fallbackPlayerViewportAnchor.y, 0.35f, 0.6f));
            fallbackForwardLookAhead = Mathf.Max(0f, fallbackForwardLookAhead);
            fallbackMovementLookAheadStrength = Mathf.Max(0f, fallbackMovementLookAheadStrength);
            fallbackMaxMovementLookAhead = Mathf.Max(0f, fallbackMaxMovementLookAhead);
            fallbackDashLookAheadMultiplier = Mathf.Max(0f, fallbackDashLookAheadMultiplier);
            fallbackDashLookAheadDuration = Mathf.Clamp(fallbackDashLookAheadDuration, 0.03f, 0.4f);
            fallbackPositionSmoothTime = Mathf.Max(0.01f, fallbackPositionSmoothTime);
            fallbackRotationSharpness = Mathf.Max(0.01f, fallbackRotationSharpness);
            fallbackFieldOfView = Mathf.Clamp(fallbackFieldOfView, 30f, 70f);
            fallbackOrthographicSize = Mathf.Max(1f, fallbackOrthographicSize);
            fallbackNearClipPlane = Mathf.Max(0.01f, fallbackNearClipPlane);
            fallbackFarClipPlane = Mathf.Max(fallbackNearClipPlane + 1f, fallbackFarClipPlane);
            fallbackMinimumSupportedAspect = Mathf.Clamp(fallbackMinimumSupportedAspect, 0.4f, fallbackMaximumSupportedAspect);
            fallbackMaximumSupportedAspect = Mathf.Clamp(fallbackMaximumSupportedAspect, fallbackMinimumSupportedAspect, 3f);

            if (cachedCamera == null)
            {
                cachedCamera = GetComponent<UnityEngine.Camera>();
            }

            ApplyProjectionSettings();
        }

        private void LateUpdate()
        {
            var activeTarget = ResolveActiveTarget();
            if (activeTarget == null)
            {
                if (logSetupWarnings && !loggedMissingTarget)
                {
                    loggedMissingTarget = true;
                    Debug.LogWarning($"{nameof(GameplayCameraController)} on {name} has no follow target assigned.", this);
                }

                return;
            }

            ApplyProjectionSettings();
            Follow(activeTarget, Time.deltaTime);
        }

        public void SetFollowTarget(Transform target, bool snapImmediately)
        {
            followTarget = target;
            loggedMissingTarget = false;
            ResetLookAheadState(target);

            if (snapImmediately)
            {
                SnapToTarget();
            }
        }

        public void SetTargetOverride(Transform target, bool snapImmediately)
        {
            targetOverride = target;
            loggedMissingTarget = false;
            ResetLookAheadState(target);

            if (snapImmediately)
            {
                SnapToTarget();
            }
        }

        public void ClearTargetOverride(bool snapToFollowTarget)
        {
            targetOverride = null;
            ResetLookAheadState(followTarget);

            if (snapToFollowTarget)
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
            var activeTarget = ResolveActiveTarget();
            if (activeTarget == null)
            {
                return;
            }

            ResetLookAheadState(activeTarget);
            var targetPosition = ResolveDesiredPosition(activeTarget, 0f);
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

        public void RequestDashLookAhead(Vector3 direction, float duration, float multiplier = 0f)
        {
            if (!EnableDashLookAhead || MaxMovementLookAhead <= 0f)
            {
                return;
            }

            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            dashLookAheadDirection = direction.normalized;
            dashLookAheadDuration = Mathf.Max(0.03f, Mathf.Min(Mathf.Max(DashLookAheadDuration, duration), 0.4f));
            dashLookAheadRemaining = dashLookAheadDuration;
            dashLookAheadMultiplier = multiplier > 0f ? multiplier : DashLookAheadMultiplier;
        }

        private void Follow(Transform activeTarget, float deltaTime)
        {
            var desiredPosition = ResolveDesiredPosition(activeTarget, deltaTime);
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

        private Vector3 ResolveDesiredPosition(Transform activeTarget, float deltaTime)
        {
            var rotation = ResolveRigRotation();
            var viewHeight = CameraFramingUtility.ResolveViewHeight(
                UseOrthographic,
                EffectiveOrthographicSize(),
                FieldOfView,
                CameraDistance);
            var centerPosition = CameraFramingUtility.ResolveAnchoredCenterPosition(
                activeTarget.position,
                rotation,
                PlayerViewportAnchor,
                viewHeight,
                ResolveCameraAspect(),
                LookAtOffset,
                ForwardLookAhead,
                FollowOffset + ResolveMovementLookAhead(activeTarget, deltaTime));
            var desiredPosition = CameraFramingUtility.ResolveCameraPosition(centerPosition, rotation, CameraDistance);
            return cameraBounds != null ? cameraBounds.ClampPosition(desiredPosition) : desiredPosition;
        }

        private Vector3 ResolveMovementLookAhead(Transform activeTarget, float deltaTime)
        {
            if (!EnableMovementLookAhead || activeTarget == null || MovementLookAheadStrength <= 0f || MaxMovementLookAhead <= 0f)
            {
                movementLookAhead = Vector3.zero;
                ResetLookAheadState(activeTarget);
                return Vector3.zero;
            }

            var targetPosition = activeTarget.position;
            var dashLookAhead = ResolveDashLookAhead(deltaTime);
            if (!hasLastTargetPosition || deltaTime <= 0f)
            {
                ResetLookAheadState(activeTarget);
                return movementLookAhead + dashLookAhead;
            }

            var planarDelta = targetPosition - lastTargetPosition;
            planarDelta.y = 0f;
            lastTargetPosition = targetPosition;

            var desired = Vector3.zero;
            if (planarDelta.sqrMagnitude > 0.000001f)
            {
                desired = planarDelta.normalized * Mathf.Min(MaxMovementLookAhead, planarDelta.magnitude / deltaTime * 0.05f * MovementLookAheadStrength);
            }

            movementLookAhead = Vector3.Lerp(
                movementLookAhead,
                desired,
                CameraFramingUtility.ResolveRotationInterpolation(12f, deltaTime));
            return movementLookAhead + dashLookAhead;
        }

        private Vector3 ResolveDashLookAhead(float deltaTime)
        {
            if (!EnableDashLookAhead || dashLookAheadRemaining <= 0f || dashLookAheadDuration <= 0f)
            {
                dashLookAheadRemaining = 0f;
                return Vector3.zero;
            }

            dashLookAheadRemaining = Mathf.Max(0f, dashLookAheadRemaining - Mathf.Max(0f, deltaTime));
            var normalized = Mathf.Clamp01(dashLookAheadRemaining / dashLookAheadDuration);
            var shaped = normalized * normalized * (3f - 2f * normalized);
            return dashLookAheadDirection * (MaxMovementLookAhead * dashLookAheadMultiplier * shaped);
        }

        private Transform ResolveActiveTarget()
        {
            return targetOverride != null ? targetOverride : followTarget;
        }

        private void ResetLookAheadState(Transform activeTarget)
        {
            if (activeTarget != null)
            {
                lastTargetPosition = activeTarget.position;
                hasLastTargetPosition = true;
            }
            else
            {
                lastTargetPosition = Vector3.zero;
                hasLastTargetPosition = false;
            }

            movementLookAhead = Vector3.zero;
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
