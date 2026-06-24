using System;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Wave
{
    [Serializable]
    public sealed class WaveEnemyEntry
    {
        [SerializeField] private EnemyConfig enemyConfig;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField, Min(0)] private int count = 1;
        [SerializeField, Min(0f)] private float spawnDelay = 0.25f;
        [SerializeField] private int spawnPointIndex = -1;

        public WaveEnemyEntry()
        {
        }

        public WaveEnemyEntry(EnemyConfig enemyConfig, GameObject enemyPrefab, int count, float spawnDelay, int spawnPointIndex = -1)
        {
            this.enemyConfig = enemyConfig;
            this.enemyPrefab = enemyPrefab;
            this.count = Mathf.Max(0, count);
            this.spawnDelay = Mathf.Max(0f, spawnDelay);
            this.spawnPointIndex = Mathf.Max(-1, spawnPointIndex);
        }

        public EnemyConfig EnemyConfig => enemyConfig;
        public GameObject EnemyPrefab => enemyPrefab;
        public int Count => count;
        public float SpawnDelay => spawnDelay;
        public int SpawnPointIndex => spawnPointIndex;
        public bool HasPrefab => enemyPrefab != null;

        public void Validate()
        {
            count = Mathf.Max(0, count);
            spawnDelay = Mathf.Max(0f, spawnDelay);
            spawnPointIndex = Mathf.Max(-1, spawnPointIndex);
        }
    }
}
