using System;
using System.Collections;
using System.Collections.Generic;
using TapKnockout.Combat;
using TapKnockout.Enemy;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Survivor
{
    public readonly struct SurvivorEnemyKilledEvent
    {
        public SurvivorEnemyKilledEvent(GameObject enemy, EnemyConfig enemyConfig, Vector3 position, HitContext killingHit)
        {
            Enemy = enemy;
            EnemyConfig = enemyConfig;
            Position = position;
            KillingHit = killingHit;
        }

        public GameObject Enemy { get; }
        public EnemyConfig EnemyConfig { get; }
        public Vector3 Position { get; }
        public HitContext KillingHit { get; }
    }

    [DisallowMultipleComponent]
    public sealed class SurvivorSpawnDirector : MonoBehaviour
    {
        [Header("Configs")]
        [SerializeField] private ArenaConfig arenaConfig;
        [SerializeField] private WaveTimelineConfig waveTimeline;

        [Header("References")]
        [SerializeField] private Transform playerTarget;
        [SerializeField] private Transform spawnRoot;
        [SerializeField] private EnemyPoolService enemyPoolService;

        [Header("Runtime")]
        [SerializeField] private bool spawnEnabled;
        [SerializeField] private bool usePooling = true;
        [SerializeField, Min(0)] private int enemyPoolWarmupPerPrefab = 4;
        [SerializeField] private bool logSpawnWarnings = true;

        [Header("Spawn Placement")]
        [SerializeField] private bool snapSpawnToGround = true;
        [SerializeField] private LayerMask spawnGroundLayers = ~0;
        [SerializeField, Min(0f)] private float spawnGroundRaycastHeight = 8f;
        [SerializeField, Min(0f)] private float spawnGroundRaycastDistance = 32f;
        [SerializeField, Min(0f)] private float spawnGroundClearance = 0.03f;
        [SerializeField] private bool disableSpawnedEnemyGravity = true;

        [Header("Spawn Telegraph")]
        [SerializeField] private bool enableSpawnTelegraph = true;
        [SerializeField, Range(0f, 2f)] private float spawnTelegraphDuration = 0.45f;
        [SerializeField] private GameObject spawnTelegraphPrefab;
        [SerializeField, Min(0.05f)] private float spawnTelegraphRadius = 0.85f;
        [SerializeField] private Color spawnTelegraphColor = new Color(1f, 0.42f, 0.08f, 0.85f);
        [SerializeField, Min(0)] private int maxConcurrentSpawnTelegraphs = 12;

        [Header("Live Budget")]
        [SerializeField, Min(1)] private int baseLiveEnemyBudget = 24;
        [SerializeField, Min(1)] private int maxLiveEnemyBudget = 120;
        [SerializeField, Min(0f)] private float liveEnemyBudgetRampPerMinute = 8f;

        [Header("Debug")]
        [SerializeField] private float debugLastElapsedSeconds;
        [SerializeField] private int debugLiveEnemyCount;
        [SerializeField] private int debugLiveBudget;
        [SerializeField] private int debugPendingSpawnCount;
        [SerializeField] private int debugLiveEnemyCap;
        [SerializeField] private int debugLiveBudgetCap;
        [SerializeField] private string debugActiveWave = "none";
        [SerializeField] private string debugLastSpawnGroup = "none";
        [SerializeField] private int debugTotalSpawned;

        private readonly List<GameObject> liveEnemies = new List<GameObject>(128);
        private readonly List<PendingSpawn> pendingSpawns = new List<PendingSpawn>(32);
        private readonly Queue<SpawnTelegraphMarker> inactiveTelegraphMarkers = new Queue<SpawnTelegraphMarker>();
        private readonly Queue<GameObject> inactiveTelegraphPrefabInstances = new Queue<GameObject>();
        private readonly Dictionary<GameObject, GameObject> prefabByInstance = new Dictionary<GameObject, GameObject>();
        private readonly Dictionary<GameObject, int> budgetByInstance = new Dictionary<GameObject, int>();
        private readonly HashSet<GameObject> releaseQueued = new HashSet<GameObject>();
        private float spawnTimer;
        private bool loggedMissingConfig;
        private bool loggedMissingPlayer;

        private sealed class PendingSpawn
        {
            public SpawnGroupConfig Group;
            public Vector3 Position;
            public float Remaining;
            public int BudgetCost;
            public bool UsesPrefabVisual;
            public GameObject VisualObject;
            public SpawnTelegraphMarker Marker;
        }

        public event Action<GameObject> OnEnemySpawned;
        public event Action<SurvivorEnemyKilledEvent> OnEnemyKilled;
        public static event Action<GameObject> OnAnyEnemySpawned;

        public IReadOnlyList<GameObject> LiveEnemies => liveEnemies;
        public string DebugActiveWave => debugActiveWave;
        public string DebugLastSpawnGroup => debugLastSpawnGroup;
        public int DebugTotalSpawned => debugTotalSpawned;
        public bool SpawnEnabled => spawnEnabled;
        public EnemyPoolService EnemyPool => enemyPoolService;
        public int PendingSpawnCount => pendingSpawns.Count;
        public int LiveEnemyCount
        {
            get
            {
                CleanLiveEnemyList();
                return liveEnemies.Count;
            }
        }

        public int CurrentLiveBudget
        {
            get
            {
                CleanLiveEnemyList();
                var total = 0;
                for (var i = 0; i < liveEnemies.Count; i++)
                {
                    if (budgetByInstance.TryGetValue(liveEnemies[i], out var budget))
                    {
                        total += budget;
                    }
                }

                return total;
            }
        }

        private int CurrentPendingBudget
        {
            get
            {
                var total = 0;
                for (var i = 0; i < pendingSpawns.Count; i++)
                {
                    total += Mathf.Max(1, pendingSpawns[i].BudgetCost);
                }

                return total;
            }
        }

        private void OnEnable()
        {
            CombatEvents.OnEntityKilled -= HandleEntityKilled;
            CombatEvents.OnEntityKilled += HandleEntityKilled;
        }

        private void OnDisable()
        {
            CombatEvents.OnEntityKilled -= HandleEntityKilled;
        }

        private void OnValidate()
        {
            spawnGroundRaycastHeight = Mathf.Max(0f, spawnGroundRaycastHeight);
            spawnGroundRaycastDistance = Mathf.Max(0f, spawnGroundRaycastDistance);
            spawnGroundClearance = Mathf.Max(0f, spawnGroundClearance);
            spawnTelegraphDuration = Mathf.Clamp(spawnTelegraphDuration, 0f, 2f);
            spawnTelegraphRadius = Mathf.Max(0.05f, spawnTelegraphRadius);
            maxConcurrentSpawnTelegraphs = Mathf.Max(0, maxConcurrentSpawnTelegraphs);
            baseLiveEnemyBudget = Mathf.Max(1, baseLiveEnemyBudget);
            maxLiveEnemyBudget = Mathf.Max(baseLiveEnemyBudget, maxLiveEnemyBudget);
            liveEnemyBudgetRampPerMinute = Mathf.Max(0f, liveEnemyBudgetRampPerMinute);
        }

        public void Configure(ArenaConfig arena, WaveTimelineConfig timeline, Transform player)
        {
            arenaConfig = arena;
            waveTimeline = timeline;
            playerTarget = player != null ? player : ResolvePlayerTarget();
            spawnTimer = 0f;
        }

        public void StartSpawning()
        {
            spawnEnabled = true;
            spawnTimer = 0f;
            ResolvePlayerTarget();
            ClearPendingSpawns();
        }

        public void StopSpawning()
        {
            spawnEnabled = false;
            ClearPendingSpawns();
        }

        public void Tick(float deltaTime, float elapsedSeconds, int liveEnemyCapOverride = -1)
        {
            debugLastElapsedSeconds = elapsedSeconds;
            debugLiveEnemyCount = LiveEnemyCount;
            debugLiveBudget = CurrentLiveBudget;
            debugPendingSpawnCount = pendingSpawns.Count;
            ResolvePlayerTarget();

            if (!spawnEnabled)
            {
                debugActiveWave = "spawn disabled";
                return;
            }

            TickPendingSpawns(deltaTime);
            CleanLiveEnemyList();
            debugLiveEnemyCount = liveEnemies.Count;
            debugLiveBudget = CurrentLiveBudget;

            if (!CanSpawn())
            {
                return;
            }

            var activeEntry = waveTimeline.GetActiveEntry(elapsedSeconds);
            if (activeEntry == null)
            {
                debugActiveWave = "none";
                return;
            }

            debugActiveWave = $"{activeEntry.StartTime:0.##}-{activeEntry.EndTime:0.##}";
            spawnTimer -= Mathf.Max(0f, deltaTime);
            if (spawnTimer > 0f)
            {
                return;
            }

            spawnTimer = Mathf.Max(0.05f, activeEntry.SpawnInterval);
            var liveCap = ResolveLiveEnemyCap(activeEntry, liveEnemyCapOverride);
            var liveBudgetCap = ResolveLiveBudgetCap(activeEntry, elapsedSeconds, liveCap);
            debugLiveEnemyCap = liveCap;
            debugLiveBudgetCap = liveBudgetCap;

            if (liveEnemies.Count + pendingSpawns.Count >= liveCap)
            {
                return;
            }

            var group = SelectSpawnGroup(activeEntry);
            if (group == null)
            {
                debugLastSpawnGroup = "no valid group";
                return;
            }

            debugLastSpawnGroup = group.GroupId;
            var requestedSpawnCount = Mathf.Max(group.ResolveSpawnCount(), group.SpawnBurstCount);
            var remainingLiveSlots = Mathf.Max(0, liveCap - liveEnemies.Count - pendingSpawns.Count);
            var remainingBudget = Mathf.Max(0, liveBudgetCap - CurrentLiveBudget - CurrentPendingBudget);
            var budgetLimitedCount = group.BudgetCost > 0 ? remainingBudget / group.BudgetCost : remainingLiveSlots;
            var spawnCount = Mathf.Min(Mathf.Min(requestedSpawnCount, remainingLiveSlots), budgetLimitedCount);
            SpawnGroup(group, spawnCount, liveCap, liveBudgetCap);
        }

        public GameObject SpawnBoss(SpawnGroupConfig bossGroup)
        {
            if (bossGroup == null || !bossGroup.HasValidEnemy)
            {
                if (logSpawnWarnings)
                {
                    Debug.LogWarning($"{nameof(SurvivorSpawnDirector)} cannot spawn boss because no valid boss spawn group is assigned.", this);
                }

                return null;
            }

            return SpawnOne(bossGroup, ResolveSpawnPosition());
        }

        public int SpawnDebugEnemies(SpawnGroupConfig group, int count, bool ignoreLiveCaps = true)
        {
            if (group == null || !group.HasValidEnemy || count <= 0)
            {
                return 0;
            }

            var spawned = 0;
            for (var i = 0; i < count; i++)
            {
                if (!ignoreLiveCaps)
                {
                    var liveCap = arenaConfig != null ? arenaConfig.MaxLiveEnemies : count;
                    if (LiveEnemyCount >= liveCap)
                    {
                        break;
                    }
                }

                if (SpawnOne(group, ResolveSpawnPosition()) != null)
                {
                    spawned++;
                }
            }

            return spawned;
        }

        public void ClearLiveEnemies(bool returnToPool = true)
        {
            ClearPendingSpawns();

            for (var i = liveEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = liveEnemies[i];
                if (enemy == null)
                {
                    continue;
                }

                if (returnToPool && usePooling)
                {
                    ReturnToPool(enemy);
                }
                else
                {
                    Destroy(enemy);
                }
            }

            liveEnemies.Clear();
            budgetByInstance.Clear();
            prefabByInstance.Clear();
            releaseQueued.Clear();
        }

        private bool CanSpawn()
        {
            if (arenaConfig == null || waveTimeline == null)
            {
                if (logSpawnWarnings && !loggedMissingConfig)
                {
                    loggedMissingConfig = true;
                    Debug.LogWarning($"{nameof(SurvivorSpawnDirector)} needs ArenaConfig and WaveTimelineConfig before spawning.", this);
                }

                return false;
            }

            if (ResolvePlayerTarget() == null)
            {
                if (logSpawnWarnings && !loggedMissingPlayer)
                {
                    loggedMissingPlayer = true;
                    Debug.LogWarning($"{nameof(SurvivorSpawnDirector)} needs a player target before spawning.", this);
                }

                return false;
            }

            return true;
        }

        private int ResolveLiveEnemyCap(WaveTimelineConfig.WaveTimelineEntry activeEntry, int overrideCap)
        {
            var cap = activeEntry != null ? activeEntry.LiveEnemyCap : arenaConfig.MaxLiveEnemies;
            cap = Mathf.Min(cap, arenaConfig.MaxLiveEnemies);
            if (overrideCap > 0)
            {
                cap = Mathf.Min(cap, overrideCap);
            }

            return Mathf.Max(1, cap);
        }

        private int ResolveLiveBudgetCap(WaveTimelineConfig.WaveTimelineEntry activeEntry, float elapsedSeconds, int liveEnemyCap)
        {
            var baseBudget = Mathf.Max(baseLiveEnemyBudget, liveEnemyCap);
            var intensity = activeEntry != null ? Mathf.Max(0.1f, activeEntry.IntensityMultiplier) : 1f;
            var ramp = Mathf.FloorToInt(Mathf.Max(0f, elapsedSeconds) / 60f * liveEnemyBudgetRampPerMinute);
            var budget = Mathf.RoundToInt(baseBudget * intensity) + ramp;
            return Mathf.Clamp(budget, 1, Mathf.Max(maxLiveEnemyBudget, baseBudget));
        }

        private SpawnGroupConfig SelectSpawnGroup(WaveTimelineConfig.WaveTimelineEntry activeEntry)
        {
            var groups = activeEntry.SpawnGroups;
            var totalWeight = 0f;
            for (var i = 0; i < groups.Count; i++)
            {
                if (groups[i] != null && groups[i].HasValidEnemy)
                {
                    totalWeight += Mathf.Max(0f, groups[i].Weight);
                }
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            var roll = UnityEngine.Random.value * totalWeight;
            var cursor = 0f;
            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                if (group == null || !group.HasValidEnemy)
                {
                    continue;
                }

                cursor += Mathf.Max(0f, group.Weight);
                if (roll <= cursor)
                {
                    return group;
                }
            }

            return null;
        }

        private void SpawnGroup(SpawnGroupConfig group, int requestedCount, int liveEnemyCap, int liveBudgetCap)
        {
            for (var i = 0; i < requestedCount; i++)
            {
                if (!CanReserveSpawn(group, liveEnemyCap, liveBudgetCap))
                {
                    return;
                }

                var position = ResolveSpawnPosition();
                if (!TryQueueTelegraphedSpawn(group, position))
                {
                    SpawnOne(group, position);
                }
            }
        }

        private bool CanReserveSpawn(SpawnGroupConfig group, int liveEnemyCap, int liveBudgetCap)
        {
            if (group == null)
            {
                return false;
            }

            if (liveEnemies.Count + pendingSpawns.Count >= liveEnemyCap)
            {
                return false;
            }

            return CurrentLiveBudget + CurrentPendingBudget + group.BudgetCost <= liveBudgetCap;
        }

        private bool TryQueueTelegraphedSpawn(SpawnGroupConfig group, Vector3 position)
        {
            if (!enableSpawnTelegraph ||
                spawnTelegraphDuration <= 0f ||
                maxConcurrentSpawnTelegraphs <= 0 ||
                pendingSpawns.Count >= maxConcurrentSpawnTelegraphs)
            {
                return false;
            }

            var pending = new PendingSpawn
            {
                Group = group,
                Position = position,
                Remaining = spawnTelegraphDuration,
                BudgetCost = group != null ? group.BudgetCost : 1
            };

            AttachTelegraphVisual(pending);
            pendingSpawns.Add(pending);
            debugPendingSpawnCount = pendingSpawns.Count;
            return true;
        }

        private void TickPendingSpawns(float deltaTime)
        {
            deltaTime = Mathf.Max(0f, deltaTime);
            for (var i = pendingSpawns.Count - 1; i >= 0; i--)
            {
                var pending = pendingSpawns[i];
                if (pending == null)
                {
                    pendingSpawns.RemoveAt(i);
                    continue;
                }

                pending.Remaining = Mathf.Max(0f, pending.Remaining - deltaTime);
                pending.Marker?.Tick(deltaTime);
                if (pending.Remaining > 0f)
                {
                    continue;
                }

                var group = pending.Group;
                var position = pending.Position;
                ReleaseTelegraphVisual(pending);
                pendingSpawns.RemoveAt(i);
                SpawnOne(group, position);
            }

            debugPendingSpawnCount = pendingSpawns.Count;
        }

        private void ClearPendingSpawns()
        {
            for (var i = pendingSpawns.Count - 1; i >= 0; i--)
            {
                if (pendingSpawns[i] != null)
                {
                    ReleaseTelegraphVisual(pendingSpawns[i]);
                }
            }

            pendingSpawns.Clear();
            debugPendingSpawnCount = 0;
        }

        private void AttachTelegraphVisual(PendingSpawn pending)
        {
            if (pending == null)
            {
                return;
            }

            var visualPosition = ResolveTelegraphVisualPosition(pending.Position);
            if (spawnTelegraphPrefab != null)
            {
                var visual = GetTelegraphPrefabInstance();
                pending.VisualObject = visual;
                pending.UsesPrefabVisual = true;
                pending.Marker = visual != null ? visual.GetComponent<SpawnTelegraphMarker>() : null;

                if (visual != null)
                {
                    visual.transform.SetPositionAndRotation(visualPosition, Quaternion.identity);
                    visual.transform.localScale = Vector3.one * Mathf.Max(0.05f, spawnTelegraphRadius * 2f);
                    visual.SetActive(true);
                }

                if (pending.Marker != null)
                {
                    pending.Marker.Play(visualPosition, spawnTelegraphRadius, spawnTelegraphDuration, spawnTelegraphColor);
                }

                return;
            }

            pending.Marker = GetTelegraphMarker();
            pending.Marker.Play(visualPosition, spawnTelegraphRadius, spawnTelegraphDuration, spawnTelegraphColor);
        }

        private Vector3 ResolveTelegraphVisualPosition(Vector3 position)
        {
            if (!snapSpawnToGround || spawnGroundRaycastDistance <= 0f)
            {
                return position;
            }

            var origin = position + Vector3.up * spawnGroundRaycastHeight;
            var maxDistance = spawnGroundRaycastHeight + spawnGroundRaycastDistance;
            if (Physics.Raycast(origin, Vector3.down, out var hit, maxDistance, spawnGroundLayers, QueryTriggerInteraction.Ignore))
            {
                position.y = hit.point.y + Mathf.Max(0.01f, spawnGroundClearance);
                return position;
            }

            if (arenaConfig != null)
            {
                position.y = arenaConfig.ArenaCenter.y + Mathf.Max(0.01f, spawnGroundClearance);
            }

            return position;
        }

        private GameObject GetTelegraphPrefabInstance()
        {
            while (inactiveTelegraphPrefabInstances.Count > 0)
            {
                var instance = inactiveTelegraphPrefabInstances.Dequeue();
                if (instance != null)
                {
                    return instance;
                }
            }

            return Instantiate(spawnTelegraphPrefab, spawnRoot != null ? spawnRoot : transform);
        }

        private SpawnTelegraphMarker GetTelegraphMarker()
        {
            while (inactiveTelegraphMarkers.Count > 0)
            {
                var marker = inactiveTelegraphMarkers.Dequeue();
                if (marker != null)
                {
                    return marker;
                }
            }

            var markerObject = new GameObject("RuntimeSpawnTelegraphMarker");
            markerObject.transform.SetParent(spawnRoot != null ? spawnRoot : transform, false);
            return markerObject.AddComponent<SpawnTelegraphMarker>();
        }

        private void ReleaseTelegraphVisual(PendingSpawn pending)
        {
            if (pending == null)
            {
                return;
            }

            if (pending.Marker != null)
            {
                pending.Marker.StopAndHide();
            }

            if (pending.UsesPrefabVisual)
            {
                if (pending.VisualObject != null)
                {
                    pending.VisualObject.SetActive(false);
                    inactiveTelegraphPrefabInstances.Enqueue(pending.VisualObject);
                }

                return;
            }

            if (pending.Marker != null)
            {
                inactiveTelegraphMarkers.Enqueue(pending.Marker);
            }
        }

        private GameObject SpawnOne(SpawnGroupConfig group, Vector3 spawnPosition)
        {
            if (group == null || !group.HasValidEnemy)
            {
                return null;
            }

            var enemy = GetEnemyInstance(group.EnemyPrefab);
            if (enemy == null)
            {
                return null;
            }

            var resolvedPlayerTarget = ResolvePlayerTarget();
            spawnPosition = EnemySpawnPlacement.ResolveGroundedPosition(
                enemy,
                spawnPosition,
                arenaConfig != null ? arenaConfig.ArenaCenter.y : spawnPosition.y,
                snapSpawnToGround,
                spawnGroundLayers,
                spawnGroundRaycastHeight,
                spawnGroundRaycastDistance,
                spawnGroundClearance,
                resolvedPlayerTarget);

            enemy.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            EnemySpawnPlacement.PrepareRigidbodyForArenaSpawn(enemy, disableSpawnedEnemyGravity);
            enemy.SetActive(true);
            ReEnableCommonEnemyBehaviours(enemy);

            if (enemy.TryGetComponent<EnemyController>(out var enemyController))
            {
                enemyController.Initialize(group.EnemyConfig, resolvedPlayerTarget);
            }
            else if (enemy.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                enemyHealth.Initialize(group.EnemyConfig);
            }

            var pooledEnemy = enemy.GetComponent<PooledEnemy>();
            pooledEnemy?.NotifySpawned();

            liveEnemies.Add(enemy);
            prefabByInstance[enemy] = group.EnemyPrefab;
            budgetByInstance[enemy] = group.BudgetCost;
            releaseQueued.Remove(enemy);
            debugTotalSpawned++;
            OnEnemySpawned?.Invoke(enemy);
            OnAnyEnemySpawned?.Invoke(enemy);
            return enemy;
        }

        private GameObject GetEnemyInstance(GameObject prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            if (usePooling)
            {
                var poolService = ResolveEnemyPoolService();
                if (enemyPoolWarmupPerPrefab > 0)
                {
                    poolService.Warmup(prefab, enemyPoolWarmupPerPrefab, spawnRoot != null ? spawnRoot : transform);
                }

                return poolService.Spawn(prefab, Vector3.zero, Quaternion.identity, spawnRoot != null ? spawnRoot : transform);
            }

            var parent = spawnRoot != null ? spawnRoot : transform;
            return Instantiate(prefab, parent);
        }

        private Vector3 ResolveSpawnPosition()
        {
            var resolvedPlayerTarget = ResolvePlayerTarget();
            if (arenaConfig == null)
            {
                return resolvedPlayerTarget != null ? resolvedPlayerTarget.position : transform.position;
            }

            var playerPosition = resolvedPlayerTarget != null ? resolvedPlayerTarget.position : arenaConfig.ArenaCenter;
            var minRadius = ResolveMinimumSpawnDistance();
            var maxRadius = Mathf.Max(minRadius, arenaConfig.EnemySpawnMaxRadiusFromPlayer);
            var attempts = Mathf.Max(1, arenaConfig.SpawnPositionRetries);
            var mode = arenaConfig.SpawnPressureMode;

            for (var attempt = 0; attempt < attempts; attempt++)
            {
                var candidate = GenerateSpawnCandidate(mode, playerPosition, minRadius, maxRadius);
                candidate = arenaConfig.ClampToArena(candidate);
                if (IsSpawnPositionValid(candidate, playerPosition, minRadius))
                {
                    return candidate;
                }
            }

            return ResolveFallbackSpawnPosition(playerPosition, minRadius);
        }

        private float ResolveMinimumSpawnDistance()
        {
            if (arenaConfig == null)
            {
                return 0f;
            }

            return Mathf.Max(
                arenaConfig.PlayerSafeSpawnRadius,
                arenaConfig.EnemySpawnMinRadiusFromPlayer,
                arenaConfig.PlayerAvoidSpawnRadius);
        }

        private Vector3 GenerateSpawnCandidate(SpawnPressureMode mode, Vector3 playerPosition, float minRadius, float maxRadius)
        {
            if (mode == SpawnPressureMode.Mixed)
            {
                mode = UnityEngine.Random.value < arenaConfig.MixedEdgePressureChance
                    ? SpawnPressureMode.EdgePressure
                    : SpawnPressureMode.RingAroundPlayer;
            }

            switch (mode)
            {
                case SpawnPressureMode.EdgePressure:
                    return GenerateEdgePressureCandidate(playerPosition);
                case SpawnPressureMode.RandomWithinArena:
                    return GenerateRandomArenaCandidate(playerPosition);
                default:
                    return GenerateRingCandidate(playerPosition, minRadius, maxRadius);
            }
        }

        private Vector3 GenerateRingCandidate(Vector3 playerPosition, float minRadius, float maxRadius)
        {
            var direction2D = RandomDirection2D();
            var radius = UnityEngine.Random.Range(minRadius, Mathf.Max(minRadius, maxRadius));
            return playerPosition + new Vector3(direction2D.x, 0f, direction2D.y) * radius;
        }

        private Vector3 GenerateEdgePressureCandidate(Vector3 playerPosition)
        {
            var direction2D = RandomDirection2D();
            var innerRadius = arenaConfig.ArenaRadius * arenaConfig.EdgeSpawnInnerRadiusFactor;
            var outerRadius = arenaConfig.ArenaRadius * 0.98f;
            var radius = UnityEngine.Random.Range(Mathf.Min(innerRadius, outerRadius), Mathf.Max(innerRadius, outerRadius));
            var candidate = arenaConfig.ArenaCenter + new Vector3(direction2D.x, 0f, direction2D.y) * radius;
            candidate.y = playerPosition.y;
            return candidate;
        }

        private Vector3 GenerateRandomArenaCandidate(Vector3 playerPosition)
        {
            var direction2D = RandomDirection2D();
            var radius = Mathf.Sqrt(UnityEngine.Random.value) * arenaConfig.ArenaRadius;
            var candidate = arenaConfig.ArenaCenter + new Vector3(direction2D.x, 0f, direction2D.y) * radius;
            candidate.y = playerPosition.y;
            return candidate;
        }

        private Vector3 ResolveFallbackSpawnPosition(Vector3 playerPosition, float minRadius)
        {
            if (arenaConfig != null && arenaConfig.FallbackToArenaEdgeWhenInvalid)
            {
                var awayFromPlayer = arenaConfig.ArenaCenter - playerPosition;
                awayFromPlayer.y = 0f;
                if (awayFromPlayer.sqrMagnitude <= 0.0001f)
                {
                    awayFromPlayer = Vector3.forward;
                }

                var edgeRadius = arenaConfig.ArenaRadius * 0.96f;
                var edgeFallback = arenaConfig.ArenaCenter + awayFromPlayer.normalized * edgeRadius;
                edgeFallback.y = playerPosition.y;
                if (IsSpawnPositionValid(edgeFallback, playerPosition, minRadius))
                {
                    return edgeFallback;
                }
            }

            var fallbackDirection = Vector3.forward;
            var fallback = playerPosition + fallbackDirection * minRadius;
            fallback = arenaConfig != null ? arenaConfig.ClampToArena(fallback) : fallback;
            if (arenaConfig != null && !IsSpawnPositionValid(fallback, playerPosition, minRadius))
            {
                var bestCandidate = fallback;
                var bestDistanceSqr = HorizontalDistanceSqr(bestCandidate, playerPosition);
                var attempts = Mathf.Max(4, arenaConfig.SpawnPositionRetries);
                for (var i = 0; i < attempts; i++)
                {
                    var candidate = arenaConfig.ClampToArena(GenerateEdgePressureCandidate(playerPosition));
                    var distanceSqr = HorizontalDistanceSqr(candidate, playerPosition);
                    if (distanceSqr > bestDistanceSqr)
                    {
                        bestCandidate = candidate;
                        bestDistanceSqr = distanceSqr;
                    }

                    if (IsSpawnPositionValid(candidate, playerPosition, minRadius))
                    {
                        return candidate;
                    }
                }

                fallback = bestCandidate;
            }

            return fallback;
        }

        private bool IsSpawnPositionValid(Vector3 candidate, Vector3 playerPosition, float minRadius)
        {
            if (arenaConfig != null && !arenaConfig.IsInsideArena(candidate))
            {
                return false;
            }

            var toPlayer = candidate - playerPosition;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < minRadius * minRadius)
            {
                return false;
            }

            if (arenaConfig == null ||
                arenaConfig.SpawnClearanceRadius <= 0f ||
                arenaConfig.SpawnBlockerLayers.value == 0)
            {
                return true;
            }

            var checkCenter = candidate + Vector3.up * Mathf.Max(0.05f, spawnGroundClearance + 0.5f);
            return !Physics.CheckSphere(
                checkCenter,
                arenaConfig.SpawnClearanceRadius,
                arenaConfig.SpawnBlockerLayers,
                QueryTriggerInteraction.Ignore);
        }

        private static float HorizontalDistanceSqr(Vector3 a, Vector3 b)
        {
            var offset = a - b;
            offset.y = 0f;
            return offset.sqrMagnitude;
        }

        private static Vector2 RandomDirection2D()
        {
            var direction = UnityEngine.Random.insideUnitCircle;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Vector2.up;
            }

            return direction.normalized;
        }

        private void HandleEntityKilled(EntityKilledEvent entityKilledEvent)
        {
            var enemy = entityKilledEvent.Entity;
            if (enemy == null || !liveEnemies.Contains(enemy))
            {
                return;
            }

            var enemyConfig = ResolveEnemyConfig(enemy);
            OnEnemyKilled?.Invoke(new SurvivorEnemyKilledEvent(
                enemy,
                enemyConfig,
                enemy.transform.position,
                entityKilledEvent.KillingHit));

            if (releaseQueued.Add(enemy))
            {
                var delay = enemyConfig != null ? enemyConfig.DeathDelay : 0f;
                StartCoroutine(ReleaseEnemyAfterDelay(enemy, delay));
            }
        }

        private IEnumerator ReleaseEnemyAfterDelay(GameObject enemy, float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            ReturnToPool(enemy);
        }

        private void ReturnToPool(GameObject enemy)
        {
            if (enemy == null)
            {
                return;
            }

            liveEnemies.Remove(enemy);
            budgetByInstance.Remove(enemy);
            releaseQueued.Remove(enemy);

            var hasPrefab = prefabByInstance.TryGetValue(enemy, out var prefab);
            prefabByInstance.Remove(enemy);

            if (!usePooling || !hasPrefab || prefab == null)
            {
                Destroy(enemy);
                return;
            }

            var pooledEnemy = enemy.GetComponent<PooledEnemy>();
            if (pooledEnemy != null && pooledEnemy.IsConfigured)
            {
                pooledEnemy.ReleaseToPool();
                return;
            }

            enemy.SetActive(false);
        }

        private EnemyPoolService ResolveEnemyPoolService()
        {
            if (enemyPoolService != null)
            {
                return enemyPoolService;
            }

            enemyPoolService = GetComponent<EnemyPoolService>();
            if (enemyPoolService == null)
            {
                enemyPoolService = gameObject.AddComponent<EnemyPoolService>();
            }

            return enemyPoolService;
        }

        private void CleanLiveEnemyList()
        {
            for (var i = liveEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = liveEnemies[i];
                if (enemy == null || !enemy.activeInHierarchy && !releaseQueued.Contains(enemy))
                {
                    liveEnemies.RemoveAt(i);
                    if (enemy != null)
                    {
                        budgetByInstance.Remove(enemy);
                    }
                }
            }
        }

        private static EnemyConfig ResolveEnemyConfig(GameObject enemy)
        {
            if (enemy == null)
            {
                return null;
            }

            if (enemy.TryGetComponent<EnemyController>(out var controller) && controller.Config != null)
            {
                return controller.Config;
            }

            return enemy.TryGetComponent<EnemyHealth>(out var health) ? health.Config : null;
        }

        private Transform ResolvePlayerTarget()
        {
            if (IsUsablePlayerTarget(playerTarget))
            {
                return playerTarget;
            }

            var playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null && playerHealth.gameObject.activeInHierarchy)
            {
                playerTarget = playerHealth.transform;
                loggedMissingPlayer = false;
                return playerTarget;
            }

            GameObject taggedPlayer = null;
            try
            {
                taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            }
            catch (UnityException)
            {
                return null;
            }

            if (taggedPlayer == null || !taggedPlayer.activeInHierarchy)
            {
                return null;
            }

            playerTarget = taggedPlayer.transform;
            loggedMissingPlayer = false;
            return playerTarget;
        }

        private static bool IsUsablePlayerTarget(Transform candidate)
        {
            return candidate != null && candidate.gameObject.activeInHierarchy;
        }

        private static void ReEnableCommonEnemyBehaviours(GameObject enemy)
        {
            var behaviours = enemy.GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is EnemyMovement || behaviours[i] is EnemyAttackController)
                {
                    behaviours[i].enabled = true;
                }
            }
        }
    }
}
