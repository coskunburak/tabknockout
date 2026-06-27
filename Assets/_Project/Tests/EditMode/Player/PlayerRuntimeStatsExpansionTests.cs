using NUnit.Framework;
using UnityEngine;

namespace TapKnockout.Player.Tests
{
    public sealed class PlayerRuntimeStatsExpansionTests
    {
        [Test]
        public void ExpandedCountsAndProcChances_AreClamped()
        {
            var gameObject = new GameObject("RuntimeStats");

            try
            {
                var stats = gameObject.AddComponent<PlayerRuntimeStats>();

                stats.AddExtraProjectileCount(99);
                stats.AddProjectilePierceCount(99);
                stats.AddProjectileRicochetCount(99);
                stats.AddProjectileWallBounceCount(99);
                stats.AddBurnOnHit(99f);
                stats.AddDamageReduction(99f);

                Assert.That(stats.ExtraProjectileCount, Is.EqualTo(6));
                Assert.That(stats.ProjectilePierceCount, Is.EqualTo(5));
                Assert.That(stats.ProjectileRicochetCount, Is.EqualTo(5));
                Assert.That(stats.ProjectileWallBounceCount, Is.EqualTo(3));
                Assert.That(stats.BurnOnHitChance, Is.EqualTo(1f));
                Assert.That(stats.DamageReductionMultiplier, Is.EqualTo(0.2f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
