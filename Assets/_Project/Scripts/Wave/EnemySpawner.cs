using System;
using System.Collections.Generic;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Wave
{
    [DisallowMultipleComponent]
    public sealed class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn References")]
        [SerializeField] private Transform[] spawnPoints = Array.Empty<Transform>();
        [SerializeField] private Transform playerTarget;
        [SerializeField] private Transform spawnParent;

        [Header("Debug")]
        [SerializeField] private bool logSetupWarnings = true;

        private readonly List<GameObject> spawnedEnemies = new List<GameObject>();

        public event Action<EnemySpawnedEventArgs> OnEnemySpawned;

        public IReadOnlyList<GameObject> SpawnedEnemies => spawnedEnemies;
        public Transform PlayerTarget => playerTarget;
        public Transform SpawnParent => spawnParent;

        public void SetPlayerTarget(Transform target)
        {
            playerTarget = target;
        }

        public void SetSpawnPoints(Transform[] points)
        {
            spawnPoints = points ?? Array.Empty<Transform>();
        }

        public GameObject Spawn(WaveEnemyEntry entry, int spawnSequenceIndex, WaveConfig waveConfig = null)
        {
            if (entry == null || entry.EnemyPrefab == null)
            {
                if (logSetupWarnings)
                {
                    Debug.LogWarning($"{nameof(EnemySpawner)} on {name} skipped spawn because the wave entry or enemy prefab is missing.", this);
                }

                return null;
            }

            var spawnPoint = ResolveSpawnPoint(entry.SpawnPointIndex, spawnSequenceIndex);
            var parent = spawnParent != null ? spawnParent : transform;
            var spawned = Instantiate(entry.EnemyPrefab, spawnPoint.position, spawnPoint.rotation, parent);

            InitializeSpawnedEnemy(spawned, entry.EnemyConfig);
            spawnedEnemies.Add(spawned);

            var eventArgs = new EnemySpawnedEventArgs(spawned, entry.EnemyConfig, waveConfig, entry, spawnSequenceIndex);
            OnEnemySpawned?.Invoke(eventArgs);
            WaveEvents.RaiseEnemySpawned(eventArgs);
            return spawned;
        }

        public void ClearInactiveTrackedEnemies()
        {
            for (var i = spawnedEnemies.Count - 1; i >= 0; i--)
            {
                if (spawnedEnemies[i] == null || !spawnedEnemies[i].activeInHierarchy)
                {
                    spawnedEnemies.RemoveAt(i);
                }
            }
        }

        private Transform ResolveSpawnPoint(int requestedIndex, int spawnSequenceIndex)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                if (logSetupWarnings)
                {
                    Debug.LogWarning($"{nameof(EnemySpawner)} on {name} has no spawn points. Falling back to spawner transform.", this);
                }

                return transform;
            }

            var index = requestedIndex >= 0 ? requestedIndex : spawnSequenceIndex;
            if (index < 0 || index >= spawnPoints.Length || spawnPoints[index] == null)
            {
                if (logSetupWarnings && requestedIndex >= 0)
                {
                    Debug.LogWarning($"{nameof(EnemySpawner)} on {name} received invalid spawn point index {requestedIndex}. Cycling through available spawn points.", this);
                }

                index = Mathf.Abs(spawnSequenceIndex) % spawnPoints.Length;
            }

            return spawnPoints[index] != null ? spawnPoints[index] : transform;
        }

        private void InitializeSpawnedEnemy(GameObject spawned, EnemyConfig enemyConfig)
        {
            if (spawned == null)
            {
                return;
            }

            var controller = spawned.GetComponentInChildren<EnemyController>(true);
            if (controller != null)
            {
                controller.Initialize(enemyConfig, playerTarget);
                return;
            }

            var health = spawned.GetComponentInChildren<EnemyHealth>(true);
            health?.Initialize(enemyConfig);

            var movement = spawned.GetComponentInChildren<EnemyMovement>(true);
            movement?.Initialize(enemyConfig, playerTarget);

            var knockbackReceiver = spawned.GetComponentInChildren<KnockbackReceiver>(true);
            knockbackReceiver?.Initialize(enemyConfig);

            var attackController = spawned.GetComponentInChildren<EnemyAttackController>(true);
            attackController?.Initialize(enemyConfig, playerTarget);
        }
    }
}
