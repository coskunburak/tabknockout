using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EnemyMovement : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private EnemyConfig config;

        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Fallback")]
        [SerializeField, Min(0f)] private float fallbackMoveSpeed = 2.2f;
        [SerializeField, Min(0f)] private float fallbackAcceleration = 18f;
        [SerializeField, Min(0f)] private float fallbackRotationSpeed = 720f;
        [SerializeField, Min(0f)] private float fallbackStoppingDistance = 1.1f;

        [Header("Separation")]
        [SerializeField] private bool enableSeparation = true;
        [SerializeField, Min(0f)] private float separationRadius = 0.75f;
        [SerializeField, Min(0f)] private float separationStrength = 0.9f;
        [SerializeField] private LayerMask separationLayers = ~0;
        [SerializeField, Range(2, 16)] private int separationBufferSize = 8;

        private Rigidbody cachedRigidbody;
        private EnemyHealth enemyHealth;
        private KnockbackReceiver knockbackReceiver;
        private Vector3 currentHorizontalVelocity;
        private Collider[] separationBuffer;

        public Transform Target => target;
        public bool HasTarget => target != null;
        public bool CanMove => enabled &&
            (enemyHealth == null || enemyHealth.IsAlive) &&
            (knockbackReceiver == null || !knockbackReceiver.IsKnockbackActive);

        private float MoveSpeed => config != null ? config.MoveSpeed : fallbackMoveSpeed;
        private float Acceleration => config != null ? config.Acceleration : fallbackAcceleration;
        private float RotationSpeed => config != null ? config.RotationSpeed : fallbackRotationSpeed;
        private float StoppingDistance => config != null ? config.StoppingDistance : fallbackStoppingDistance;

        private void Reset()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            cachedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            cachedRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            cachedRigidbody.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        private void Awake()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            enemyHealth = GetComponent<EnemyHealth>();
            knockbackReceiver = GetComponent<KnockbackReceiver>();
            EnsureSeparationBuffer();
        }

        private void OnValidate()
        {
            fallbackMoveSpeed = Mathf.Max(0f, fallbackMoveSpeed);
            fallbackAcceleration = Mathf.Max(0f, fallbackAcceleration);
            fallbackRotationSpeed = Mathf.Max(0f, fallbackRotationSpeed);
            fallbackStoppingDistance = Mathf.Max(0f, fallbackStoppingDistance);
            separationRadius = Mathf.Max(0f, separationRadius);
            separationStrength = Mathf.Max(0f, separationStrength);
            separationBufferSize = Mathf.Clamp(separationBufferSize, 2, 16);
        }

        private void FixedUpdate()
        {
            if (!CanMove || target == null)
            {
                currentHorizontalVelocity = Vector3.zero;
                return;
            }

            MoveTowardTarget(Time.fixedDeltaTime);
        }

        public void Initialize(EnemyConfig enemyConfig, Transform movementTarget)
        {
            config = enemyConfig;
            target = movementTarget;
        }

        public void SetTarget(Transform movementTarget)
        {
            target = movementTarget;
        }

        private void MoveTowardTarget(float deltaTime)
        {
            var currentPosition = cachedRigidbody.position;
            var toTarget = target.position - currentPosition;
            toTarget.y = 0f;

            if (IsWithinStoppingDistance(currentPosition, target.position, StoppingDistance))
            {
                currentHorizontalVelocity = Vector3.MoveTowards(
                    currentHorizontalVelocity,
                    Vector3.zero,
                    Acceleration * deltaTime);
                return;
            }

            var direction = toTarget.normalized;
            var separation = ResolveSeparationOffset(currentPosition);
            var desiredDirection = direction + separation * separationStrength;
            if (desiredDirection.sqrMagnitude > 1f)
            {
                desiredDirection.Normalize();
            }

            if (desiredDirection.sqrMagnitude <= 0.0001f)
            {
                desiredDirection = direction;
            }

            var desiredVelocity = desiredDirection * MoveSpeed;
            currentHorizontalVelocity = Vector3.MoveTowards(
                currentHorizontalVelocity,
                desiredVelocity,
                Acceleration * deltaTime);

            var targetPosition = currentPosition + currentHorizontalVelocity * deltaTime;
            targetPosition.y = currentPosition.y;
            cachedRigidbody.MovePosition(targetPosition);
            RotateToward(desiredDirection, deltaTime);
        }

        private Vector3 ResolveSeparationOffset(Vector3 currentPosition)
        {
            if (!enableSeparation || separationRadius <= 0f || separationStrength <= 0f || separationLayers.value == 0)
            {
                return Vector3.zero;
            }

            EnsureSeparationBuffer();
            var count = Physics.OverlapSphereNonAlloc(
                currentPosition,
                separationRadius,
                separationBuffer,
                separationLayers,
                QueryTriggerInteraction.Ignore);

            var offset = Vector3.zero;
            for (var i = 0; i < count; i++)
            {
                var candidate = separationBuffer[i];
                if (candidate == null || candidate.transform == transform || candidate.transform.IsChildOf(transform))
                {
                    continue;
                }

                var otherMovement = candidate.GetComponentInParent<EnemyMovement>();
                if (otherMovement == null || otherMovement == this)
                {
                    continue;
                }

                offset += CalculateSeparationOffset(currentPosition, otherMovement.transform.position, separationRadius);
            }

            return offset.sqrMagnitude > 1f ? offset.normalized : offset;
        }

        private void EnsureSeparationBuffer()
        {
            if (separationBuffer == null || separationBuffer.Length != separationBufferSize)
            {
                separationBuffer = new Collider[separationBufferSize];
            }
        }

        private void RotateToward(Vector3 direction, float deltaTime)
        {
            if (direction.sqrMagnitude <= 0f || RotationSpeed <= 0f)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            cachedRigidbody.MoveRotation(Quaternion.RotateTowards(
                cachedRigidbody.rotation,
                targetRotation,
                RotationSpeed * deltaTime));
        }

        public static bool IsWithinStoppingDistance(Vector3 currentPosition, Vector3 targetPosition, float stoppingDistance)
        {
            var offset = targetPosition - currentPosition;
            offset.y = 0f;
            return offset.sqrMagnitude <= Mathf.Max(0f, stoppingDistance) * Mathf.Max(0f, stoppingDistance);
        }

        public static Vector3 CalculateSeparationOffset(Vector3 currentPosition, Vector3 neighborPosition, float radius)
        {
            var safeRadius = Mathf.Max(0f, radius);
            if (safeRadius <= 0f)
            {
                return Vector3.zero;
            }

            var offset = currentPosition - neighborPosition;
            offset.y = 0f;
            var distance = offset.magnitude;
            if (distance <= 0.0001f || distance >= safeRadius)
            {
                return Vector3.zero;
            }

            var strength = 1f - distance / safeRadius;
            return offset.normalized * strength;
        }
    }
}
