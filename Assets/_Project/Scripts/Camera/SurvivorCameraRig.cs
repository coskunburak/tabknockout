using UnityEngine;

namespace TapKnockout.Camera
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(50)]
    public sealed class SurvivorCameraRig : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform target;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private CameraBounds cameraBounds;

        [Header("Follow")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 32f, -6f);
        [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 0.35f, 0f);
        [SerializeField] private bool snapFollowToTarget = true;
        [SerializeField, Min(0f)] private float followSharpness;
        [SerializeField, Min(0f)] private float snapDistance = 0.08f;
        [SerializeField] private bool snapOnEnable = true;

        [Header("Readability")]
        [SerializeField] private bool lookAtTarget = true;
        [SerializeField] private bool lockRotationToOffset = true;
        [SerializeField] private bool clampToCameraBounds = true;

        [Header("Projection")]
        [SerializeField] private bool useOrthographic = true;
        [SerializeField, Range(30f, 70f)] private float fieldOfView = 38f;
        [SerializeField, Min(1f)] private float orthographicSize = 14f;
        [SerializeField, Min(0.01f)] private float nearClipPlane = 0.1f;
        [SerializeField, Min(1f)] private float farClipPlane = 220f;

        private UnityEngine.Camera cachedCamera;

        public Transform Target => target;
        public Vector3 Offset
        {
            get => offset;
            set => offset = value;
        }

        private void Reset()
        {
            cameraTransform = transform;
            cachedCamera = GetComponent<UnityEngine.Camera>();
            if (cachedCamera != null)
            {
                cachedCamera.tag = "MainCamera";
            }

            ApplyProjectionSettings();
        }

        private void Awake()
        {
            if (cameraTransform == null)
            {
                cameraTransform = transform;
            }

            cachedCamera = GetComponent<UnityEngine.Camera>();
            ApplyProjectionSettings();
        }

        private void OnValidate()
        {
            fieldOfView = Mathf.Clamp(fieldOfView, 30f, 70f);
            orthographicSize = Mathf.Max(1f, orthographicSize);
            nearClipPlane = Mathf.Max(0.01f, nearClipPlane);
            farClipPlane = Mathf.Max(nearClipPlane + 1f, farClipPlane);
            followSharpness = Mathf.Max(0f, followSharpness);
            snapDistance = Mathf.Max(0f, snapDistance);

            if (cameraTransform == null)
            {
                cameraTransform = transform;
            }

            cachedCamera = GetComponent<UnityEngine.Camera>();
            ApplyProjectionSettings();
        }

        private void OnEnable()
        {
            if (snapOnEnable)
            {
                SnapToTarget();
            }
        }

        private void LateUpdate()
        {
            if (target == null || cameraTransform == null)
            {
                return;
            }

            var desiredPosition = ResolveDesiredPosition();
            var sqrDistance = (cameraTransform.position - desiredPosition).sqrMagnitude;
            if (snapFollowToTarget || followSharpness <= 0f || sqrDistance <= snapDistance * snapDistance)
            {
                cameraTransform.position = desiredPosition;
            }
            else
            {
                var t = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
                cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, t);
            }

            ApplyProjectionSettings();
            if (lookAtTarget)
            {
                LookAtTarget();
            }
        }

        public void SetTarget(Transform followTarget, bool snap = true)
        {
            target = followTarget;
            if (snap)
            {
                SnapToTarget();
            }
        }

        public void SetCameraBounds(CameraBounds bounds)
        {
            cameraBounds = bounds;
        }

        public void SetComposition(Vector3 followOffset, Vector3 targetLookAtOffset, bool snap = true)
        {
            offset = followOffset;
            lookAtOffset = targetLookAtOffset;
            if (snap)
            {
                SnapToTarget();
            }
        }

        public void ConfigureProjection(
            bool orthographic,
            float orthoSize,
            float perspectiveFieldOfView,
            float nearClip,
            float farClip)
        {
            useOrthographic = orthographic;
            orthographicSize = Mathf.Max(1f, orthoSize);
            fieldOfView = Mathf.Clamp(perspectiveFieldOfView, 30f, 70f);
            nearClipPlane = Mathf.Max(0.01f, nearClip);
            farClipPlane = Mathf.Max(nearClipPlane + 1f, farClip);
            ApplyProjectionSettings();
        }

        public void ApplySurvivor2_5DPreset(Transform followTarget = null, bool snap = true)
        {
            if (followTarget != null)
            {
                target = followTarget;
            }

            followSharpness = 0f;
            snapDistance = 0.08f;
            snapFollowToTarget = true;
            lockRotationToOffset = true;
            SetComposition(new Vector3(0f, 32f, -6f), new Vector3(0f, 0.35f, 0f), false);
            ConfigureProjection(true, 14f, 38f, 0.1f, 220f);

            if (snap)
            {
                SnapToTarget();
            }
        }

        public void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            if (cameraTransform == null)
            {
                cameraTransform = transform;
            }

            cameraTransform.position = ResolveDesiredPosition();
            if (lookAtTarget)
            {
                LookAtTarget();
            }
        }

        private Vector3 ResolveDesiredPosition()
        {
            var desiredPosition = target.position + offset;
            if (clampToCameraBounds && cameraBounds != null)
            {
                desiredPosition = cameraBounds.ClampPosition(desiredPosition);
            }

            return desiredPosition;
        }

        private void LookAtTarget()
        {
            var direction = lockRotationToOffset
                ? lookAtOffset - offset
                : target.position + lookAtOffset - cameraTransform.position;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            cameraTransform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private void ApplyProjectionSettings()
        {
            if (cachedCamera == null)
            {
                cachedCamera = GetComponent<UnityEngine.Camera>();
            }

            if (cachedCamera == null)
            {
                return;
            }

            cachedCamera.orthographic = useOrthographic;
            cachedCamera.orthographicSize = orthographicSize;
            cachedCamera.fieldOfView = fieldOfView;
            cachedCamera.nearClipPlane = nearClipPlane;
            cachedCamera.farClipPlane = farClipPlane;
        }
    }
}
