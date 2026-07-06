using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Survivor
{
    [DisallowMultipleComponent]
    public sealed class EnemyPoolService : MonoBehaviour
    {
        [SerializeField, Min(0)] private int defaultWarmupCount = 4;

        private readonly Dictionary<GameObject, Stack<PooledEnemy>> poolByPrefab = new Dictionary<GameObject, Stack<PooledEnemy>>();
        private readonly HashSet<PooledEnemy> pooledEnemies = new HashSet<PooledEnemy>();
        private readonly HashSet<PooledEnemy> activeEnemies = new HashSet<PooledEnemy>();

        public int InactiveCount => pooledEnemies.Count;
        public int ActiveCount => activeEnemies.Count;
        public int PoolPrefabCount => poolByPrefab.Count;

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform runtimeParent)
        {
            if (prefab == null)
            {
                return null;
            }

            Warmup(prefab, defaultWarmupCount, runtimeParent);

            var pooledEnemy = GetOrCreate(prefab, runtimeParent);
            pooledEnemy.PrepareForSpawn(position, rotation, runtimeParent);
            pooledEnemies.Remove(pooledEnemy);
            activeEnemies.Add(pooledEnemy);
            return pooledEnemy.gameObject;
        }

        public void Warmup(GameObject prefab, int count, Transform runtimeParent = null)
        {
            if (prefab == null || count <= 0)
            {
                return;
            }

            var pool = GetPool(prefab);
            while (pool.Count < count)
            {
                var pooledEnemy = CreateEnemy(prefab, runtimeParent);
                pooledEnemy.PrepareForPool(transform);
                pool.Push(pooledEnemy);
                pooledEnemies.Add(pooledEnemy);
            }
        }

        public void Release(PooledEnemy pooledEnemy)
        {
            if (pooledEnemy == null || !pooledEnemy.IsConfigured)
            {
                return;
            }

            if (!pooledEnemies.Add(pooledEnemy))
            {
                return;
            }

            activeEnemies.Remove(pooledEnemy);
            pooledEnemy.PrepareForPool(transform);
            GetPool(pooledEnemy.PrefabKey).Push(pooledEnemy);
        }

        private PooledEnemy GetOrCreate(GameObject prefab, Transform runtimeParent)
        {
            var pool = GetPool(prefab);
            while (pool.Count > 0)
            {
                var pooled = pool.Pop();
                pooledEnemies.Remove(pooled);
                if (pooled != null)
                {
                    return pooled;
                }
            }

            return CreateEnemy(prefab, runtimeParent);
        }

        public int GetInactiveCount(GameObject prefab)
        {
            return prefab != null && poolByPrefab.TryGetValue(prefab, out var pool) ? pool.Count : 0;
        }

        private PooledEnemy CreateEnemy(GameObject prefab, Transform runtimeParent)
        {
            var parent = runtimeParent != null ? runtimeParent : transform;
            var instance = Instantiate(prefab, parent);
            var pooledEnemy = instance.GetComponent<PooledEnemy>();
            if (pooledEnemy == null)
            {
                pooledEnemy = instance.AddComponent<PooledEnemy>();
            }

            pooledEnemy.Configure(this, prefab);
            return pooledEnemy;
        }

        private Stack<PooledEnemy> GetPool(GameObject prefab)
        {
            if (!poolByPrefab.TryGetValue(prefab, out var pool))
            {
                pool = new Stack<PooledEnemy>();
                poolByPrefab[prefab] = pool;
            }

            return pool;
        }
    }
}
