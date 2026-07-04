using TapKnockout.Projectile;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TapKnockout.Survivor
{
    [DisallowMultipleComponent]
    public sealed class SurvivorStressTestController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SurvivorSpawnDirector spawnDirector;
        [SerializeField] private EnemyPoolService enemyPoolService;
        [SerializeField] private ProjectilePoolService projectilePoolService;
        [SerializeField] private ArenaRunDirector runDirector;
        [SerializeField] private ArenaBossDirector bossDirector;
        [SerializeField] private SpawnGroupConfig stressSpawnGroup;

        [Header("Stress Spawn")]
        [SerializeField, Min(1)] private int stressEnemyCount = 100;
        [SerializeField] private bool ignoreLiveCaps = true;

        [Header("Runtime Counters")]
        [SerializeField] private int liveEnemies;
        [SerializeField] private int activePooledEnemies;
        [SerializeField] private int inactivePooledEnemies;
        [SerializeField] private int activeProjectiles;
        [SerializeField] private int inactiveProjectiles;
        [SerializeField] private float runTimeSeconds;
        [SerializeField] private string activeWave = "none";
        [SerializeField] private bool bossActive;
        [SerializeField] private int lastStressSpawnCount;

        public int LiveEnemies => liveEnemies;
        public int ActivePooledEnemies => activePooledEnemies;
        public int InactivePooledEnemies => inactivePooledEnemies;
        public int ActiveProjectiles => activeProjectiles;
        public int InactiveProjectiles => inactiveProjectiles;
        public float RunTimeSeconds => runTimeSeconds;
        public string ActiveWave => activeWave;
        public bool BossActive => bossActive;
        public int LastStressSpawnCount => lastStressSpawnCount;

        private void Reset()
        {
            ResolveReferences();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void Update()
        {
            RefreshCounters();
        }

        [ContextMenu("Spawn Stress Enemies")]
        public void SpawnStressEnemies()
        {
            ResolveReferences();
            lastStressSpawnCount = spawnDirector != null
                ? spawnDirector.SpawnDebugEnemies(stressSpawnGroup, stressEnemyCount, ignoreLiveCaps)
                : 0;
            RefreshCounters();
        }

        [ContextMenu("Clear Live Enemies To Pool")]
        public void ClearLiveEnemiesToPool()
        {
            spawnDirector?.ClearLiveEnemies(true);
            RefreshCounters();
        }

        public void SetStressSpawnGroup(SpawnGroupConfig spawnGroup)
        {
            stressSpawnGroup = spawnGroup;
        }

        private void ResolveReferences()
        {
            if (spawnDirector == null)
            {
                spawnDirector = GetComponent<SurvivorSpawnDirector>();
            }

            if (spawnDirector == null)
            {
                spawnDirector = Object.FindFirstObjectByType<SurvivorSpawnDirector>();
            }

            if (enemyPoolService == null && spawnDirector != null)
            {
                enemyPoolService = spawnDirector.EnemyPool != null
                    ? spawnDirector.EnemyPool
                    : spawnDirector.GetComponent<EnemyPoolService>();
            }

            if (projectilePoolService == null)
            {
                projectilePoolService = Object.FindFirstObjectByType<ProjectilePoolService>();
            }

            if (runDirector == null)
            {
                runDirector = Object.FindFirstObjectByType<ArenaRunDirector>();
            }

            if (bossDirector == null)
            {
                bossDirector = Object.FindFirstObjectByType<ArenaBossDirector>();
            }
        }

        private void RefreshCounters()
        {
            ResolveReferences();
            liveEnemies = spawnDirector != null ? spawnDirector.LiveEnemyCount : 0;
            activePooledEnemies = enemyPoolService != null ? enemyPoolService.ActiveCount : 0;
            inactivePooledEnemies = enemyPoolService != null ? enemyPoolService.InactiveCount : 0;
            activeProjectiles = projectilePoolService != null ? projectilePoolService.ActiveCount : 0;
            inactiveProjectiles = projectilePoolService != null ? projectilePoolService.InactiveCount : 0;
            runTimeSeconds = runDirector != null ? runDirector.RunTimer.ElapsedSeconds : 0f;
            activeWave = spawnDirector != null ? spawnDirector.DebugActiveWave : "none";
            bossActive = bossDirector != null && bossDirector.HasActiveBoss;
        }
    }
}
