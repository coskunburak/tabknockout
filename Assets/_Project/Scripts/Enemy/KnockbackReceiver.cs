using System;
using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class KnockbackReceiver : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private EnemyConfig config;

        [Header("Fallback")]
        [SerializeField, Range(0f, 1f)] private float fallbackResistance;

        private Rigidbody cachedRigidbody;
        private EnemyHealth enemyHealth;
        private Vector3 knockbackVelocity;
        private float remainingDuration;

        public event Action<KnockbackData> OnKnockbackReceived;

        public bool IsKnockbackActive => remainingDuration > 0f;
        public float RemainingDuration => remainingDuration;
        public EnemyConfig Config => config;

        private float Resistance => config != null ? config.KnockbackResistance : fallbackResistance;

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
        }

        private void OnValidate()
        {
            fallbackResistance = Mathf.Clamp01(fallbackResistance);
        }

        private void FixedUpdate()
        {
            if (!IsKnockbackActive)
            {
                return;
            }

            var stepTime = Mathf.Min(Time.fixedDeltaTime, remainingDuration);
            remainingDuration = Mathf.Max(0f, remainingDuration - Time.fixedDeltaTime);

            var currentPosition = cachedRigidbody.position;
            var targetPosition = currentPosition + knockbackVelocity * stepTime;
            targetPosition.y = currentPosition.y;
            cachedRigidbody.MovePosition(targetPosition);
        }

        public void Initialize(EnemyConfig enemyConfig)
        {
            config = enemyConfig;
        }

        public void ApplyKnockback(HitContext hitContext)
        {
            if (hitContext == null)
            {
                return;
            }

            ApplyKnockback(hitContext.Knockback);
        }

        public void ApplyKnockback(KnockbackData knockbackData)
        {
            if (!knockbackData.HasKnockback || enemyHealth != null && !enemyHealth.IsAlive)
            {
                return;
            }

            if (config != null && !config.CanBeKnockedBack)
            {
                return;
            }

            var effectiveForce = CalculateEffectiveForce(knockbackData.Force, Resistance);
            if (effectiveForce <= 0f)
            {
                return;
            }

            knockbackVelocity = knockbackData.Direction.normalized * effectiveForce;
            remainingDuration = knockbackData.Duration;
            OnKnockbackReceived?.Invoke(knockbackData);
        }

        public static float CalculateEffectiveForce(float force, float resistance)
        {
            return Mathf.Max(0f, force) * (1f - Mathf.Clamp01(resistance));
        }
    }
}
