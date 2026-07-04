using NUnit.Framework;
using TapKnockout.Combat;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Player.Tests
{
    public sealed class PlayerAbilityCombatEffectControllerTests
    {
        [Test]
        public void EntityKilledByPlayer_HealsFromRuntimeStats()
        {
            var player = new GameObject("Player");
            var enemy = new GameObject("Enemy");

            try
            {
                var stats = player.AddComponent<PlayerRuntimeStats>();
                var health = player.AddComponent<PlayerHealth>();
                var effects = player.AddComponent<PlayerAbilityCombatEffectController>();
                effects.SetRuntimeStats(stats);
                effects.SetPlayerHealth(health);
                health.SetRuntimeStats(stats);
                health.ResetHealth();
                stats.AddHealOnKill(7f);
                health.ReceiveHit(new HitContext(null, player, 20f));

                CombatEvents.RaiseEntityKilled(new EntityKilledEvent(enemy, player, new HitContext(player, enemy, 10f)));

                Assert.That(health.CurrentHealth, Is.EqualTo(87f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(enemy);
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void PlayerDamageDealt_AppliesGuaranteedBurnStatus()
        {
            var player = new GameObject("Player");
            var target = new GameObject("Target");

            try
            {
                var stats = player.AddComponent<PlayerRuntimeStats>();
                var effects = player.AddComponent<PlayerAbilityCombatEffectController>();
                effects.SetRuntimeStats(stats);
                stats.AddBurnOnHit(1f);

                target.AddComponent<DummyDamageable>();
                var statusController = target.AddComponent<StatusEffectController>();
                var hitContext = new HitContext(player, target, 10f)
                {
                    IsProjectileHit = true
                };

                CombatEvents.RaiseDamageDealt(new DamageEvent(player, target, 10f, DamageType.Physical, hitContext));

                Assert.That(statusController.ActiveEffectCount, Is.EqualTo(1));
                Assert.That(statusController.HasActiveEffect(StatusEffectType.Burn), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(player);
            }
        }
    }
}
