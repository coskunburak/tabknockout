using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Enemy
{
    /// <summary>
    /// Added to area-zone projectiles (e.g. Cthulhu slime orb).
    /// When the projectile is deactivated (hit or expired), this component
    /// spawns an EnemyAreaZone at the projectile's final position.
    ///
    /// Reads parameters from an EnemyAttackConfig assigned at spawn time.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProjectileAreaZoneSpawner : MonoBehaviour
    {
        private GameObject source;
        private EnemyAttackConfig config;
        private bool initialized;

        public void Initialize(GameObject zoneSource, EnemyAttackConfig attackConfig)
        {
            source = zoneSource;
            config = attackConfig;
            initialized = true;
        }

        private void OnDisable()
        {
            if (!initialized || config == null || config.AreaZonePrefab == null)
            {
                initialized = false;
                return;
            }

            // Spawn area zone at current world position
            var zonePos = transform.position;
            zonePos.y = source != null ? source.transform.position.y : 0f;

            var zoneGo = Instantiate(config.AreaZonePrefab, zonePos, Quaternion.identity);
            var zone = zoneGo.GetComponent<EnemyAreaZone>();
            if (zone == null)
            {
                zone = zoneGo.AddComponent<EnemyAreaZone>();
            }

            zone.Initialize(
                source,
                config.AreaZoneRadius,
                config.AreaZoneDuration,
                config.AreaZoneTickInterval,
                config.AreaZoneTickDamage,
                config.StatusEffectType,
                config.StatusEffectDuration,
                config.StatusEffectSlowMultiplier,
                config.HitLayerMask);

            initialized = false;
        }
    }
}
