using NUnit.Framework;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Player.Tests
{
    public sealed class PlayerConfigTests
    {
        [Test]
        public void DefaultConfigValues_AreSaneForMovementFoundation()
        {
            var config = ScriptableObject.CreateInstance<PlayerConfig>();

            try
            {
                Assert.That(config.MoveSpeed, Is.GreaterThan(0f));
                Assert.That(config.MaxHealth, Is.GreaterThan(0f));
                Assert.That(config.ContactDamageInvulnerabilityWindow, Is.GreaterThanOrEqualTo(0f));
                Assert.That(config.Acceleration, Is.GreaterThan(0f));
                Assert.That(config.RotationSpeed, Is.GreaterThan(0f));
                Assert.That(config.MovementDeadZone, Is.InRange(0f, 0.95f));
                Assert.That(config.StopToAttackMovementThreshold, Is.GreaterThanOrEqualTo(0f));
                Assert.That(config.DashDistance, Is.GreaterThan(0f));
                Assert.That(config.DashDuration, Is.GreaterThan(0f));
                Assert.That(config.DashCooldown, Is.GreaterThan(config.DashDuration));
                Assert.That(config.DashImpactDamage, Is.GreaterThanOrEqualTo(0f));
                Assert.That(config.DashKnockbackForce, Is.GreaterThanOrEqualTo(0f));
                Assert.That(config.DashKnockbackDuration, Is.GreaterThanOrEqualTo(0f));
                Assert.That(config.DashHitRadius, Is.GreaterThan(0f));
                Assert.That(config.DashHasIFrames, Is.True);
                Assert.That(config.DashIFrameDuration, Is.GreaterThanOrEqualTo(0f));
                Assert.That(config.DashIFrameDuration, Is.LessThanOrEqualTo(config.DashDuration));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }
    }
}
