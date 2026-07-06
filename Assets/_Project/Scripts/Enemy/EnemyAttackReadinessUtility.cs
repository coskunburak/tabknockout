using System;
using UnityEngine;

namespace TapKnockout.Enemy
{
    public static class EnemyAttackReadinessUtility
    {
        public static bool IsDistinctAttackSystemReady(
            EnemyAttackConfig[] configs,
            out string reason,
            bool requireFallbackVisuals = true)
        {
            if (configs == null || configs.Length == 0)
            {
                reason = "no attack configs assigned";
                return false;
            }

            for (var i = 0; i < configs.Length; i++)
            {
                if (!IsConfigGameplayReady(configs[i], out reason, requireFallbackVisuals))
                {
                    reason = $"attackConfigs[{i}]: {reason}";
                    return false;
                }
            }

            reason = "ready";
            return true;
        }

        public static bool IsConfigGameplayReady(
            EnemyAttackConfig config,
            out string reason,
            bool requireFallbackVisuals = true)
        {
            if (config == null)
            {
                reason = "config is null";
                return false;
            }

            if (config.AttackType == EnemyDistinctAttackType.None)
            {
                reason = "attack type is None";
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.AttackId))
            {
                reason = "attack id is empty";
                return false;
            }

            if (config.TriggerRange <= 0f)
            {
                reason = "trigger range is zero";
                return false;
            }

            if (config.Cooldown <= 0f)
            {
                reason = "cooldown is zero";
                return false;
            }

            if (config.WindupTime <= 0f || config.ActiveTime <= 0f || config.RecoveryTime < 0f)
            {
                reason = "windup, active, or recovery timing is invalid";
                return false;
            }

            if (config.HitLayerMask.value == 0)
            {
                reason = "hit layer mask is zero";
                return false;
            }

            if (config.Damage <= 0f && config.AreaZoneTickDamage <= 0f)
            {
                reason = "damage and area tick damage are zero";
                return false;
            }

            if (config.NeedsProjectile && config.ProjectilePrefab == null)
            {
                reason = "projectile prefab is missing";
                return false;
            }

            if (config.NeedsAreaZone && config.AreaZonePrefab == null)
            {
                reason = "area zone prefab is missing";
                return false;
            }

            if (config.AreaZonePrefab != null)
            {
                if (config.AreaZoneRadius <= 0f || config.AreaZoneDuration <= 0f || config.AreaZoneTickInterval <= 0f)
                {
                    reason = "area zone timing or radius is invalid";
                    return false;
                }
            }

            if (config.AttackType == EnemyDistinctAttackType.Beam &&
                (config.BeamLength <= 0f || config.BeamWidth <= 0f))
            {
                reason = "beam length or width is invalid";
                return false;
            }

            if (requireFallbackVisuals)
            {
                if (config.TelegraphPrefab == null)
                {
                    reason = "telegraph prefab is missing";
                    return false;
                }

                if (config.ActiveVfxPrefab == null)
                {
                    reason = "active VFX prefab is missing";
                    return false;
                }

                if (config.ImpactVfxPrefab == null)
                {
                    reason = "impact VFX prefab is missing";
                    return false;
                }
            }

            reason = "ready";
            return true;
        }

        public static bool RequiresProjectile(EnemyAttackConfig config)
        {
            return config != null && config.NeedsProjectile;
        }

        public static bool RequiresAreaZone(EnemyAttackConfig config)
        {
            return config != null && config.NeedsAreaZone;
        }

        public static string DescribeReferences(EnemyAttackConfig config)
        {
            if (config == null)
            {
                return "config=null";
            }

            return string.Join(", ", new[]
            {
                $"projectile={Describe(config.ProjectilePrefab)}",
                $"area={Describe(config.AreaZonePrefab)}",
                $"telegraph={Describe(config.TelegraphPrefab)}",
                $"activeVfx={Describe(config.ActiveVfxPrefab)}",
                $"impactVfx={Describe(config.ImpactVfxPrefab)}"
            });
        }

        private static string Describe(UnityEngine.Object obj)
        {
            return obj != null ? obj.name : "null";
        }
    }
}
