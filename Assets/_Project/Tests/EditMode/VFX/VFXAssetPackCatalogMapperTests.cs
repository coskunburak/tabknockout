using NUnit.Framework;
using TapKnockout.VFX;

namespace TapKnockout.VFX.Tests
{
    public sealed class VFXAssetPackCatalogMapperTests
    {
        [Test]
        public void ScoreCandidate_PrefersLightningHitForDashImpact()
        {
            var dashImpactScore = VFXCandidateScoring.ScoreCandidate(
                VFXEventType.DashImpact,
                "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Lightning Hit Blue.prefab");
            var lootScore = VFXCandidateScoring.ScoreCandidate(
                VFXEventType.DashImpact,
                "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_LootDrop_Blue.prefab");

            Assert.That(dashImpactScore, Is.GreaterThan(lootScore));
        }

        [Test]
        public void ScoreCandidate_PrefersMagicCircleForBossWarning()
        {
            var warningScore = VFXCandidateScoring.ScoreCandidate(
                VFXEventType.BossWarning,
                "Assets/ThirdParty/VFX/Eric VFX Studio/Game VFX - Magic Circle(Free)/Prefabs/FX_MagicCircle_Icearrow01.prefab");
            var hitScore = VFXCandidateScoring.ScoreCandidate(
                VFXEventType.BossWarning,
                "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Basic Hit 2.prefab");

            Assert.That(warningScore, Is.GreaterThan(hitScore));
        }

        [Test]
        public void ScoreCandidate_PenalizesTextEffectsForCombatHits()
        {
            var textScore = VFXCandidateScoring.ScoreCandidate(
                VFXEventType.EnemyHit,
                "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Texts/CFXR _POW_.prefab");
            var hitScore = VFXCandidateScoring.ScoreCandidate(
                VFXEventType.EnemyHit,
                "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Basic Hit 2.prefab");

            Assert.That(hitScore, Is.GreaterThan(textScore));
        }
    }
}
