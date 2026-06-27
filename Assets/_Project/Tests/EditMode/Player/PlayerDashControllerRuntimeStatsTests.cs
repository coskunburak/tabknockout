using NUnit.Framework;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Player.Tests
{
    public sealed class PlayerDashControllerRuntimeStatsTests
    {
        [Test]
        public void EffectiveDashValues_UseRuntimeStatsWhenAvailable()
        {
            var player = new GameObject("Player");

            try
            {
                player.AddComponent<Rigidbody>();
                var stats = player.AddComponent<PlayerRuntimeStats>();
                player.AddComponent<PlayerMovementController>();
                var dashController = player.AddComponent<PlayerDashController>();

                stats.AddDashCooldownReduction(0.5f);
                stats.AddDashDamageMultiplier(0.5f);
                stats.AddDashKnockbackMultiplier(0.25f);

                Assert.That(dashController.EffectiveDashCooldown, Is.EqualTo(2f).Within(0.0001f));
                Assert.That(dashController.EffectiveDashImpactDamage, Is.EqualTo(18f).Within(0.0001f));
                Assert.That(dashController.EffectiveDashKnockbackForce, Is.EqualTo(10f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }
    }
}
