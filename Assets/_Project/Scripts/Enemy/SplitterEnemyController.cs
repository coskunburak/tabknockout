using System;
using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    public sealed class SplitterEnemyController : MonoBehaviour, IEnemyRuntimeConfigReceiver, IEnemyRuntimeTargetReceiver, IPoolLifecycle
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private Transform target;
        [SerializeField] private EnemyHealth health;
        [SerializeField] private EnemyConfig splitChildConfigOverride;
        [SerializeField, Min(0)] private int maxChildrenPerDeath = 6;
        [SerializeField, Min(0)] private int maxSplitDepth = 1;
        [SerializeField, Min(0f)] private float spawnRadius = 0.75f;

        private bool hasSplit;

        public static event Action<GameObject> OnAnySplitChildSpawned;
        public event Action<GameObject> OnSplitChildSpawned;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<EnemyHealth>();
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnDied -= HandleDied;
                health.OnDied += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDied -= HandleDied;
            }
        }

        public void Initialize(EnemyConfig enemyConfig, Transform runtimeTarget)
        {
            config = enemyConfig;
            target = runtimeTarget;
            hasSplit = false;
        }

        public void SetTarget(Transform runtimeTarget)
        {
            target = runtimeTarget;
        }

        public void ResetRuntimeState(bool clearTarget = true)
        {
            hasSplit = false;
            if (clearTarget)
            {
                target = null;
            }
        }

        public void OnBeforeSpawnFromPool()
        {
            ResetRuntimeState();
        }

        public void OnSpawnedFromPool()
        {
        }

        public void OnBeforeDespawnToPool()
        {
            ResetRuntimeState();
        }

        public void ResetForPool()
        {
            ResetRuntimeState();
        }

        private void HandleDied(HitContext hitContext)
        {
            TrySplit();
        }

        public int TrySplit()
        {
            if (hasSplit || config == null || config.SplitSpawnPrefab == null)
            {
                return 0;
            }

            var marker = GetComponent<SplitSpawnRuntimeMarker>();
            var currentDepth = marker != null ? marker.Depth : 0;
            var spawnCount = CalculateAllowedSpawnCount(config.SplitSpawnCount, maxChildrenPerDeath, currentDepth, maxSplitDepth);
            if (spawnCount <= 0)
            {
                return 0;
            }

            hasSplit = true;
            for (var i = 0; i < spawnCount; i++)
            {
                SpawnChild(i, spawnCount, currentDepth + 1);
            }

            return spawnCount;
        }

        private void SpawnChild(int index, int totalCount, int depth)
        {
            var angle = totalCount > 0 ? index * Mathf.PI * 2f / totalCount : 0f;
            var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spawnRadius;
            var child = Instantiate(config.SplitSpawnPrefab, transform.position + offset, transform.rotation, transform.parent);
            var marker = child.GetComponent<SplitSpawnRuntimeMarker>();
            if (marker == null)
            {
                marker = child.AddComponent<SplitSpawnRuntimeMarker>();
            }

            marker.SetDepth(depth);

            var childConfig = splitChildConfigOverride;
            var childController = child.GetComponentInChildren<EnemyController>(true);
            if (childController != null)
            {
                if (childConfig != null)
                {
                    childController.Initialize(childConfig, target);
                }
                else
                {
                    childController.SetTarget(target);
                }
            }

            OnSplitChildSpawned?.Invoke(child);
            OnAnySplitChildSpawned?.Invoke(child);
        }

        public static int CalculateAllowedSpawnCount(int requestedCount, int maxChildren, int currentDepth, int maxDepth)
        {
            if (currentDepth >= Mathf.Max(0, maxDepth))
            {
                return 0;
            }

            return Mathf.Clamp(requestedCount, 0, Mathf.Max(0, maxChildren));
        }
    }

}
