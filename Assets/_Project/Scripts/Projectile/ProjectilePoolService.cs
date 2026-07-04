using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Projectile
{
    [DisallowMultipleComponent]
    public sealed class ProjectilePoolService : MonoBehaviour
    {
        private static ProjectilePoolService sharedInstance;

        [SerializeField, Min(0)] private int defaultWarmupCount;

        private readonly Dictionary<GameObject, Stack<PooledProjectile>> poolByPrefab = new Dictionary<GameObject, Stack<PooledProjectile>>();
        private readonly HashSet<PooledProjectile> pooledProjectiles = new HashSet<PooledProjectile>();
        private readonly HashSet<PooledProjectile> activeProjectiles = new HashSet<PooledProjectile>();

        public int InactiveCount => pooledProjectiles.Count;
        public int ActiveCount => activeProjectiles.Count;
        public int PoolPrefabCount => poolByPrefab.Count;

        public static ProjectilePoolService Shared
        {
            get
            {
                if (sharedInstance != null)
                {
                    return sharedInstance;
                }

                sharedInstance = Object.FindFirstObjectByType<ProjectilePoolService>();
                if (sharedInstance != null)
                {
                    return sharedInstance;
                }

                var serviceObject = new GameObject("ProjectilePoolService");
                sharedInstance = serviceObject.AddComponent<ProjectilePoolService>();
                return sharedInstance;
            }
        }

        private void Awake()
        {
            if (sharedInstance != null && sharedInstance != this)
            {
                return;
            }

            sharedInstance = this;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform runtimeParent = null)
        {
            if (prefab == null)
            {
                return null;
            }

            Warmup(prefab, defaultWarmupCount);

            var projectile = GetOrCreate(prefab);
            projectile.PrepareForSpawn(position, rotation, runtimeParent);
            pooledProjectiles.Remove(projectile);
            activeProjectiles.Add(projectile);
            return projectile.gameObject;
        }

        public void Warmup(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0)
            {
                return;
            }

            var pool = GetPool(prefab);
            while (pool.Count < count)
            {
                var projectile = CreateProjectile(prefab);
                projectile.PrepareForPool(transform);
                pool.Push(projectile);
                pooledProjectiles.Add(projectile);
            }
        }

        public void Release(PooledProjectile projectile)
        {
            if (projectile == null || !projectile.IsConfigured)
            {
                return;
            }

            if (!pooledProjectiles.Add(projectile))
            {
                return;
            }

            activeProjectiles.Remove(projectile);
            projectile.PrepareForPool(transform);
            GetPool(projectile.PrefabKey).Push(projectile);
        }

        public int GetInactiveCount(GameObject prefab)
        {
            return prefab != null && poolByPrefab.TryGetValue(prefab, out var pool) ? pool.Count : 0;
        }

        private PooledProjectile GetOrCreate(GameObject prefab)
        {
            var pool = GetPool(prefab);
            while (pool.Count > 0)
            {
                var pooled = pool.Pop();
                pooledProjectiles.Remove(pooled);
                if (pooled != null)
                {
                    return pooled;
                }
            }

            return CreateProjectile(prefab);
        }

        private PooledProjectile CreateProjectile(GameObject prefab)
        {
            var instance = Instantiate(prefab, transform);
            var pooledProjectile = instance.GetComponent<PooledProjectile>();
            if (pooledProjectile == null)
            {
                pooledProjectile = instance.AddComponent<PooledProjectile>();
            }

            pooledProjectile.Configure(this, prefab);
            return pooledProjectile;
        }

        private Stack<PooledProjectile> GetPool(GameObject prefab)
        {
            if (!poolByPrefab.TryGetValue(prefab, out var pool))
            {
                pool = new Stack<PooledProjectile>();
                poolByPrefab[prefab] = pool;
            }

            return pool;
        }
    }
}
