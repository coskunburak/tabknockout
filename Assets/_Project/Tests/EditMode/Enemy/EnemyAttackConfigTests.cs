// EnemyAttackConfigTests.cs
// EditMode tests for EnemyAttackConfig ScriptableObjects and EnemyDistinctAttackController.
// Tests asset values, attack type correctness, and system constraints.

using NUnit.Framework;
using TapKnockout.Combat;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Enemy.Tests
{
    /// <summary>
    /// Tests for EnemyAttackConfig ScriptableObject values and EnemyDistinctAttackController
    /// data validation. Runs as EditMode tests (no scene required).
    /// </summary>
    public sealed class EnemyAttackConfigTests
    {
        private const string ConfigPath = "Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/AttackConfigs";

        // ─── Helper ───────────────────────────────────────────────────────────

        private static EnemyAttackConfig LoadConfig(string name)
        {
#if UNITY_EDITOR
            var path = $"{ConfigPath}/{name}.asset";
            return UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyAttackConfig>(path);
#else
            return null;
#endif
        }

        // ─── Config Existence ─────────────────────────────────────────────────

        [Test]
        public void AllExpectedConfigsExist()
        {
            var names = new[]
            {
                "AC_GreenDemon_MeleeArc",
                "AC_Bat_FlyingDive",
                "AC_Bee_StingCharge",
                "AC_YellowDragon_Fireball",
                "AC_Cactus_SpikeProjectile",
                "AC_Cactus_RadialSpikeBurst",
                "AC_Cthulhu_SlimeProjectileSlowPool",
                "AC_Cyclops_EyeBeam",
                "AC_Demon_LeapSlash",
                "AC_Ghost_PhaseHomingCurse",
                "AC_Mushroom_SporePoisonZone",
                "AC_Yeti_FrostSlamShockwave"
            };

            foreach (var name in names)
            {
                var cfg = LoadConfig(name);
                Assert.IsNotNull(cfg, $"Config '{name}' should exist in {ConfigPath}");
            }
        }

        // ─── Attack Types ─────────────────────────────────────────────────────

        [Test]
        public void GreenDemonHasMeleeArcType()
        {
            var cfg = LoadConfig("AC_GreenDemon_MeleeArc");
            if (cfg == null) Assert.Ignore("Config not built yet — run builder first.");
            Assert.AreEqual(EnemyDistinctAttackType.MeleeArc, cfg.AttackType);
        }

        [Test]
        public void BatHasDiveType()
        {
            var cfg = LoadConfig("AC_Bat_FlyingDive");
            if (cfg == null) Assert.Ignore("Config not built yet.");
            Assert.AreEqual(EnemyDistinctAttackType.Dive, cfg.AttackType);
        }

        [Test]
        public void BeeHasChargeType()
        {
            var cfg = LoadConfig("AC_Bee_StingCharge");
            if (cfg == null) Assert.Ignore("Config not built yet.");
            Assert.AreEqual(EnemyDistinctAttackType.Charge, cfg.AttackType);
        }

        [Test]
        public void CactusHasBothRangeAndClose()
        {
            var spike = LoadConfig("AC_Cactus_SpikeProjectile");
            var radial = LoadConfig("AC_Cactus_RadialSpikeBurst");
            if (spike == null || radial == null) Assert.Ignore("Cactus configs not built yet.");

            Assert.AreEqual(EnemyDistinctAttackType.SpikeProjectile, spike.AttackType);
            Assert.AreEqual(EnemyDistinctAttackType.RadialBurst, radial.AttackType);
        }

        [Test]
        public void CyclopsHasBeamType()
        {
            var cfg = LoadConfig("AC_Cyclops_EyeBeam");
            if (cfg == null) Assert.Ignore("Config not built yet.");
            Assert.AreEqual(EnemyDistinctAttackType.Beam, cfg.AttackType);
        }

        [Test]
        public void GhostHasHomingProjectileType()
        {
            var cfg = LoadConfig("AC_Ghost_PhaseHomingCurse");
            if (cfg == null) Assert.Ignore("Config not built yet.");
            Assert.AreEqual(EnemyDistinctAttackType.HomingProjectile, cfg.AttackType);
        }

        [Test]
        public void YetiHasFrostSlamType()
        {
            var cfg = LoadConfig("AC_Yeti_FrostSlamShockwave");
            if (cfg == null) Assert.Ignore("Config not built yet.");
            Assert.AreEqual(EnemyDistinctAttackType.FrostSlamShockwave, cfg.AttackType);
        }

        // ─── Value Ranges ─────────────────────────────────────────────────────

        [Test]
        public void AllConfigs_DamageIsPositive()
        {
            // Some attacks (Mushroom zone) have 0 direct damage but tick damage
            var names = new[] { "AC_GreenDemon_MeleeArc", "AC_Bat_FlyingDive", "AC_Bee_StingCharge",
                "AC_YellowDragon_Fireball", "AC_Cactus_SpikeProjectile", "AC_Cactus_RadialSpikeBurst",
                "AC_Cyclops_EyeBeam", "AC_Demon_LeapSlash", "AC_Ghost_PhaseHomingCurse",
                "AC_Yeti_FrostSlamShockwave" };

            foreach (var name in names)
            {
                var cfg = LoadConfig(name);
                if (cfg == null) continue;
                Assert.Greater(cfg.Damage, 0f, $"{name} should have damage > 0");
            }
        }

        [Test]
        public void AllConfigs_CooldownIsPositive()
        {
            foreach (var name in new[] { "AC_GreenDemon_MeleeArc", "AC_Bat_FlyingDive", "AC_Bee_StingCharge",
                "AC_YellowDragon_Fireball", "AC_Cactus_SpikeProjectile", "AC_Cactus_RadialSpikeBurst",
                "AC_Cthulhu_SlimeProjectileSlowPool", "AC_Cyclops_EyeBeam", "AC_Demon_LeapSlash",
                "AC_Ghost_PhaseHomingCurse", "AC_Mushroom_SporePoisonZone", "AC_Yeti_FrostSlamShockwave" })
            {
                var cfg = LoadConfig(name);
                if (cfg == null) continue;
                Assert.Greater(cfg.Cooldown, 0f, $"{name} cooldown must be > 0");
            }
        }

        [Test]
        public void AllConfigs_WindupIsReadable()
        {
            // Designer rule: no attack should have 0 windup (no telegraph window)
            foreach (var name in new[] { "AC_GreenDemon_MeleeArc", "AC_Bat_FlyingDive", "AC_Bee_StingCharge",
                "AC_YellowDragon_Fireball", "AC_Cactus_SpikeProjectile", "AC_Cactus_RadialSpikeBurst",
                "AC_Cthulhu_SlimeProjectileSlowPool", "AC_Cyclops_EyeBeam", "AC_Demon_LeapSlash",
                "AC_Ghost_PhaseHomingCurse", "AC_Mushroom_SporePoisonZone", "AC_Yeti_FrostSlamShockwave" })
            {
                var cfg = LoadConfig(name);
                if (cfg == null) continue;
                Assert.GreaterOrEqual(cfg.WindupTime, 0.25f, $"{name} windup must be >= 0.25s for readability");
            }
        }

        [Test]
        public void AllConfigs_TriggerRangeIsPositive()
        {
            foreach (var name in new[] { "AC_GreenDemon_MeleeArc", "AC_Bat_FlyingDive", "AC_Bee_StingCharge",
                "AC_YellowDragon_Fireball", "AC_Cactus_SpikeProjectile", "AC_Cactus_RadialSpikeBurst",
                "AC_Cthulhu_SlimeProjectileSlowPool", "AC_Cyclops_EyeBeam", "AC_Demon_LeapSlash",
                "AC_Ghost_PhaseHomingCurse", "AC_Mushroom_SporePoisonZone", "AC_Yeti_FrostSlamShockwave" })
            {
                var cfg = LoadConfig(name);
                if (cfg == null) continue;
                Assert.Greater(cfg.TriggerRange, 0f, $"{name} trigger range must be > 0");
            }
        }

        // ─── Status Effect Safety ─────────────────────────────────────────────

        [Test]
        public void CthulhuSlowMultiplierInValidRange()
        {
            var cfg = LoadConfig("AC_Cthulhu_SlimeProjectileSlowPool");
            if (cfg == null) Assert.Ignore("Config not built yet.");
            Assert.GreaterOrEqual(cfg.StatusEffectSlowMultiplier, 0f);
            Assert.LessOrEqual(cfg.StatusEffectSlowMultiplier, 1f);
            Assert.Greater(cfg.StatusEffectDuration, 0f);
        }

        [Test]
        public void YetiFrostSlowMultiplierInValidRange()
        {
            var cfg = LoadConfig("AC_Yeti_FrostSlamShockwave");
            if (cfg == null) Assert.Ignore("Config not built yet.");
            Assert.GreaterOrEqual(cfg.StatusEffectSlowMultiplier, 0f);
            Assert.LessOrEqual(cfg.StatusEffectSlowMultiplier, 1f);
        }

        [Test]
        public void MushroomHasPoisonEffect()
        {
            var cfg = LoadConfig("AC_Mushroom_SporePoisonZone");
            if (cfg == null) Assert.Ignore("Config not built yet.");
            Assert.AreEqual(StatusEffectType.Poison, cfg.StatusEffectType);
            Assert.Greater(cfg.AreaZoneTickDamage, 0f, "Mushroom spore zone should deal tick damage");
        }

        // ─── Area Zone Config ─────────────────────────────────────────────────

        [Test]
        public void AreaZoneConfigsHaveValidRadius()
        {
            foreach (var name in new[] { "AC_Cthulhu_SlimeProjectileSlowPool", "AC_Mushroom_SporePoisonZone", "AC_Yeti_FrostSlamShockwave" })
            {
                var cfg = LoadConfig(name);
                if (cfg == null) continue;
                Assert.Greater(cfg.AreaZoneRadius, 0f, $"{name} area zone radius must be > 0");
                Assert.Greater(cfg.AreaZoneDuration, 0f, $"{name} area zone duration must be > 0");
                Assert.Greater(cfg.AreaZoneTickInterval, 0f, $"{name} area zone tick interval must be > 0");
            }
        }

        // ─── Cyclops Beam ─────────────────────────────────────────────────────

        [Test]
        public void CyclopsBeamHasSignificantWindup()
        {
            var cfg = LoadConfig("AC_Cyclops_EyeBeam");
            if (cfg == null) Assert.Ignore("Config not built yet.");
            Assert.GreaterOrEqual(cfg.WindupTime, 0.75f, "Cyclops beam should have long windup for readability");
            Assert.Greater(cfg.BeamLength, 0f, "Cyclops beam length must be > 0");
            Assert.Greater(cfg.ActiveTime, 0.1f, "Cyclops beam needs active damage window");
        }

        // ─── Projectile Configs ───────────────────────────────────────────────

        [Test]
        public void ProjectileAttacksHaveSpeedAndLifetime()
        {
            var projectileConfigs = new[] {
                "AC_YellowDragon_Fireball",
                "AC_Cactus_SpikeProjectile",
                "AC_Cthulhu_SlimeProjectileSlowPool",
                "AC_Ghost_PhaseHomingCurse"
            };

            foreach (var name in projectileConfigs)
            {
                var cfg = LoadConfig(name);
                if (cfg == null) continue;
                Assert.Greater(cfg.ProjectileSpeed, 0f, $"{name} projectile speed must be > 0");
                Assert.Greater(cfg.ProjectileLifetime, 0.1f, $"{name} projectile lifetime must be > 0.1s");
            }
        }

        // ─── Homing Clamp ─────────────────────────────────────────────────────

        [Test]
        public void GhostHomingStrengthInValidRange()
        {
            var cfg = LoadConfig("AC_Ghost_PhaseHomingCurse");
            if (cfg == null) Assert.Ignore("Config not built yet.");
            Assert.GreaterOrEqual(cfg.HomingStrength, 0f);
            Assert.LessOrEqual(cfg.HomingStrength, 1f);
            // Must not be perfectly homing — require it stays weak enough to dodge
            Assert.Less(cfg.HomingStrength, 0.8f, "Homing strength should be weak enough to dodge");
        }

        // ─── EnemyAreaZone Data Validation ────────────────────────────────────

        [Test]
        public void EnemyAreaZone_InitializeDoesNotThrowWithValidData()
        {
            var go = new GameObject("TestZone");
            var zone = go.AddComponent<EnemyAreaZone>();

            Assert.DoesNotThrow(() =>
            {
                zone.Initialize(go, 2f, 3f, 1f, 2f, StatusEffectType.Slow, 2f, 0.6f, ~0);
            });

            Assert.IsTrue(zone.IsActive);
            Assert.AreEqual(2f, zone.RadiusValue);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void EnemyAreaZone_ZeroRadiusIsClampedToMinimum()
        {
            var go = new GameObject("TestZone");
            var zone = go.AddComponent<EnemyAreaZone>();
            zone.Initialize(go, 0f, 3f, 1f, 0f, StatusEffectType.None, 0f, 1f, ~0);

            Assert.Greater(zone.RadiusValue, 0f, "Radius should be clamped to minimum (0.1)");

            Object.DestroyImmediate(go);
        }

        // ─── EnemyDistinctAttackController Data Validation ────────────────────

        [Test]
        public void EnemyDistinctAttackController_StartsInIdleState()
        {
            var go = new GameObject("TestEnemy");
            var controller = go.AddComponent<EnemyDistinctAttackController>();

            Assert.IsFalse(controller.IsAttacking);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void EnemyDistinctAttackController_ResetForPool_ClearsState()
        {
            var go = new GameObject("TestEnemy");
            var controller = go.AddComponent<EnemyDistinctAttackController>();

            Assert.DoesNotThrow(() => controller.ResetForPool());
            Assert.IsFalse(controller.IsAttacking);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void EnemyHomingProjectile_InitializeDoesNotThrow()
        {
            var go = new GameObject("TestProjectile");
            go.AddComponent<Rigidbody>();
            var homing = go.AddComponent<EnemyHomingProjectile>();

            var target = new GameObject("Target");

            Assert.DoesNotThrow(() => homing.Initialize(target.transform, 0.3f, 45f));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(target);
        }
    }
}
