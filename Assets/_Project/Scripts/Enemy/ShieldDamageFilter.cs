using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    public sealed class ShieldDamageFilter : MonoBehaviour, IEnemyRuntimeConfigReceiver
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField, Range(0f, 180f)] private float fallbackBlockAngle = 120f;
        [SerializeField, Range(0f, 1f)] private float fallbackDamageReduction = 0.65f;
        [SerializeField, Range(0f, 1f)] private float dashReductionMultiplier = 0.5f;

        private float BlockAngle => config != null ? config.ShieldBlockAngle : fallbackBlockAngle;
        private float DamageReduction => config != null ? config.ShieldDamageReduction : fallbackDamageReduction;

        public void Initialize(EnemyConfig enemyConfig, Transform target)
        {
            config = enemyConfig;
        }

        public bool ApplyToHit(HitContext hitContext)
        {
            if (hitContext == null || hitContext.DamageAmount <= 0f)
            {
                return false;
            }

            if (!IsFrontalHit(transform.forward, hitContext.HitDirection, BlockAngle))
            {
                return false;
            }

            var reduction = hitContext.IsDashHit ? DamageReduction * dashReductionMultiplier : DamageReduction;
            hitContext.DamageAmount = CalculateDamageAfterShield(hitContext.DamageAmount, reduction);
            return true;
        }

        public static bool IsFrontalHit(Vector3 shieldForward, Vector3 incomingDirectionToTarget, float blockAngle)
        {
            shieldForward.y = 0f;
            incomingDirectionToTarget.y = 0f;
            if (shieldForward.sqrMagnitude <= 0.0001f || incomingDirectionToTarget.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            var halfAngle = Mathf.Clamp(blockAngle, 0f, 180f) * 0.5f;
            var dot = Vector3.Dot(shieldForward.normalized, -incomingDirectionToTarget.normalized);
            var threshold = Mathf.Cos(halfAngle * Mathf.Deg2Rad);
            return dot >= threshold;
        }

        public static float CalculateDamageAfterShield(float damage, float damageReduction)
        {
            return Mathf.Max(0f, damage) * (1f - Mathf.Clamp01(damageReduction));
        }
    }
}
