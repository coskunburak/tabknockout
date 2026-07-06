using NUnit.Framework;
using TapKnockout.Enemy;
using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.Enemy.Tests
{
    public sealed class EnemyAttackPatternTests
    {
        [Test]
        public void EnemyArchetype_ContainsPhaseFourValues()
        {
            Assert.That(EnemyArchetype.MeleeChaser, Is.EqualTo((EnemyArchetype)0));
            Assert.That(EnemyArchetype.FastCharger, Is.EqualTo((EnemyArchetype)1));
            Assert.That(EnemyArchetype.RangedShooter, Is.EqualTo((EnemyArchetype)2));
            Assert.That(EnemyArchetype.AreaBomber, Is.EqualTo((EnemyArchetype)3));
            Assert.That(EnemyArchetype.ShieldEnemy, Is.EqualTo((EnemyArchetype)4));
            Assert.That(EnemyArchetype.SplitterEnemy, Is.EqualTo((EnemyArchetype)5));
            Assert.That(EnemyArchetype.EliteChaser, Is.EqualTo((EnemyArchetype)6));
            Assert.That(EnemyArchetype.EliteRanged, Is.EqualTo((EnemyArchetype)7));
            Assert.That(EnemyArchetype.Boss, Is.EqualTo((EnemyArchetype)8));
        }

        [Test]
        public void SetSteps_ClampsNegativeTimingAndDamage()
        {
            var config = ScriptableObject.CreateInstance<EnemyAttackPatternConfig>();

            try
            {
                config.SetSteps(new[]
                {
                    new EnemyAttackStep(
                        EnemyAttackType.AreaBomb,
                        -1f,
                        -2f,
                        -3f,
                        -4f,
                        -5f,
                        -6f,
                        -1,
                        -8f,
                        EnemyTelegraphType.Circle,
                        VFXEventType.EnemyTelegraph)
                });

                Assert.That(config.Steps.Count, Is.EqualTo(1));
                var step = config.Steps[0];
                Assert.That(step.WindupDuration, Is.GreaterThanOrEqualTo(0f));
                Assert.That(step.ActiveDuration, Is.GreaterThanOrEqualTo(0f));
                Assert.That(step.CooldownDuration, Is.GreaterThanOrEqualTo(0f));
                Assert.That(step.Damage, Is.GreaterThanOrEqualTo(0f));
                Assert.That(step.ProjectileCount, Is.GreaterThanOrEqualTo(0));
                Assert.That(step.ProjectileSpeed, Is.GreaterThanOrEqualTo(0f));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void PatternController_AdvancesThroughStepPhases()
        {
            var enemy = new GameObject("Enemy");
            var config = ScriptableObject.CreateInstance<EnemyAttackPatternConfig>();

            try
            {
                config.SetLoop(false);
                config.SetSteps(new[]
                {
                    new EnemyAttackStep(EnemyAttackType.MeleeSwing, 0.1f, 0.1f, 0.1f, 5f, 1f, 0f, 0, 0f, EnemyTelegraphType.Circle)
                });

                var controller = enemy.AddComponent<EnemyAttackPatternController>();
                controller.SetConfig(config);

                Assert.That(controller.StartPattern(), Is.True);
                Assert.That(controller.CurrentPhase, Is.EqualTo(EnemyAttackPatternPhase.Windup));

                controller.Advance(0.11f);
                Assert.That(controller.CurrentPhase, Is.EqualTo(EnemyAttackPatternPhase.Active));

                controller.Advance(0.11f);
                Assert.That(controller.CurrentPhase, Is.EqualTo(EnemyAttackPatternPhase.Cooldown));

                controller.Advance(0.11f);
                Assert.That(controller.CurrentPhase, Is.EqualTo(EnemyAttackPatternPhase.Completed));
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(enemy);
            }
        }
    }
}
