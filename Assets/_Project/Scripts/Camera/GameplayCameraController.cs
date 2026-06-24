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
        [SerializeField] private Vector3 fallbackFollowOffset = new Vector3(0f, 12.5f, -8.75f);
        [SerializeField] private Vector3 fallbackLookAtOffset = new Vector3(0f, 0.8f, 4.25f);
        [SerializeField, Min(0.01f)] private float fallbackPositionSmoothTime = 0.1f;
        [SerializeField, Min(0.01f)] private float fallbackRotationSharpness = 16f;
        [SerializeField] private bool fallbackSnapOnEnable = true;

        [Header("Debug")]
        [SerializeField] private bool logSetupWarnings = true;

        private UnityEngine.Camera cachedCamera;
        private Vector3 followVelocity;
        private bool loggedMissingTarget;

        public Transform FollowTarget => followTarget;
        public CameraBounds Bounds => cameraBounds;
        public GameplayCameraConfig Config => config;

        private Vector3 FollowOffset => config != null ? config.FollowOffset : fallbackFollowOffset;
        private Vector3 LookAtOffset => config != null ? config.LookAtOffset : fallbackLookAtOffset;
        private float PositionSmoothTime => config != null ? config.PositionSmoothTime : fallbackPositionSmoothTime;
        private float RotationSharpness => config != null ? config.RotationSharpness : fallbackRotationSharpness;
        private bool SnapOnEnable => config != null ? config.SnapOnEnable : fallbackSnapOnEnable;

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
            fallbackPositionSmoothTime = Mathf.Max(0.01f, fallbackPositionSmoothTime);
            fallbackRotationSharpness = Mathf.Max(0.01f, fallbackRotationSharpness);

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

            if (config == null)
            {
                return;
            }

            cachedCamera.orthographic = config.UseOrthographic;
            cachedCamera.fieldOfView = config.FieldOfView;
            cachedCamera.orthographicSize = config.OrthographicSize;
            cachedCamera.nearClipPlane = config.NearClipPlane;
            cachedCamera.farClipPlane = config.FarClipPlane;
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
            var desiredPosition = CameraFramingUtility.ResolveFollowPosition(followTarget.position, FollowOffset);
            return cameraBounds != null ? cameraBounds.ClampPosition(desiredPosition) : desiredPosition;
        }

        private Quaternion ResolveDesiredRotation(Vector3 cameraPosition)
        {
            var lookAtPosition = CameraFramingUtility.ResolveLookAtPosition(followTarget.position, LookAtOffset);
            return CameraFramingUtility.ResolveLookRotation(cameraPosition, lookAtPosition, transform.rotation);
        }
    }
}
