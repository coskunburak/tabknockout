using UnityEngine;

namespace TapKnockout.Combat
{
    public static class DashImpactDamageCalculator
    {
        private const float Epsilon = 0.0001f;

        public static float CalculateDamage(
            float baseDamage,
            float damageMultiplier,
            float currentDashSpeed,
            float referenceDashSpeed,
            float speedDamageScale,
            float minSpeedMultiplier,
            float maxSpeedMultiplier,
            float conditionalMultiplier = 1f)
        {
            var speedMultiplier = CalculateSpeedMultiplier(
                currentDashSpeed,
                referenceDashSpeed,
                speedDamageScale,
                minSpeedMultiplier,
                maxSpeedMultiplier);

            return Mathf.Max(0f, baseDamage) *
                Mathf.Max(0f, damageMultiplier) *
                speedMultiplier *
                Mathf.Max(0f, conditionalMultiplier);
        }

        public static float CalculateSpeedMultiplier(
            float currentDashSpeed,
            float referenceDashSpeed,
            float speedDamageScale,
            float minSpeedMultiplier,
            float maxSpeedMultiplier)
        {
            if (referenceDashSpeed <= Epsilon || speedDamageScale <= 0f)
            {
                return 1f;
            }

            var safeMin = Mathf.Max(0f, minSpeedMultiplier);
            var safeMax = Mathf.Max(safeMin, maxSpeedMultiplier);
            var speedRatio = Mathf.Max(0f, currentDashSpeed) / referenceDashSpeed;
            var scaledMultiplier = 1f + (speedRatio - 1f) * speedDamageScale;
            return Mathf.Clamp(scaledMultiplier, safeMin, safeMax);
        }
    }
}
