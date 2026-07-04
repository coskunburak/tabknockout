using NUnit.Framework;
using TapKnockout.Enemy;
using TapKnockout.Wave;
using UnityEngine;

namespace TapKnockout.Wave.Tests
{
    public sealed class WaveConfigTests
    {
        [Test]
        public void DefaultWaveConfigValues_AreSafe()
        {
            var config = ScriptableObject.CreateInstance<WaveConfig>();

            try
            {
                Assert.That(config.WaveId, Is.Not.Empty);
                Assert.That(config.Enemies, Is.Not.Null);
                Assert.That(config.StartDelay, Is.GreaterThanOrEqualTo(0f));
                Assert.That(config.CompleteWhenAllSpawnedEnemiesDead, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void WaveEnemyEntry_ClampsCountDelayAndSpawnPointIndex()
        {
            var entry = new WaveEnemyEntry(null, null, -5, -1f, -4);

            Assert.That(entry.Count, Is.EqualTo(0));
            Assert.That(entry.SpawnDelay, Is.EqualTo(0f));
            Assert.That(entry.SpawnPointIndex, Is.EqualTo(-1));
        }

        [Test]
        public void AreSpawnedEnemiesDefeated_TreatsDeadEnemyHealthAsCleared()
        {
            var enemy = new GameObject("Enemy");

            try
            {
                var health = enemy.AddComponent<EnemyHealth>();
                health.ResetHealth();

                Assert.That(WaveManager.AreSpawnedEnemiesDefeated(new[] { enemy }), Is.False);

                health.ReceiveHit(new TapKnockout.Combat.HitContext(null, enemy, 999f));

                Assert.That(WaveManager.AreSpawnedEnemiesDefeated(new[] { enemy }), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void EnemySpawner_ClearSpawnedEnemies_DestroysTrackedSpawnedObjects()
        {
            var spawnerObject = new GameObject("Spawner");
            var prefab = new GameObject("EnemyPrefab");

            try
            {
                prefab.AddComponent<EnemyHealth>();
                var spawner = spawnerObject.AddComponent<EnemySpawner>();
                var entry = new WaveEnemyEntry(null, prefab, 1, 0f);

                var spawned = spawner.Spawn(entry, 0);

                Assert.That(spawned, Is.Not.Null);
                Assert.That(spawner.SpawnedEnemies.Count, Is.EqualTo(1));

                spawner.ClearSpawnedEnemies();

                Assert.That(spawned == null, Is.True);
                Assert.That(spawner.SpawnedEnemies.Count, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(prefab);
                Object.DestroyImmediate(spawnerObject);
            }
        }
    }
}
