using NUnit.Framework;
using TapKnockout.Combat;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Player.Tests
{
    public sealed class PlayerHealthTests
    {
        [Test]
        public void ReceiveHit_ReducesHealth()
        {
            var player = new GameObject("Player");

            try
            {
                var health = player.AddComponent<PlayerHealth>();
                health.ResetHealth();

                health.ReceiveHit(new HitContext(null, player, 15f, DamageType.Physical));

                Assert.That(health.CurrentHealth, Is.EqualTo(health.MaxHealth - 15f));
                Assert.That(health.IsAlive, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void ReceiveHit_DeathTriggersOnce()
        {
            var player = new GameObject("Player");
            var deathCount = 0;

            try
            {
                var health = player.AddComponent<PlayerHealth>();
                health.ResetHealth();
                health.OnPlayerDied += _ => deathCount++;

                health.ReceiveHit(new HitContext(null, player, 999f, DamageType.Physical));
                health.ReceiveHit(new HitContext(null, player, 999f, DamageType.Physical));

                Assert.That(health.CurrentHealth, Is.EqualTo(0f));
                Assert.That(health.IsAlive, Is.False);
                Assert.That(deathCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void ReceiveHit_IgnoresSecondHitDuringContactInvulnerabilityWindow()
        {
            var player = new GameObject("Player");

            try
            {
                var health = player.AddComponent<PlayerHealth>();
                health.ResetHealth();
                var ignoredHit = new HitContext(null, player, 10f, DamageType.Physical);

                health.ReceiveHit(new HitContext(null, player, 10f, DamageType.Physical));
                health.ReceiveHit(ignoredHit);

                Assert.That(health.CurrentHealth, Is.EqualTo(health.MaxHealth - 10f));
                Assert.That(health.IsDamageInvulnerabilityActive, Is.True);
                Assert.That(ignoredHit.WasIgnored, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void ReceiveHit_IgnoresDamageDuringDashIFrame()
        {
            var player = new GameObject("Player");

            try
            {
                player.AddComponent<Rigidbody>();
                player.AddComponent<PlayerMovementController>();
                var dash = player.AddComponent<PlayerDashController>();
                var health = player.AddComponent<PlayerHealth>();
                health.SetDashController(dash);
                health.ResetHealth();

                Assert.That(dash.TryDash(), Is.True);
                Assert.That(health.IsDashInvulnerable, Is.True);

                var ignoredHit = new HitContext(null, player, 20f, DamageType.Physical);
                health.ReceiveHit(ignoredHit);

                Assert.That(health.CurrentHealth, Is.EqualTo(health.MaxHealth));
                Assert.That(ignoredHit.WasIgnored, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void RefreshFromRuntimeStats_UsesMaxHealthBonus()
        {
            var player = new GameObject("Player");

            try
            {
                var stats = player.AddComponent<PlayerRuntimeStats>();
                var health = player.AddComponent<PlayerHealth>();
                health.SetRuntimeStats(stats);
                health.ResetHealth();

                stats.AddMaxHealthBonus(20f);
                health.RefreshFromRuntimeStats(20f);

                Assert.That(health.MaxHealth, Is.EqualTo(120f));
                Assert.That(health.CurrentHealth, Is.EqualTo(120f));
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }
    }
}
