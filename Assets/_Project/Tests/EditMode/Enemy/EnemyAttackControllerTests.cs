using NUnit.Framework;
using TapKnockout.Combat;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Enemy.Tests
{
    public sealed class EnemyAttackControllerTests
    {
        [Test]
        public void IsTargetInRange_UsesHorizontalDistanceOnly()
        {
            var source = new Vector3(0f, 0f, 0f);
            var target = new Vector3(0f, 5f, 1.1f);

            Assert.That(EnemyAttackController.IsTargetInRange(source, target, 1.2f), Is.True);
            Assert.That(EnemyAttackController.IsTargetInRange(source, target, 1f), Is.False);
        }

        [Test]
        public void TryDealContactDamage_AppliesHitAndStartsCooldown()
        {
            var enemy = new GameObject("Enemy");
            var target = new GameObject("PlayerTarget");

            try
            {
                enemy.transform.position = Vector3.zero;
                target.transform.position = Vector3.forward;

                var targetDamageable = target.AddComponent<TestDamageable>();
                var attack = enemy.AddComponent<EnemyAttackController>();
                attack.Initialize(null, target.transform);

                Assert.That(attack.TryDealContactDamage(), Is.True);
                Assert.That(targetDamageable.ReceivedHitCount, Is.EqualTo(1));
                Assert.That(targetDamageable.LastHit.DamageAmount, Is.EqualTo(8f));
                Assert.That(attack.IsCooldownReady, Is.False);
                Assert.That(attack.TryDealContactDamage(), Is.False);
                Assert.That(targetDamageable.ReceivedHitCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(enemy);
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void TryDealContactDamage_DoesNotRaiseDamageEventsWhenTargetIgnoresHit()
        {
            var enemy = new GameObject("Enemy");
            var target = new GameObject("PlayerTarget");
            var receivedCount = 0;

            void HandleDamageReceived(DamageEvent damageEvent)
            {
                receivedCount++;
            }

            CombatEvents.OnDamageReceived += HandleDamageReceived;

            try
            {
                enemy.transform.position = Vector3.zero;
                target.transform.position = Vector3.forward;

                target.AddComponent<IgnoringDamageable>();
                var attack = enemy.AddComponent<EnemyAttackController>();
                attack.Initialize(null, target.transform);

                Assert.That(attack.TryDealContactDamage(), Is.True);
                Assert.That(receivedCount, Is.EqualTo(0));
            }
            finally
            {
                CombatEvents.OnDamageReceived -= HandleDamageReceived;
                Object.DestroyImmediate(enemy);
                Object.DestroyImmediate(target);
            }
        }

        private sealed class TestDamageable : MonoBehaviour, IDamageable
        {
            public bool IsAlive { get; set; } = true;
            public GameObject GameObject => gameObject;
            public int ReceivedHitCount { get; private set; }
            public HitContext LastHit { get; private set; }

            public void ReceiveHit(HitContext hitContext)
            {
                ReceivedHitCount++;
                LastHit = hitContext;
            }
        }

        private sealed class IgnoringDamageable : MonoBehaviour, IDamageable
        {
            public bool IsAlive => true;
            public GameObject GameObject => gameObject;

            public void ReceiveHit(HitContext hitContext)
            {
                hitContext.WasIgnored = true;
            }
        }
    }
}
