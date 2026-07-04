using NUnit.Framework;
using TapKnockout.Boss;
using UnityEngine;

namespace TapKnockout.Boss.Tests
{
    public sealed class BossPhaseControllerTests
    {
        [Test]
        public void ResolvePhaseForHealthPercent_UsesDescendingThresholds()
        {
            var config = ScriptableObject.CreateInstance<BossConfig>();

            try
            {
                config.SetPhases(new[]
                {
                    new BossPhaseConfig(BossPhaseState.Phase1, 1f, null),
                    new BossPhaseConfig(BossPhaseState.Phase2, 0.66f, null),
                    new BossPhaseConfig(BossPhaseState.Phase3, 0.33f, null, true)
                });

                Assert.That(config.ResolvePhaseForHealthPercent(1f).PhaseState, Is.EqualTo(BossPhaseState.Phase1));
                Assert.That(config.ResolvePhaseForHealthPercent(0.7f).PhaseState, Is.EqualTo(BossPhaseState.Phase1));
                Assert.That(config.ResolvePhaseForHealthPercent(0.66f).PhaseState, Is.EqualTo(BossPhaseState.Phase2));
                Assert.That(config.ResolvePhaseForHealthPercent(0.34f).PhaseState, Is.EqualTo(BossPhaseState.Phase2));
                Assert.That(config.ResolvePhaseForHealthPercent(0.33f).PhaseState, Is.EqualTo(BossPhaseState.Phase3));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void BossAddSpawnAction_RespectsMaxActiveAdds()
        {
            Assert.That(BossAddSpawnAction.CalculateSpawnCount(3, 1, 4), Is.EqualTo(3));
            Assert.That(BossAddSpawnAction.CalculateSpawnCount(3, 3, 4), Is.EqualTo(1));
            Assert.That(BossAddSpawnAction.CalculateSpawnCount(3, 4, 4), Is.EqualTo(0));
        }
    }
}
