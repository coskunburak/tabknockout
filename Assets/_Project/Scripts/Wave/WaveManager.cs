using System;
using System.Collections;
using System.Collections.Generic;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Wave
{
    [DisallowMultipleComponent]
    public sealed class WaveManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private WaveConfig config;

        [Header("References")]
        [SerializeField] private EnemySpawner enemySpawner;

        [Header("Runtime")]
        [SerializeField] private bool runConfiguredWaveOnStart;

        [Header("Debug")]
        [SerializeField] private bool logLifecycle;

        private readonly List<GameObject> spawnedEnemies = new List<GameObject>();
        private readonly List<EnemyHealth> trackedEnemyHealth = new List<EnemyHealth>();
        private Coroutine runCoroutine;
        private bool hasSpawnedAllEnemies;
        private int currentWaveIndex;

        public event Action<WaveStartedEventArgs> OnWaveStarted;
        public event Action<WaveCompletedEventArgs> OnWaveCompleted;

        public bool IsRunning { get; private set; }
        public bool IsComplete { get; private set; }
        public WaveConfig CurrentWave => config;
        public IReadOnlyList<GameObject> SpawnedEnemies => spawnedEnemies;

        private void Reset()
        {
            enemySpawner = GetComponent<EnemySpawner>();
        }

        private void Awake()
        {
            if (enemySpawner == null)
            {
                enemySpawner = GetComponent<EnemySpawner>();
            }
        }

        private void Start()
        {
            if (runConfiguredWaveOnStart && config != null)
            {
                RunWave(config);
            }
        }

        private void Update()
        {
            if (IsRunning && hasSpawnedAllEnemies)
            {
                EvaluateCompletion();
            }
        }

        private void OnDisable()
        {
            SplitterEnemyController.OnAnySplitChildSpawned -= HandleSplitChildSpawned;
            StopCurrentWave(false);
        }

        private void OnEnable()
        {
            SplitterEnemyController.OnAnySplitChildSpawned -= HandleSplitChildSpawned;
            SplitterEnemyController.OnAnySplitChildSpawned += HandleSplitChildSpawned;
        }

        public void RunWave(WaveConfig waveConfig, int waveIndex = 0)
        {
            StopCurrentWave(false);

            config = waveConfig;
            currentWaveIndex = Mathf.Max(0, waveIndex);
            spawnedEnemies.Clear();
            trackedEnemyHealth.Clear();
            hasSpawnedAllEnemies = false;
            IsRunning = true;
            IsComplete = false;

            var startedArgs = new WaveStartedEventArgs(this, config, currentWaveIndex);
            OnWaveStarted?.Invoke(startedArgs);
            WaveEvents.RaiseWaveStarted(startedArgs);

            if (logLifecycle)
            {
                Debug.Log($"{nameof(WaveManager)} started wave {config?.WaveId ?? "<null>"}.", this);
            }

            runCoroutine = StartCoroutine(RunWaveRoutine());
        }

        public void StopCurrentWave(bool markComplete)
        {
            if (runCoroutine != null)
            {
                StopCoroutine(runCoroutine);
                runCoroutine = null;
            }

            UnsubscribeTrackedHealth();

            if (!markComplete)
            {
                IsRunning = false;
                enemySpawner?.ClearSpawnedEnemies();
                return;
            }

            CompleteWave();
        }

        public void ResetWaveState()
        {
            if (runCoroutine != null)
            {
                StopCoroutine(runCoroutine);
                runCoroutine = null;
            }

            UnsubscribeTrackedHealth();
            spawnedEnemies.Clear();
            enemySpawner?.ClearSpawnedEnemies();
            hasSpawnedAllEnemies = false;
            currentWaveIndex = 0;
            IsRunning = false;
            IsComplete = false;
        }

        public static bool AreSpawnedEnemiesDefeated(IReadOnlyList<GameObject> enemies)
        {
            if (enemies == null || enemies.Count == 0)
            {
                return true;
            }

            for (var i = 0; i < enemies.Count; i++)
            {
                if (IsEnemyBlockingClear(enemies[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private IEnumerator RunWaveRoutine()
        {
            if (config == null)
            {
                hasSpawnedAllEnemies = true;
                EvaluateCompletion();
                yield break;
            }

            if (config.StartDelay > 0f)
            {
                yield return new WaitForSeconds(config.StartDelay);
            }

            var spawnIndex = 0;
            var entries = config.Enemies;
            for (var entryIndex = 0; entryIndex < entries.Count; entryIndex++)
            {
                var entry = entries[entryIndex];
                if (entry == null)
                {
                    continue;
                }

                for (var countIndex = 0; countIndex < entry.Count; countIndex++)
                {
                    if (countIndex > 0 && entry.SpawnDelay > 0f)
                    {
                        yield return new WaitForSeconds(entry.SpawnDelay);
                    }

                    var spawned = enemySpawner != null ? enemySpawner.Spawn(entry, spawnIndex, config) : null;
                    TrackSpawnedEnemy(spawned);
                    spawnIndex++;
                }
            }

            hasSpawnedAllEnemies = true;
            EvaluateCompletion();
            runCoroutine = null;
        }

        private void TrackSpawnedEnemy(GameObject spawned)
        {
            if (spawned == null)
            {
                return;
            }

            if (spawnedEnemies.Contains(spawned))
            {
                return;
            }

            spawnedEnemies.Add(spawned);
            var enemyHealth = spawned.GetComponentInChildren<EnemyHealth>(true);
            if (enemyHealth == null)
            {
                return;
            }

            trackedEnemyHealth.Add(enemyHealth);
            enemyHealth.OnDied += HandleTrackedEnemyDied;
        }

        private void HandleTrackedEnemyDied(TapKnockout.Combat.HitContext hitContext)
        {
            EvaluateCompletion();
        }

        private void HandleSplitChildSpawned(GameObject spawnedChild)
        {
            if (!IsRunning || spawnedChild == null)
            {
                return;
            }

            TrackSpawnedEnemy(spawnedChild);
            EvaluateCompletion();
        }

        private void EvaluateCompletion()
        {
            if (!IsRunning || !hasSpawnedAllEnemies)
            {
                return;
            }

            if (config != null && config.CompleteWhenAllSpawnedEnemiesDead && !AreSpawnedEnemiesDefeated(spawnedEnemies))
            {
                return;
            }

            CompleteWave();
        }

        private void CompleteWave()
        {
            if (IsComplete)
            {
                return;
            }

            IsRunning = false;
            IsComplete = true;
            UnsubscribeTrackedHealth();

            var completedArgs = new WaveCompletedEventArgs(this, config, currentWaveIndex, spawnedEnemies.Count);
            OnWaveCompleted?.Invoke(completedArgs);
            WaveEvents.RaiseWaveCompleted(completedArgs);

            if (logLifecycle)
            {
                Debug.Log($"{nameof(WaveManager)} completed wave {config?.WaveId ?? "<null>"}.", this);
            }
        }

        private void UnsubscribeTrackedHealth()
        {
            for (var i = 0; i < trackedEnemyHealth.Count; i++)
            {
                if (trackedEnemyHealth[i] != null)
                {
                    trackedEnemyHealth[i].OnDied -= HandleTrackedEnemyDied;
                }
            }

            trackedEnemyHealth.Clear();
        }

        private static bool IsEnemyBlockingClear(GameObject enemy)
        {
            if (enemy == null)
            {
                return false;
            }

            var intro = enemy.GetComponentInChildren<EnemySpawnIntroController>(true);
            if (intro != null && intro.IsIntroRunning)
            {
                return true;
            }

            if (!enemy.activeInHierarchy)
            {
                return false;
            }

            var enemyHealth = enemy.GetComponentInChildren<EnemyHealth>(true);
            return enemyHealth == null || enemyHealth.IsAlive;
        }
    }
}
