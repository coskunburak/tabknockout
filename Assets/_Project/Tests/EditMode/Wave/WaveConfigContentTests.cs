using NUnit.Framework;
using UnityEditor;

namespace TapKnockout.Wave.Tests
{
    public sealed class WaveConfigContentTests
    {
        [TestCase("Wave_VS_01_SmallMelee", "wave_vs_01_small_melee", 1, 3)]
        [TestCase("Wave_VS_02_MeleeGroup", "wave_vs_02_melee_group", 1, 5)]
        [TestCase("Wave_VS_03_MixedPressure", "wave_vs_03_mixed_pressure", 2, 5)]
        [TestCase("Wave_VS_04_ElitePlaceholder", "wave_vs_04_elite_placeholder", 1, 2)]
        [TestCase("Wave_VS_05_LightRecoveryCombat", "wave_vs_05_light_recovery_combat", 1, 3)]
        [TestCase("Wave_VS_06_CombatPressure", "wave_vs_06_combat_pressure", 2, 6)]
        [TestCase("Wave_VS_07_RangedPressure", "wave_vs_07_ranged_pressure", 2, 6)]
        [TestCase("Wave_VS_08_EliteAbility", "wave_vs_08_elite_ability", 2, 4)]
        [TestCase("Wave_VS_09_PreBossPressure", "wave_vs_09_pre_boss_pressure", 2, 8)]
        [TestCase("Wave_VS_10_BossPlaceholder", "wave_vs_10_boss_placeholder", 1, 1)]
        public void GeneratedWave_HasExpectedEnemyEntries(
            string assetName,
            string expectedWaveId,
            int expectedEntryCount,
            int expectedTotalEnemyCount)
        {
            var config = Load(assetName);

            Assert.That(config.WaveId, Is.EqualTo(expectedWaveId));
            Assert.That(config.CompleteWhenAllSpawnedEnemiesDead, Is.True);
            Assert.That(config.StartDelay, Is.GreaterThanOrEqualTo(0f));
            Assert.That(config.Enemies.Count, Is.EqualTo(expectedEntryCount));

            var totalCount = 0;
            for (var i = 0; i < config.Enemies.Count; i++)
            {
                Assert.That(config.Enemies[i].EnemyConfig, Is.Not.Null);
                Assert.That(config.Enemies[i].EnemyPrefab, Is.Not.Null);
                Assert.That(config.Enemies[i].Count, Is.GreaterThan(0));
                Assert.That(config.Enemies[i].SpawnDelay, Is.GreaterThanOrEqualTo(0f));
                Assert.That(config.Enemies[i].SpawnPointIndex, Is.GreaterThanOrEqualTo(-1));
                totalCount += config.Enemies[i].Count;
            }

            Assert.That(totalCount, Is.EqualTo(expectedTotalEnemyCount));
        }

        private static WaveConfig Load(string assetName)
        {
            var config = AssetDatabase.LoadAssetAtPath<WaveConfig>(
                "Assets/_Project/ScriptableObjects/Waves/" + assetName + ".asset");
            Assert.That(config, Is.Not.Null);
            return config;
        }
    }
}
