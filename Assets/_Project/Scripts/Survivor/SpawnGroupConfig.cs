using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Survivor
{
    [CreateAssetMenu(fileName = "SpawnGroupConfig", menuName = "Tap Knockout/Survivor/Spawn Group Config")]
    public sealed class SpawnGroupConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string groupId = "spawn_group_melee_chaser";

        [Header("Enemy")]
        [SerializeField] private EnemyConfig enemyConfig;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private bool elite;

        [Header("Spawn")]
        [SerializeField, Min(0f)] private float weight = 1f;
        [SerializeField, Min(1)] private int minCount = 1;
        [SerializeField, Min(1)] private int maxCount = 1;
        [SerializeField, Min(1)] private int spawnBurstCount = 1;
        [SerializeField, Min(1)] private int budgetCost = 1;

        public string GroupId => groupId;
        public EnemyConfig EnemyConfig => enemyConfig;
        public GameObject EnemyPrefab => enemyPrefab;
        public bool IsElite => elite;
        public float Weight => weight;
        public int MinCount => minCount;
        public int MaxCount => maxCount;
        public int SpawnBurstCount => spawnBurstCount;
        public int BudgetCost => budgetCost;
        public bool HasValidEnemy => enemyConfig != null && enemyPrefab != null;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                groupId = "spawn_group_melee_chaser";
            }

            weight = Mathf.Max(0f, weight);
            minCount = Mathf.Max(1, minCount);
            maxCount = Mathf.Max(minCount, maxCount);
            spawnBurstCount = Mathf.Clamp(spawnBurstCount, minCount, maxCount);
            budgetCost = Mathf.Max(1, budgetCost);
        }

        public int ResolveSpawnCount()
        {
            return Mathf.Clamp(Random.Range(minCount, maxCount + 1), minCount, maxCount);
        }
    }
}
