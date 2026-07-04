using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EnemyMovement : MonoBehaviour, IPoolLifecycle
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
        private StatusEffectController statusEffectController;
        private Vector3 currentHorizontalVelocity;
        private Collider[] separationBuffer;

        public Transform Target => target;
        public bool HasTarget => HasUsableTarget;
        public bool CanMove => enabled &&
            (enemyHealth == null || enemyHealth.IsAlive) &&
            (knockbackReceiver == null || !knockbackReceiver.IsKnockbackActive) &&
            (statusEffectController == null || !statusEffectController.IsStunned);
        public Vector3 CurrentHorizontalVelocity => currentHorizontalVelocity;
        public float CurrentMoveSpeed => currentHorizontalVelocity.magnitude;
        public float NormalizedMoveSpeed => CalculateNormalizedMoveSpeed(CurrentMoveSpeed, MoveSpeed);
        public bool IsMoving => CanMove && CurrentMoveSpeed > 0.05f;
        public bool IsWithinStoppingDistanceToTarget =>
            target != null && IsWithinStoppingDistance(transform.position, target.position, StoppingDistance);
        private bool HasUsableTarget => target != null && target.gameObject.activeInHierarchy;

        private float MoveSpeed => config != null ? config.MoveSpeed : fallbackMoveSpeed;
        private float Acceleration => config != null ? config.Acceleration : fallbackAcceleration;
        private float RotationSpeed => config != null ? config.RotationSpeed : fallbackRotationSpeed;
        private float StoppingDistance => config != null ? config.StoppingDistance : fallbackStoppingDistance;
        private float StatusMoveSpeedMultiplier => statusEffectController != null ? statusEffectController.MoveSpeedMultiplier : 1f;
        private float EffectiveMoveSpeed => MoveSpeed * Mathf.Clamp01(StatusMoveSpeedMultiplier);

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
            statusEffectController = GetComponent<StatusEffectController>();
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
            if (!CanMove || !HasUsableTarget)
            {
                SettleAfterExternalDisplacement();
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

        public void ResetRuntimeState(bool clearTarget = true)
        {
            currentHorizontalVelocity = Vector3.zero;
            if (clearTarget)
            {
                target = null;
            }

            if (cachedRigidbody == null)
            {
                cachedRigidbody = GetComponent<Rigidbody>();
            }

            if (cachedRigidbody != null)
            {
                cachedRigidbody.linearVelocity = Vector3.zero;
                cachedRigidbody.angularVelocity = Vector3.zero;
            }
        }

        public void SettleAfterExternalDisplacement()
        {
            currentHorizontalVelocity = Vector3.zero;

            if (cachedRigidbody == null)
            {
                cachedRigidbody = GetComponent<Rigidbody>();
            }

            if (cachedRigidbody == null)
            {
                return;
            }

            cachedRigidbody.linearVelocity = Vector3.zero;
            cachedRigidbody.angularVelocity = Vector3.zero;
        }

        public void OnBeforeSpawnFromPool()
        {
            ResetRuntimeState();
        }

        public void OnSpawnedFromPool()
        {
        }

        public void OnBeforeDespawnToPool()
        {
            ResetRuntimeState();
        }

        public void ResetForPool()
        {
            ResetRuntimeState();
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
            var desiredDirection = CalculateDesiredDirection(direction, separation, separationStrength);

            var desiredVelocity = desiredDirection * EffectiveMoveSpeed;
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

        public static Vector3 CalculateDesiredDirection(Vector3 targetDirection, Vector3 separationOffset, float separationWeight)
        {
            targetDirection.y = 0f;
            if (targetDirection.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            var forward = targetDirection.normalized;
            separationOffset.y = 0f;
            if (separationOffset.sqrMagnitude > 1f)
            {
                separationOffset.Normalize();
            }

            if (separationOffset.sqrMagnitude > 0.0001f)
            {
                var backwardAmount = Vector3.Dot(separationOffset, -forward);
                if (backwardAmount > 0f)
                {
                    separationOffset += forward * backwardAmount;
                }
            }

            var desiredDirection = forward + separationOffset * Mathf.Max(0f, separationWeight);
            if (desiredDirection.sqrMagnitude <= 0.0001f || Vector3.Dot(desiredDirection, forward) <= 0f)
            {
                return forward;
            }

            return desiredDirection.normalized;
        }

        public static float CalculateNormalizedMoveSpeed(float currentSpeed, float maxSpeed)
        {
            var safeMaxSpeed = Mathf.Max(0f, maxSpeed);
            if (safeMaxSpeed <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(Mathf.Max(0f, currentSpeed) / safeMaxSpeed);
        }
    }
}
