using NUnit.Framework;
using TapKnockout.Boss;
using UnityEngine;

namespace TapKnockout.Boss.Tests
{
    public sealed class BossPatternControllerTests
    {
        [Test]
        public void StartPattern_WithNoSteps_ReturnsFalse()
        {
            var boss = new GameObject("Boss");
            var config = ScriptableObject.CreateInstance<BossPatternConfig>();

            try
            {
                var controller = boss.AddComponent<BossPatternController>();
                controller.SetConfig(config);

                Assert.That(controller.StartPattern(), Is.False);
                Assert.That(controller.IsRunning, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(boss);
            }
        }

        [Test]
        public void Advance_MovesThroughWindupActiveAndCooldown()
        {
            var boss = new GameObject("Boss");
            var config = ScriptableObject.CreateInstance<BossPatternConfig>();

            try
            {
                config.SetLoop(false);
                config.SetSteps(new[]
                {
                    new BossAttackStep(BossAttackType.MeleeSlam, 0.1f, 0.2f, 0.3f)
                });

                var controller = boss.AddComponent<BossPatternController>();
                controller.SetConfig(config);

                Assert.That(controller.StartPattern(), Is.True);
                Assert.That(controller.CurrentPhase, Is.EqualTo(BossPatternPhase.Windup));

                controller.Advance(0.11f);
                Assert.That(controller.CurrentPhase, Is.EqualTo(BossPatternPhase.Active));

                controller.Advance(0.21f);
                Assert.That(controller.CurrentPhase, Is.EqualTo(BossPatternPhase.Cooldown));

                controller.Advance(0.31f);
                Assert.That(controller.CurrentPhase, Is.EqualTo(BossPatternPhase.Completed));
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(boss);
            }
        }
    }
}
