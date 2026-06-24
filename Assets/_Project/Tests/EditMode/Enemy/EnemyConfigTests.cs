using NUnit.Framework;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Enemy.Tests
{
    public sealed class EnemyConfigTests
    {
        [Test]
        public void DefaultConfigValues_AreSafeForMeleeChaserFoundation()
        {
            var config = ScriptableObject.CreateInstance<EnemyConfig>();

            try
            {
                Assert.That(config.EnemyId, Is.Not.Empty);
                Assert.That(config.MaxHealth, Is.GreaterThan(0f));
                Assert.That(config.MoveSpeed, Is.GreaterThan(0f));
                Assert.That(config.Acceleration, Is.GreaterThan(0f));
                Assert.That(config.RotationSpeed, Is.GreaterThan(0f));
                Assert.That(config.ContactDamage, Is.GreaterThanOrEqualTo(0f));
                Assert.That(config.AttackRange, Is.GreaterThan(0f));
                Assert.That(config.AttackCooldown, Is.GreaterThan(0f));
                Assert.That(config.StoppingDistance, Is.GreaterThan(0f));
                Assert.That(config.KnockbackResistance, Is.InRange(0f, 1f));
                Assert.That(config.DeathDelay, Is.GreaterThanOrEqualTo(0f));
                Assert.That(config.CoinReward, Is.GreaterThanOrEqualTo(0));
                Assert.That(config.XpReward, Is.GreaterThanOrEqualTo(0));
                Assert.That(config.CanBeKnockedBack, Is.True);
                Assert.That(config.CanBeInterrupted, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }
    }
}
