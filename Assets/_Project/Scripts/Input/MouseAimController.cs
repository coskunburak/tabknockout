using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TapKnockout.Input
{
    [DefaultExecutionOrder(80)]
    [DisallowMultipleComponent]
    public sealed class MouseAimController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UnityEngine.Camera aimCamera;
        [SerializeField] private Transform aimOrigin;
        [SerializeField] private Transform facingTarget;

        [Header("Projection")]
        [SerializeField] private bool preferStableGroundPlane = true;
        [SerializeField] private bool usePhysicsRaycast;
        [SerializeField] private LayerMask groundLayers = ~0;
        [SerializeField, Min(1f)] private float maxRaycastDistance = 500f;
        [SerializeField] private float fallbackGroundPlaneY;
        [SerializeField, Min(0f)] private float minAimDirectionDistance = 0.35f;

        [Header("Facing")]
        [SerializeField] private bool rotateFacingTarget = true;
        [SerializeField] private bool rotateRigidbodyInFixedUpdate = true;
        [SerializeField, Min(0f)] private float rotationSpeed = 1080f;

        public Vector3 AimPointWorld { get; private set; }
        public Vector3 AimDirectionWorld { get; private set; } = Vector3.forward;
        public bool HasAimPoint { get; private set; }
        public UnityEngine.Camera AimCamera => aimCamera;
        public LayerMask GroundLayers => groundLayers;
        public bool PreferStableGroundPlane => preferStableGroundPlane;
        public bool UsePhysicsRaycast => usePhysicsRaycast;

        private Rigidbody cachedFacingRigidbody;
        private Transform cachedFacingTargetForRigidbody;
        private Quaternion pendingFacingRotation;
        private bool hasPendingFacingRotation;

        private void Reset()
        {
            aimOrigin = transform;
            facingTarget = transform;
        }

        private void Awake()
        {
            ResolveReferences();
            ResolveFacingRigidbody();
            RefreshAim();
        }

        private void OnValidate()
        {
            maxRaycastDistance = Mathf.Max(1f, maxRaycastDistance);
            minAimDirectionDistance = Mathf.Max(0f, minAimDirectionDistance);
            rotationSpeed = Mathf.Max(0f, rotationSpeed);
        }

        private void FixedUpdate()
        {
            ApplyRigidbodyFacingRotation();
        }

        private void LateUpdate()
        {
            RefreshAim();
            RotateFacingTarget();
        }

        public void SetAimCamera(UnityEngine.Camera camera)
        {
            aimCamera = camera;
        }

        public void SetFacingTarget(Transform target)
        {
            facingTarget = target != null ? target : transform;
            ResolveFacingRigidbody();
        }

        public bool TryGetAimDirection(out Vector3 direction)
        {
            direction = AimDirectionWorld;
            return HasAimPoint && direction.sqrMagnitude > 0.0001f;
        }

        public bool TryGetAimPoint(out Vector3 point)
        {
            point = AimPointWorld;
            return HasAimPoint;
        }

        public void RefreshAim()
        {
            ResolveReferences();

            var origin = aimOrigin != null ? aimOrigin.position : transform.position;
            if (aimCamera == null)
            {
                AimPointWorld = origin + transform.forward;
                AimDirectionWorld = FlattenOrForward(transform.forward);
                HasAimPoint = false;
                return;
            }

            var ray = aimCamera.ScreenPointToRay(ReadMousePosition());
            HasAimPoint = TryProjectRay(ray, out var hitPoint);
            AimPointWorld = hitPoint;

            AimDirectionWorld = ResolveAimDirection(origin, hitPoint);
        }

        private void ResolveReferences()
        {
            if (aimCamera == null)
            {
                aimCamera = UnityEngine.Camera.main;
            }

            if (aimOrigin == null)
            {
                aimOrigin = transform;
            }

            if (facingTarget == null)
            {
                facingTarget = transform;
            }
        }

        private void ResolveFacingRigidbody()
        {
            if (facingTarget == cachedFacingTargetForRigidbody)
            {
                return;
            }

            cachedFacingTargetForRigidbody = facingTarget;
            cachedFacingRigidbody = facingTarget != null ? facingTarget.GetComponent<Rigidbody>() : null;
            hasPendingFacingRotation = false;
        }

        private bool TryProjectRay(Ray ray, out Vector3 hitPoint)
        {
            if (preferStableGroundPlane && TryProjectGroundPlane(ray, out hitPoint))
            {
                return true;
            }

            if (usePhysicsRaycast &&
                Physics.Raycast(ray, out var hit, maxRaycastDistance, groundLayers, QueryTriggerInteraction.Ignore))
            {
                hitPoint = hit.point;
                return true;
            }

            if (TryProjectGroundPlane(ray, out hitPoint))
            {
                return true;
            }

            hitPoint = ray.origin + ray.direction * maxRaycastDistance;
            return false;
        }

        private bool TryProjectGroundPlane(Ray ray, out Vector3 hitPoint)
        {
            var plane = new Plane(Vector3.up, new Vector3(0f, fallbackGroundPlaneY, 0f));
            if (plane.Raycast(ray, out var enter))
            {
                hitPoint = ray.GetPoint(enter);
                return true;
            }

            hitPoint = default;
            return false;
        }

        private void RotateFacingTarget()
        {
            if (!rotateFacingTarget || facingTarget == null || AimDirectionWorld.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            ResolveFacingRigidbody();
            var targetRotation = Quaternion.LookRotation(AimDirectionWorld, Vector3.up);
            if (rotateRigidbodyInFixedUpdate && cachedFacingRigidbody != null)
            {
                pendingFacingRotation = targetRotation;
                hasPendingFacingRotation = true;
                return;
            }

            facingTarget.rotation = rotationSpeed <= 0f
                ? targetRotation
                : Quaternion.RotateTowards(facingTarget.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private void ApplyRigidbodyFacingRotation()
        {
            if (!rotateFacingTarget || !rotateRigidbodyInFixedUpdate || !hasPendingFacingRotation)
            {
                return;
            }

            ResolveFacingRigidbody();
            if (cachedFacingRigidbody == null)
            {
                return;
            }

            var newRotation = rotationSpeed <= 0f
                ? pendingFacingRotation
                : Quaternion.RotateTowards(cachedFacingRigidbody.rotation, pendingFacingRotation, rotationSpeed * Time.fixedDeltaTime);
            cachedFacingRigidbody.MoveRotation(newRotation);
        }

        private static Vector3 ReadMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.position.ReadValue();
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.mousePosition;
#else
            return Vector3.zero;
#endif
        }

        private Vector3 ResolveAimDirection(Vector3 origin, Vector3 hitPoint)
        {
            var direction = hitPoint - origin;
            direction.y = 0f;

            var minDistance = Mathf.Max(0f, minAimDirectionDistance);
            if (direction.sqrMagnitude >= minDistance * minDistance)
            {
                return direction.normalized;
            }

            return AimDirectionWorld.sqrMagnitude > 0.0001f
                ? AimDirectionWorld
                : FlattenOrForward(transform.forward);
        }

        private static Vector3 FlattenOrForward(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }
    }
}
