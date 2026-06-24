using NUnit.Framework;
using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Combat.Tests
{
    public sealed class DummyDamageableTests
    {
        [Test]
        public void ReceiveHit_ReducesHealthAndKeepsContractAliveState()
        {
            var target = new GameObject("Dummy Target");

            try
            {
                var damageable = target.AddComponent<DummyDamageable>();
                damageable.ResetHealth();
                var startingHealth = damageable.CurrentHealth;

                damageable.ReceiveHit(new HitContext(null, target, 10f, DamageType.Physical));

                Assert.That(damageable.CurrentHealth, Is.EqualTo(startingHealth - 10f));
                Assert.That(damageable.IsAlive, Is.True);
                Assert.That(damageable.GameObject, Is.EqualTo(target));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
