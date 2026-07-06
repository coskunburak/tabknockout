using NUnit.Framework;
using TapKnockout.Boss;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Boss.Tests
{
    public sealed class BossAttackFoundationTests
    {
        [Test]
        public void BossSlam_RecordsTelegraphBeforeDamage()
        {
            var boss = new GameObject("Boss");

            try
            {
                var slam = boss.AddComponent<BossSlamAttack>();
                var step = new BossAttackStep(BossAttackType.BossSlam, 0.25f, 0.1f, 0.5f, 10f, 1.5f, 0f, 0, EnemyTelegraphType.BossSlamArea);

                slam.BeginTelegraph(step);
                slam.Execute(step);

                Assert.That(slam.LastDamageResolvedAfterTelegraph, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(boss);
            }
        }

        [Test]
        public void BossCharge_ResolvesFallbackDirection()
        {
            var direction = BossChargeAttack.ResolveDirection(Vector3.zero, Vector3.zero, Vector3.right);

            Assert.That(direction.x, Is.EqualTo(1f).Within(0.001f));
            Assert.That(direction.z, Is.EqualTo(0f).Within(0.001f));
        }
    }
}
