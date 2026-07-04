using UnityEngine;

namespace TapKnockout.Survivor
{
    [CreateAssetMenu(fileName = "ArenaConfig", menuName = "Tap Knockout/Survivor/Arena Config")]
    public sealed class ArenaConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string arenaId = "arena_prototype_01";

        [Header("Bounds")]
        [SerializeField] private Vector3 arenaCenter;
        [SerializeField, Min(1f)] private float arenaRadius = 24f;
        [SerializeField, Min(0f)] private float playerSafeSpawnRadius = 5f;

        [Header("Spawn Ring")]
        [SerializeField, Min(0f)] private float enemySpawnMinRadiusFromPlayer = 10f;
        [SerializeField, Min(0f)] private float enemySpawnMaxRadiusFromPlayer = 18f;
        [SerializeField, Min(1)] private int maxLiveEnemies = 80;

        [Header("Spawn Safety")]
        [SerializeField] private SpawnPressureMode spawnPressureMode = SpawnPressureMode.Mixed;
        [SerializeField, Min(0f)] private float playerAvoidSpawnRadius = 7f;
        [SerializeField, Range(0.1f, 1f)] private float edgeSpawnInnerRadiusFactor = 0.82f;
        [SerializeField, Range(0f, 1f)] private float mixedEdgePressureChance = 0.45f;
        [SerializeField, Range(1, 64)] private int spawnPositionRetries = 18;
        [SerializeField, Min(0f)] private float spawnClearanceRadius = 0.55f;
        [SerializeField] private LayerMask spawnBlockerLayers;
        [SerializeField] private bool fallbackToArenaEdgeWhenInvalid = true;

        public string ArenaId => arenaId;
        public Vector3 ArenaCenter => arenaCenter;
        public float ArenaRadius => arenaRadius;
        public float PlayerSafeSpawnRadius => playerSafeSpawnRadius;
        public float EnemySpawnMinRadiusFromPlayer => enemySpawnMinRadiusFromPlayer;
        public float EnemySpawnMaxRadiusFromPlayer => enemySpawnMaxRadiusFromPlayer;
        public int MaxLiveEnemies => maxLiveEnemies;
        public SpawnPressureMode SpawnPressureMode => spawnPressureMode;
        public float PlayerAvoidSpawnRadius => playerAvoidSpawnRadius;
        public float EdgeSpawnInnerRadiusFactor => edgeSpawnInnerRadiusFactor;
        public float MixedEdgePressureChance => mixedEdgePressureChance;
        public int SpawnPositionRetries => spawnPositionRetries;
        public float SpawnClearanceRadius => spawnClearanceRadius;
        public LayerMask SpawnBlockerLayers => spawnBlockerLayers;
        public bool FallbackToArenaEdgeWhenInvalid => fallbackToArenaEdgeWhenInvalid;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(arenaId))
            {
                arenaId = "arena_prototype_01";
            }

            arenaRadius = Mathf.Max(1f, arenaRadius);
            playerSafeSpawnRadius = Mathf.Clamp(playerSafeSpawnRadius, 0f, arenaRadius);
            enemySpawnMinRadiusFromPlayer = Mathf.Max(playerSafeSpawnRadius, enemySpawnMinRadiusFromPlayer);
            enemySpawnMaxRadiusFromPlayer = Mathf.Max(enemySpawnMinRadiusFromPlayer, enemySpawnMaxRadiusFromPlayer);
            maxLiveEnemies = Mathf.Max(1, maxLiveEnemies);
            playerAvoidSpawnRadius = Mathf.Clamp(Mathf.Max(playerSafeSpawnRadius, playerAvoidSpawnRadius), 0f, arenaRadius);
            edgeSpawnInnerRadiusFactor = Mathf.Clamp(edgeSpawnInnerRadiusFactor, 0.1f, 1f);
            mixedEdgePressureChance = Mathf.Clamp01(mixedEdgePressureChance);
            spawnPositionRetries = Mathf.Clamp(spawnPositionRetries, 1, 64);
            spawnClearanceRadius = Mathf.Max(0f, spawnClearanceRadius);
        }

        public Vector3 ClampToArena(Vector3 position)
        {
            var offset = position - arenaCenter;
            offset.y = 0f;
            if (offset.sqrMagnitude <= arenaRadius * arenaRadius)
            {
                return position;
            }

            var clamped = arenaCenter + offset.normalized * arenaRadius;
            clamped.y = position.y;
            return clamped;
        }

        public bool IsInsideArena(Vector3 position)
        {
            var offset = position - arenaCenter;
            offset.y = 0f;
            return offset.sqrMagnitude <= arenaRadius * arenaRadius;
        }
    }
}
