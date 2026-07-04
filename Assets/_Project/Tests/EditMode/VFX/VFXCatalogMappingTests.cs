using System.Collections.Generic;
using NUnit.Framework;
using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.VFX.Tests
{
    public sealed class VFXCatalogMappingTests
    {
        [Test]
        public void CatalogMappings_DoNotCreateDuplicateEventDefinitions()
        {
            var catalog = ScriptableObject.CreateInstance<VFXCatalog>();
            try
            {
                catalog.SetDefinitions(new[]
                {
                    new VFXDefinition(VFXEventType.DashImpact, null, 8, 1.1f),
                    new VFXDefinition(VFXEventType.EnemyHit, null, 16, 0.8f),
                    new VFXDefinition(VFXEventType.ProjectileHit, null, 16, 0.8f),
                    new VFXDefinition(VFXEventType.EnemyDeath, null, 12, 1.2f),
                    new VFXDefinition(VFXEventType.RoomClear, null, 2, 2f),
                    new VFXDefinition(VFXEventType.BossWarning, null, 4, 2f)
                });

                var seen = new HashSet<VFXEventType>();
                var definitions = catalog.Definitions;
                for (var i = 0; i < definitions.Count; i++)
                {
                    Assert.That(seen.Add(definitions[i].EventType), Is.True);
                }
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void CatalogMappings_CoverProductionSurvivorVFXEvents()
        {
            var catalog = ScriptableObject.CreateInstance<VFXCatalog>();
            try
            {
                var requiredEvents = new[]
                {
                    VFXEventType.PrimaryFireMuzzle,
                    VFXEventType.PrimaryProjectileTrail,
                    VFXEventType.PrimaryProjectileImpact,
                    VFXEventType.ForwardCleaveCast,
                    VFXEventType.ForwardCleaveHit,
                    VFXEventType.GroundImpactCast,
                    VFXEventType.GroundImpactArea,
                    VFXEventType.GroundImpactHit,
                    VFXEventType.DashStart,
                    VFXEventType.DashTrail,
                    VFXEventType.DashEnd,
                    VFXEventType.EnemyHit,
                    VFXEventType.EnemyDeath,
                    VFXEventType.EnemyDeathLarge,
                    VFXEventType.EnemySpawn,
                    VFXEventType.EliteSpawn,
                    VFXEventType.EliteDeath,
                    VFXEventType.SpawnTelegraph,
                    VFXEventType.BossSpawnWarning,
                    VFXEventType.BossPhaseTransition,
                    VFXEventType.BossHeavyAttackTelegraph,
                    VFXEventType.BossHeavyAttackImpact,
                    VFXEventType.BossDeath,
                    VFXEventType.XPOrbCollect,
                    VFXEventType.LevelUpBurst,
                    VFXEventType.ReticleFirePulse
                };

                var definitions = new List<VFXDefinition>();
                for (var i = 0; i < requiredEvents.Length; i++)
                {
                    definitions.Add(new VFXDefinition(requiredEvents[i], null, 1, 0.5f));
                }

                catalog.SetDefinitions(definitions);

                for (var i = 0; i < requiredEvents.Length; i++)
                {
                    Assert.That(catalog.TryGetDefinition(requiredEvents[i], out _), Is.True, requiredEvents[i].ToString());
                }
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void CandidateScoring_RanksProductionEventNamesAboveUnrelatedAssets()
        {
            Assert.That(
                VFXCandidateScoring.ScoreCandidate(VFXEventType.PrimaryFireMuzzle, "vfx_MuzzleFlash_01.prefab"),
                Is.GreaterThan(VFXCandidateScoring.ScoreCandidate(VFXEventType.PrimaryFireMuzzle, "CFX2_Blood.prefab")));
            Assert.That(
                VFXCandidateScoring.ScoreCandidate(VFXEventType.SpawnTelegraph, "FX_MagicCircle_Icearrow01.prefab"),
                Is.GreaterThan(VFXCandidateScoring.ScoreCandidate(VFXEventType.SpawnTelegraph, "vfx_Rain_01.prefab")));
            Assert.That(
                VFXCandidateScoring.ScoreCandidate(VFXEventType.ForwardCleaveHit, "CFXR4 Sword Hit PLAIN (Cross).prefab"),
                Is.GreaterThan(VFXCandidateScoring.ScoreCandidate(VFXEventType.ForwardCleaveHit, "vfx_LootDrop_01.prefab")));
        }

        [Test]
        public void CatalogMappings_WithNullPrefabsRemainLookupSafe()
        {
            var catalog = ScriptableObject.CreateInstance<VFXCatalog>();
            try
            {
                catalog.SetDefinitions(new[]
                {
                    new VFXDefinition(VFXEventType.DashImpact, null, 8, 1.1f)
                });

                Assert.That(catalog.TryGetDefinition(VFXEventType.DashImpact, out var definition), Is.True);
                Assert.That(definition.HasPrefab, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }
    }
}
