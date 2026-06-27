using NUnit.Framework;
using TapKnockout.Combat;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Enemy.Tests
{
    public sealed class EnemyHealthTests
    {
        [Test]
        public void Initialize_UsesConfigMaxHealth()
        {
            var config = ScriptableObject.CreateInstance<EnemyConfig>();
            var enemy = new GameObject("Enemy");

            try
            {
                var health = enemy.AddComponent<EnemyHealth>();
                health.Initialize(config);

                Assert.That(health.MaxHealth, Is.EqualTo(config.MaxHealth));
                Assert.That(health.CurrentHealth, Is.EqualTo(config.MaxHealth));
                Assert.That(health.IsAlive, Is.True);
                Assert.That(health.IsTargetable, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void ReceiveHit_ReducesHealth()
        {
            var enemy = new GameObject("Enemy");

            try
            {
                var health = enemy.AddComponent<EnemyHealth>();
                health.ResetHealth();

                health.ReceiveHit(new HitContext(null, enemy, 10f, DamageType.Physical));

                Assert.That(health.CurrentHealth, Is.EqualTo(health.MaxHealth - 10f));
                Assert.That(health.IsAlive, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void ReceiveHit_DeathTriggersOnceAndIgnoresLaterHits()
        {
            var enemy = new GameObject("Enemy");
            var deathCount = 0;

            try
            {
                var health = enemy.AddComponent<EnemyHealth>();
                health.ResetHealth();
                health.OnDied += _ => deathCount++;

                health.ReceiveHit(new HitContext(null, enemy, 999f, DamageType.Impact));
                health.ReceiveHit(new HitContext(null, enemy, 999f, DamageType.Impact));

                Assert.That(health.CurrentHealth, Is.EqualTo(0f));
                Assert.That(health.IsAlive, Is.False);
                Assert.That(health.IsTargetable, Is.False);
                Assert.That(deathCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void ReceiveHit_WhenKilled_DisablesCollidersAndResetReEnablesThem()
        {
            var enemy = new GameObject("Enemy");

            try
            {
                var collider = enemy.AddComponent<CapsuleCollider>();
                var health = enemy.AddComponent<EnemyHealth>();
                health.ResetHealth();

                health.ReceiveHit(new HitContext(null, enemy, 999f, DamageType.Impact));

                Assert.That(collider.enabled, Is.False);

                health.ResetHealth();

                Assert.That(collider.enabled, Is.True);
                Assert.That(health.IsAlive, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(enemy);
            }
        }
    }
}
