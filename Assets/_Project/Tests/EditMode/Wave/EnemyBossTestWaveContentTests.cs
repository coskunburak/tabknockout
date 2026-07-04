using NUnit.Framework;
using UnityEditor;

namespace TapKnockout.Wave.Tests
{
    public sealed class EnemyBossTestWaveContentTests
    {
        private static readonly string[] WavePaths =
        {
            "Assets/_Project/ScriptableObjects/Waves/Wave_Test_MeleeChaser.asset",
            "Assets/_Project/ScriptableObjects/Waves/Wave_Test_FastCharger.asset",
            "Assets/_Project/ScriptableObjects/Waves/Wave_Test_RangedShooter.asset",
            "Assets/_Project/ScriptableObjects/Waves/Wave_Test_AreaBomber.asset",
            "Assets/_Project/ScriptableObjects/Waves/Wave_Test_ShieldEnemy.asset",
            "Assets/_Project/ScriptableObjects/Waves/Wave_Test_SplitterEnemy.asset",
            "Assets/_Project/ScriptableObjects/Waves/Wave_Test_EliteChaser.asset",
            "Assets/_Project/ScriptableObjects/Waves/Wave_Test_EliteRanged.asset",
            "Assets/_Project/ScriptableObjects/Waves/Wave_Test_Boss1_DashCounterBrute.asset"
        };

        [Test]
        public void Phase4TestWaves_HaveConfigPrefabAndPositiveCount_WhenGenerated()
        {
            for (var i = 0; i < WavePaths.Length; i++)
            {
                var wave = AssetDatabase.LoadAssetAtPath<WaveConfig>(WavePaths[i]);

                Assert.That(wave, Is.Not.Null, $"Missing wave at {WavePaths[i]}.");
                Assert.That(wave.Enemies, Is.Not.Empty, $"{WavePaths[i]} has no enemy entries.");
                Assert.That(wave.Enemies[0].EnemyConfig, Is.Not.Null, $"{WavePaths[i]} missing enemy config.");
                Assert.That(wave.Enemies[0].EnemyPrefab, Is.Not.Null, $"{WavePaths[i]} missing enemy prefab.");
                Assert.That(wave.Enemies[0].Count, Is.GreaterThan(0), $"{WavePaths[i]} has non-positive count.");
            }
        }
    }
}
