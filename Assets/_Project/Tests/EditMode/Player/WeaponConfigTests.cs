using NUnit.Framework;
using TapKnockout.Combat;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Player.Tests
{
    public sealed class WeaponConfigTests
    {
        [Test]
        public void DefaultConfigValues_AreSafeForAutoAttackFoundation()
        {
            var config = ScriptableObject.CreateInstance<WeaponConfig>();

            try
            {
                Assert.That(config.WeaponId, Is.Not.Empty);
                Assert.That(config.AttackDamage, Is.GreaterThan(0f));
                Assert.That(config.AttackCooldown, Is.GreaterThan(0f));
                Assert.That(config.AttackRange, Is.GreaterThan(0f));
                Assert.That(config.ProjectileSpeed, Is.GreaterThan(0f));
                Assert.That(config.ProjectileLifetime, Is.GreaterThan(0f));
                Assert.That(config.DamageType, Is.EqualTo(DamageType.Physical));
                Assert.That(config.CriticalChance, Is.InRange(0f, 1f));
                Assert.That(config.CriticalMultiplier, Is.GreaterThanOrEqualTo(1f));
                Assert.That(config.ProjectilePrefab, Is.Null);
                Assert.That(config.TargetLayers.value, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }
    }
}
