using NUnit.Framework;
using UnityEngine;

namespace TapKnockout.Survivor.Tests
{
    public sealed class SurvivorConfigTests
    {
        [Test]
        public void SurvivorRunTimer_CompletesAtConfiguredDuration()
        {
            var timer = new SurvivorRunTimer();

            timer.Configure(10f);
            timer.Tick(4f);
            timer.Tick(6f);

            Assert.That(timer.ElapsedSeconds, Is.EqualTo(10f));
            Assert.That(timer.IsComplete, Is.True);
            Assert.That(timer.NormalizedTime, Is.EqualTo(1f));
        }

        [Test]
        public void ArenaConfig_ClampToArena_ConstrainsHorizontalPosition()
        {
            var arenaConfig = ScriptableObject.CreateInstance<ArenaConfig>();

            try
            {
                var clamped = arenaConfig.ClampToArena(new Vector3(100f, 3f, 0f));

                Assert.That(clamped.y, Is.EqualTo(3f));
                Assert.That(new Vector2(clamped.x, clamped.z).magnitude, Is.LessThanOrEqualTo(arenaConfig.ArenaRadius + 0.001f));
            }
            finally
            {
                Object.DestroyImmediate(arenaConfig);
            }
        }

        [Test]
        public void RunConfig_DefaultsExposeSafePrototypeValues()
        {
            var runConfig = ScriptableObject.CreateInstance<RunConfig>();

            try
            {
                Assert.That(runConfig.RunId, Is.Not.Empty);
                Assert.That(runConfig.TargetRunDurationSeconds, Is.GreaterThanOrEqualTo(30f));
                Assert.That(runConfig.StartingEnemyCap, Is.GreaterThan(0));
                Assert.That(runConfig.MaxEnemyCap, Is.GreaterThanOrEqualTo(runConfig.StartingEnemyCap));
                Assert.That(runConfig.GetXPRequiredForLevel(1), Is.GreaterThan(0));
            }
            finally
            {
                Object.DestroyImmediate(runConfig);
            }
        }
    }
}
