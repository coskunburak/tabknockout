using System;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Wave
{
    public readonly struct EnemySpawnedEventArgs
    {
        public EnemySpawnedEventArgs(GameObject enemy, EnemyConfig enemyConfig, WaveConfig waveConfig, WaveEnemyEntry entry, int spawnIndex)
        {
            Enemy = enemy;
            EnemyConfig = enemyConfig;
            WaveConfig = waveConfig;
            Entry = entry;
            SpawnIndex = Mathf.Max(0, spawnIndex);
        }

        public GameObject Enemy { get; }
        public EnemyConfig EnemyConfig { get; }
        public WaveConfig WaveConfig { get; }
        public WaveEnemyEntry Entry { get; }
        public int SpawnIndex { get; }
    }

    public readonly struct WaveStartedEventArgs
    {
        public WaveStartedEventArgs(WaveManager source, WaveConfig waveConfig, int waveIndex)
        {
            Source = source;
            WaveConfig = waveConfig;
            WaveIndex = Mathf.Max(0, waveIndex);
        }

        public WaveManager Source { get; }
        public WaveConfig WaveConfig { get; }
        public int WaveIndex { get; }
    }

    public readonly struct WaveCompletedEventArgs
    {
        public WaveCompletedEventArgs(WaveManager source, WaveConfig waveConfig, int waveIndex, int spawnedEnemyCount)
        {
            Source = source;
            WaveConfig = waveConfig;
            WaveIndex = Mathf.Max(0, waveIndex);
            SpawnedEnemyCount = Mathf.Max(0, spawnedEnemyCount);
        }

        public WaveManager Source { get; }
        public WaveConfig WaveConfig { get; }
        public int WaveIndex { get; }
        public int SpawnedEnemyCount { get; }
    }

    public static class WaveEvents
    {
        public static event Action<EnemySpawnedEventArgs> OnEnemySpawned;
        public static event Action<WaveStartedEventArgs> OnWaveStarted;
        public static event Action<WaveCompletedEventArgs> OnWaveCompleted;

        public static void RaiseEnemySpawned(EnemySpawnedEventArgs eventArgs)
        {
            OnEnemySpawned?.Invoke(eventArgs);
        }

        public static void RaiseWaveStarted(WaveStartedEventArgs eventArgs)
        {
            OnWaveStarted?.Invoke(eventArgs);
        }

        public static void RaiseWaveCompleted(WaveCompletedEventArgs eventArgs)
        {
            OnWaveCompleted?.Invoke(eventArgs);
        }
    }
}
