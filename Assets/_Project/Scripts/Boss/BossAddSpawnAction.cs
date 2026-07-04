using System.Collections.Generic;
using TapKnockout.Enemy;
using TapKnockout.Wave;
using UnityEngine;

namespace TapKnockout.Boss
{
    [DisallowMultipleComponent]
    public sealed class BossAddSpawnAction : MonoBehaviour
    {
        [SerializeField] private BossConfig config;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private EnemyConfig fallbackAddConfig;
        [SerializeField] private GameObject fallbackAddPrefab;
        [SerializeField, Min(0)] private int fallbackMaxActiveAdds = 4;

        private readonly List<GameObject> activeAdds = new List<GameObject>();

        public int ActiveAddCount
        {
            get
            {
                CleanupInactiveAdds();
                return activeAdds.Count;
            }
        }

        private EnemyConfig AddConfig => config != null && config.AddEnemyConfig != null ? config.AddEnemyConfig : fallbackAddConfig;
        private GameObject AddPrefab => config != null && config.AddEnemyPrefab != null ? config.AddEnemyPrefab : fallbackAddPrefab;
        private int MaxActiveAdds => config != null ? config.MaxActiveAdds : fallbackMaxActiveAdds;

        private void Awake()
        {
            if (enemySpawner == null)
            {
                enemySpawner = GetComponentInParent<EnemySpawner>();
            }
        }

        public void Initialize(BossConfig bossConfig)
        {
            config = bossConfig;
        }

        public int Execute(BossAttackStep step)
        {
            CleanupInactiveAdds();
            var requestedCount = step.AddCount > 0 ? step.AddCount : 1;
            var availableSlots = Mathf.Max(0, MaxActiveAdds - activeAdds.Count);
            var spawnCount = Mathf.Min(requestedCount, availableSlots);
            if (spawnCount <= 0 || AddPrefab == null)
            {
                return 0;
            }

            for (var i = 0; i < spawnCount; i++)
            {
                SpawnAdd(i);
            }

            return spawnCount;
        }

        private void SpawnAdd(int index)
        {
            var spawnPoint = ResolveSpawnPoint(index);
            GameObject spawned;
            if (enemySpawner != null && AddConfig != null)
            {
                var entry = new WaveEnemyEntry(AddConfig, AddPrefab, 1, 0f, index);
                spawned = enemySpawner.Spawn(entry, index, null);
            }
            else
            {
                spawned = Instantiate(AddPrefab, spawnPoint.position, spawnPoint.rotation, transform.parent);
                var controller = spawned.GetComponentInChildren<EnemyController>(true);
                if (controller != null && AddConfig != null)
                {
                    controller.Initialize(AddConfig, null);
                }
            }

            if (spawned != null)
            {
                activeAdds.Add(spawned);
            }
        }

        private Transform ResolveSpawnPoint(int index)
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                var safeIndex = Mathf.Abs(index) % spawnPoints.Length;
                if (spawnPoints[safeIndex] != null)
                {
                    return spawnPoints[safeIndex];
                }
            }

            return transform;
        }

        private void CleanupInactiveAdds()
        {
            for (var i = activeAdds.Count - 1; i >= 0; i--)
            {
                var add = activeAdds[i];
                if (add == null || !add.activeInHierarchy || IsDead(add))
                {
                    activeAdds.RemoveAt(i);
                }
            }
        }

        private static bool IsDead(GameObject add)
        {
            var health = add != null ? add.GetComponentInChildren<EnemyHealth>(true) : null;
            return health != null && !health.IsAlive;
        }

        public static int CalculateSpawnCount(int requestedCount, int activeCount, int maxActive)
        {
            return Mathf.Min(Mathf.Max(0, requestedCount), Mathf.Max(0, maxActive - Mathf.Max(0, activeCount)));
        }
    }
}
