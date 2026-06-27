using NUnit.Framework;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Player.Tests
{
    public sealed class PlayerRuntimeStatsTests
    {
        [Test]
        public void DefaultValues_AreNeutral()
        {
            var gameObject = new GameObject("RuntimeStats");

            try
            {
                var stats = gameObject.AddComponent<PlayerRuntimeStats>();

                Assert.That(stats.AttackDamageMultiplier, Is.EqualTo(1f));
                Assert.That(stats.AttackCooldownMultiplier, Is.EqualTo(1f));
                Assert.That(stats.DashCooldownMultiplier, Is.EqualTo(1f));
                Assert.That(stats.DashDamageMultiplier, Is.EqualTo(1f));
                Assert.That(stats.DashKnockbackMultiplier, Is.EqualTo(1f));
                Assert.That(stats.MaxHealthBonus, Is.EqualTo(0f));
                Assert.That(stats.MoveSpeedMultiplier, Is.EqualTo(1f));
                Assert.That(stats.ProjectileSpeedMultiplier, Is.EqualTo(1f));
                Assert.That(stats.ExtraProjectileCount, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void AdditiveModifiers_UpdateEffectiveValues()
        {
            var gameObject = new GameObject("RuntimeStats");

            try
            {
                var stats = gameObject.AddComponent<PlayerRuntimeStats>();

                stats.AddAttackDamageMultiplier(0.15f);
                stats.AddAttackCooldownReduction(0.10f);
                stats.AddDashCooldownReduction(0.15f);
                stats.AddDashDamageMultiplier(0.20f);
                stats.AddDashKnockbackMultiplier(0.25f);
                stats.AddMaxHealthBonus(20f);
                stats.AddMoveSpeedMultiplier(0.10f);
                stats.AddProjectileSpeedMultiplier(0.30f);
                stats.AddExtraProjectileCount(1);

                Assert.That(stats.AttackDamageMultiplier, Is.EqualTo(1.15f).Within(0.0001f));
                Assert.That(stats.AttackCooldownMultiplier, Is.EqualTo(0.90f).Within(0.0001f));
                Assert.That(stats.DashCooldownMultiplier, Is.EqualTo(0.85f).Within(0.0001f));
                Assert.That(stats.DashDamageMultiplier, Is.EqualTo(1.20f).Within(0.0001f));
                Assert.That(stats.DashKnockbackMultiplier, Is.EqualTo(1.25f).Within(0.0001f));
                Assert.That(stats.MaxHealthBonus, Is.EqualTo(20f));
                Assert.That(stats.MoveSpeedMultiplier, Is.EqualTo(1.10f).Within(0.0001f));
                Assert.That(stats.ProjectileSpeedMultiplier, Is.EqualTo(1.30f).Within(0.0001f));
                Assert.That(stats.ExtraProjectileCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void CooldownReductions_AreClampedAboveZero()
        {
            var gameObject = new GameObject("RuntimeStats");

            try
            {
                var stats = gameObject.AddComponent<PlayerRuntimeStats>();

                stats.AddAttackCooldownReduction(100f);
                stats.AddDashCooldownReduction(100f);

                Assert.That(stats.AttackCooldownMultiplier, Is.GreaterThan(0f));
                Assert.That(stats.DashCooldownMultiplier, Is.GreaterThan(0f));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void ResetRunModifiers_ReturnsToNeutralValues()
        {
            var gameObject = new GameObject("RuntimeStats");

            try
            {
                var stats = gameObject.AddComponent<PlayerRuntimeStats>();
                stats.AddAttackDamageMultiplier(0.5f);
                stats.AddMoveSpeedMultiplier(0.25f);
                stats.AddExtraProjectileCount(2);

                stats.ResetRunModifiers();

                Assert.That(stats.AttackDamageMultiplier, Is.EqualTo(1f));
                Assert.That(stats.MoveSpeedMultiplier, Is.EqualTo(1f));
                Assert.That(stats.ExtraProjectileCount, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
