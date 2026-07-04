using System.Collections.Generic;
using NUnit.Framework;
using TapKnockout.Ability;
using TapKnockout.Enemy;
using TapKnockout.Player;
using TapKnockout.Room;
using TapKnockout.Wave;
using UnityEditor;

namespace TapKnockout.Level.Tests
{
    public sealed class VerticalSliceChapterContentTests
    {
        private const string ChapterPath = "Assets/_Project/ScriptableObjects/Chapters/Chapter_VerticalSlice_01.asset";

        [Test]
        public void ChapterVerticalSlice01_HasTenDistinctRoomsWithBossFinale()
        {
            var chapter = LoadVerticalSliceChapterOrIgnore();

            Assert.That(chapter.Rooms, Is.Not.Null);
            Assert.That(chapter.Rooms.Count, Is.EqualTo(10));

            var distinctRooms = new HashSet<RoomTemplateConfig>();
            for (var i = 0; i < chapter.Rooms.Count; i++)
            {
                Assert.That(chapter.Rooms[i], Is.Not.Null, $"Room {i + 1} is not assigned.");
                Assert.That(distinctRooms.Add(chapter.Rooms[i]), Is.True, $"Room {i + 1} duplicates another RoomTemplateConfig.");
            }

            var finalRoom = chapter.Rooms[chapter.Rooms.Count - 1];
            Assert.That(finalRoom.IsBossRoom || finalRoom.RoomType == RoomType.Boss, Is.True);
            Assert.That(finalRoom.RewardType, Is.EqualTo(RoomRewardType.BossClear));
        }

        [Test]
        public void ChapterVerticalSlice01_AbilityRewardsAreConfiguredForFourChoices()
        {
            var chapter = LoadVerticalSliceChapterOrIgnore();
            var abilityRewardCount = 0;

            for (var i = 0; i < chapter.Rooms.Count; i++)
            {
                var room = chapter.Rooms[i];
                if (room.GrantsAbilityReward || room.RewardType == RoomRewardType.Ability)
                {
                    abilityRewardCount++;
                    var decision = RoomCompletionDecision.Evaluate(room, i, chapter.Rooms.Count);
                    Assert.That(decision.ShouldOpenAbilitySelection, Is.True, $"Room {i + 1} should open ability selection.");
                }
            }

            Assert.That(abilityRewardCount, Is.EqualTo(4));
        }

        [Test]
        public void ChapterVerticalSlice01_NoRewardRoomsDoNotOpenAbilitySelection()
        {
            var chapter = LoadVerticalSliceChapterOrIgnore();

            for (var i = 0; i < chapter.Rooms.Count - 1; i++)
            {
                var room = chapter.Rooms[i];
                if (room.RewardType != RoomRewardType.None)
                {
                    continue;
                }

                var decision = RoomCompletionDecision.Evaluate(room, i, chapter.Rooms.Count);
                Assert.That(decision.ShouldOpenAbilitySelection, Is.False, $"Room {i + 1} should not open ability selection.");
            }
        }

        [Test]
        public void ChapterVerticalSlice01_WavesHavePositiveSpawnCountsAndConfigs()
        {
            var chapter = LoadVerticalSliceChapterOrIgnore();

            for (var roomIndex = 0; roomIndex < chapter.Rooms.Count; roomIndex++)
            {
                var room = chapter.Rooms[roomIndex];
                Assert.That(room.Waves.Count, Is.GreaterThan(0), $"Room {roomIndex + 1} has no wave.");

                for (var waveIndex = 0; waveIndex < room.Waves.Count; waveIndex++)
                {
                    var wave = room.Waves[waveIndex];
                    Assert.That(wave, Is.Not.Null, $"Room {roomIndex + 1} wave {waveIndex + 1} is missing.");
                    Assert.That(wave.Enemies.Count, Is.GreaterThan(0), $"Wave {wave.WaveId} has no enemy entries.");

                    for (var entryIndex = 0; entryIndex < wave.Enemies.Count; entryIndex++)
                    {
                        var entry = wave.Enemies[entryIndex];
                        Assert.That(entry.EnemyConfig, Is.Not.Null, $"Wave {wave.WaveId} entry {entryIndex + 1} has no EnemyConfig.");
                        Assert.That(entry.EnemyPrefab, Is.Not.Null, $"Wave {wave.WaveId} entry {entryIndex + 1} has no EnemyPrefab.");
                        Assert.That(entry.Count, Is.GreaterThan(0), $"Wave {wave.WaveId} entry {entryIndex + 1} has no spawn count.");
                    }
                }
            }
        }

        [Test]
        public void PlaceholderEnemyConfigs_AreValidForVerticalSlice()
        {
            var ranged = Load<EnemyConfig>("Assets/_Project/ScriptableObjects/Enemies/EnemyConfig_RangedPlaceholder.asset");
            var elite = Load<EnemyConfig>("Assets/_Project/ScriptableObjects/Enemies/EnemyConfig_ElitePlaceholder.asset");
            var boss = Load<EnemyConfig>("Assets/_Project/ScriptableObjects/Enemies/EnemyConfig_BossPlaceholder.asset");

            if (ranged == null || elite == null || boss == null)
            {
                Assert.Ignore("Run Tools > Tap Knockout > Content > Create Vertical Slice Chapter Content before validating placeholder enemy content.");
            }

            Assert.That(ranged.MaxHealth, Is.GreaterThan(0f));
            Assert.That(ranged.AttackRange, Is.GreaterThan(1f));
            Assert.That(elite.MaxHealth, Is.GreaterThan(ranged.MaxHealth));
            Assert.That(elite.KnockbackResistance, Is.GreaterThan(ranged.KnockbackResistance));
            Assert.That(boss.MaxHealth, Is.GreaterThan(elite.MaxHealth));
            Assert.That(boss.KnockbackResistance, Is.GreaterThanOrEqualTo(elite.KnockbackResistance));
        }

        [Test]
        public void VerticalSliceEarlyCombat_IsTunedForFlowValidation()
        {
            var player = Load<PlayerConfig>("Assets/_Project/ScriptableObjects/Player/PlayerConfig_Default.asset");
            var melee = Load<EnemyConfig>("Assets/_Project/ScriptableObjects/Enemies/EnemyConfig_MeleeChaser.asset");
            var ranged = Load<EnemyConfig>("Assets/_Project/ScriptableObjects/Enemies/EnemyConfig_RangedPlaceholder.asset");

            if (player == null || melee == null || ranged == null)
            {
                Assert.Ignore("Run Tools > Tap Knockout > Content > Create Vertical Slice Chapter Content before validating vertical slice balance.");
            }

            Assert.That(player.MaxHealth, Is.GreaterThanOrEqualTo(180f));
            Assert.That(player.ContactDamageInvulnerabilityWindow, Is.GreaterThanOrEqualTo(0.4f));
            Assert.That(melee.ContactDamage, Is.LessThanOrEqualTo(4f));
            Assert.That(ranged.ContactDamage, Is.LessThanOrEqualTo(3f));
        }

        [Test]
        public void SupportedVerticalSliceAbilityAssets_AreRealRuntimeEffects()
        {
            var dashKnockback = Load<AbilityDefinition>("Assets/_Project/ScriptableObjects/Abilities/Ability_DashKnockbackUp.asset");
            var moveSpeed = Load<AbilityDefinition>("Assets/_Project/ScriptableObjects/Abilities/Ability_MoveSpeedUp.asset");
            var projectileSpeed = Load<AbilityDefinition>("Assets/_Project/ScriptableObjects/Abilities/Ability_ProjectileSpeedUp.asset");

            if (dashKnockback == null || moveSpeed == null || projectileSpeed == null)
            {
                Assert.Ignore("Run Tools > Tap Knockout > Content > Create Vertical Slice Chapter Content before validating generated ability assets.");
            }

            Assert.That(dashKnockback.EffectType, Is.EqualTo(AbilityEffectType.DashKnockbackUp));
            Assert.That(moveSpeed.EffectType, Is.EqualTo(AbilityEffectType.MoveSpeedUp));
            Assert.That(projectileSpeed.EffectType, Is.EqualTo(AbilityEffectType.ProjectileSpeedUp));
            Assert.That(dashKnockback.IsEnabled, Is.True);
            Assert.That(moveSpeed.IsEnabled, Is.True);
            Assert.That(projectileSpeed.IsEnabled, Is.True);
        }

        [Test]
        public void BossClearDecision_CompletesVerticalSlice()
        {
            var chapter = LoadVerticalSliceChapterOrIgnore();
            var finalRoom = chapter.Rooms[chapter.Rooms.Count - 1];

            var decision = RoomCompletionDecision.Evaluate(finalRoom, chapter.Rooms.Count - 1, chapter.Rooms.Count);

            Assert.That(decision.ShouldCompleteChapter, Is.True);
            Assert.That(decision.ShouldOpenAbilitySelection, Is.False);
            Assert.That(decision.ShouldWaitForContinue, Is.False);
        }

        private static ChapterConfig LoadVerticalSliceChapterOrIgnore()
        {
            var chapter = Load<ChapterConfig>(ChapterPath);
            if (chapter == null)
            {
                Assert.Ignore("Run Tools > Tap Knockout > Content > Create Vertical Slice Chapter Content before validating generated vertical slice assets.");
            }

            return chapter;
        }

        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
