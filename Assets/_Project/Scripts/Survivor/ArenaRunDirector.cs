using System;
using System.Collections;
using System.Collections.Generic;
using TapKnockout.Ability;
using TapKnockout.Combat;
using TapKnockout.Enemy;
using TapKnockout.Pickups;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Survivor
{
    public readonly struct SurvivorRunStateChangedEventArgs
    {
        public SurvivorRunStateChangedEventArgs(SurvivorRunState previousState, SurvivorRunState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }

        public SurvivorRunState PreviousState { get; }
        public SurvivorRunState NewState { get; }
    }

    public readonly struct SurvivorRunSummary
    {
        public SurvivorRunSummary(SurvivorRunState resultState, float elapsedSeconds, int playerLevel, int enemiesKilled)
        {
            ResultState = resultState;
            ElapsedSeconds = elapsedSeconds;
            PlayerLevel = playerLevel;
            EnemiesKilled = enemiesKilled;
        }

        public SurvivorRunState ResultState { get; }
        public float ElapsedSeconds { get; }
        public int PlayerLevel { get; }
        public int EnemiesKilled { get; }
    }

    [DisallowMultipleComponent]
    public sealed class ArenaRunDirector : MonoBehaviour
    {
        [Header("Configs")]
        [SerializeField] private RunConfig runConfig;
        [SerializeField] private ArenaConfig arenaConfigOverride;
        [SerializeField] private WaveTimelineConfig waveTimelineOverride;

        [Header("References")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerXPController xpController;
        [SerializeField] private PickupCollector pickupCollector;
        [SerializeField] private AbilitySelectionController abilitySelectionController;
        [SerializeField] private SurvivorSpawnDirector spawnDirector;
        [SerializeField] private ArenaBossDirector bossDirector;

        [Header("XP")]
        [SerializeField] private XPOrb xpOrbPrefab;
        [SerializeField] private Transform pickupRoot;
        [SerializeField] private bool grantXPDirectlyWhenNoOrbPrefab = true;

        [Header("Runtime")]
        [SerializeField] private bool autoStartOnStart = true;
        [SerializeField] private bool pauseTimeScaleOnLevelUp = true;
        [SerializeField] private bool logPrototypeWarnings = true;

        private readonly SurvivorRunTimer runTimer = new SurvivorRunTimer();
        private readonly Stack<XPOrb> xpOrbPool = new Stack<XPOrb>();
        private readonly HashSet<XPOrb> xpOrbsInPool = new HashSet<XPOrb>();
        private readonly HashSet<GameObject> xpRewardedEnemies = new HashSet<GameObject>();
        private SurvivorRunState state = SurvivorRunState.NotStarted;
        private SurvivorRunState stateBeforeLevelUp = SurvivorRunState.Running;
        private SurvivorRunSummary lastSummary;
        private GameObject activeBoss;
        private bool bossWarningRaised;
        private bool bossSpawnRaised;
        private bool hasSubscribed;
        private bool hasLoggedDirectXPFallback;
        private bool isManualPaused;
        private float previousTimeScale = 1f;
        private int enemiesKilled;
        private Coroutine resumeLevelUpCoroutine;

        public event Action<SurvivorRunStateChangedEventArgs> OnRunStateChanged;
        public event Action<SurvivorRunSummary> OnRunEnded;
        public event Action<float> OnBossWarning;

        public SurvivorRunState State => state;
        public SurvivorRunTimer RunTimer => runTimer;
        public SurvivorRunSummary LastSummary => lastSummary;
        public bool IsRunning => state == SurvivorRunState.Running || state == SurvivorRunState.BossActive;
        public bool IsManualPaused => isManualPaused;
        public int EnemiesKilled => enemiesKilled;

        private void Reset()
        {
            spawnDirector = GetComponent<SurvivorSpawnDirector>();
            bossDirector = GetComponent<ArenaBossDirector>();
        }

        private void Start()
        {
            if (autoStartOnStart)
            {
                StartRun();
            }
        }

        private void OnDisable()
        {
            UnsubscribeRuntimeEvents();
            RestoreLevelUpPauseIfNeeded();
            RestoreManualPauseIfNeeded();
        }

        private void Update()
        {
            if (!IsRunning || isManualPaused)
            {
                return;
            }

            var deltaTime = Time.deltaTime;
            runTimer.Tick(deltaTime);

            var liveCap = ResolveCurrentLiveEnemyCap();
            spawnDirector?.Tick(deltaTime, runTimer.ElapsedSeconds, liveCap);

            TickBossMilestones();

            if (runTimer.IsComplete && state != SurvivorRunState.Victory && state != SurvivorRunState.Defeat)
            {
                EndRun(SurvivorRunState.Victory);
            }
        }

        public void StartRun()
        {
            ResolveReferences();
            ApplyRunConfig();
            SubscribeRuntimeEvents();

            enemiesKilled = 0;
            bossWarningRaised = false;
            bossSpawnRaised = false;
            activeBoss = null;
            hasLoggedDirectXPFallback = false;
            isManualPaused = false;
            xpRewardedEnemies.Clear();
            runTimer.Reset();
            xpController?.ResetProgression();

            bossDirector?.ClearBoss();
            bossDirector?.Configure(spawnDirector);
            spawnDirector?.StartSpawning();
            SetState(SurvivorRunState.Running);
        }

        public void EndRun(SurvivorRunState resultState)
        {
            if (state == SurvivorRunState.Victory || state == SurvivorRunState.Defeat)
            {
                return;
            }

            if (resultState != SurvivorRunState.Victory && resultState != SurvivorRunState.Defeat)
            {
                throw new ArgumentOutOfRangeException(nameof(resultState), resultState, "Run can only end as Victory or Defeat.");
            }

            RestoreManualPauseIfNeeded();
            spawnDirector?.StopSpawning();
            SetState(resultState);

            lastSummary = new SurvivorRunSummary(
                resultState,
                runTimer.ElapsedSeconds,
                xpController != null ? xpController.Level : 1,
                enemiesKilled);
            OnRunEnded?.Invoke(lastSummary);
        }

        public void ToggleManualPause()
        {
            SetManualPause(!isManualPaused);
        }

        public void SetManualPause(bool paused)
        {
            if (!IsRunning || isManualPaused == paused)
            {
                return;
            }

            isManualPaused = paused;
            if (paused)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = previousTimeScale;
            }
        }

        private void ResolveReferences()
        {
            if (spawnDirector == null)
            {
                spawnDirector = GetComponent<SurvivorSpawnDirector>();
            }

            if (bossDirector == null)
            {
                bossDirector = GetComponent<ArenaBossDirector>();
            }

            if (playerTransform == null && playerHealth != null)
            {
                playerTransform = playerHealth.transform;
            }

            if (playerHealth == null && playerTransform != null)
            {
                playerHealth = playerTransform.GetComponent<PlayerHealth>();
            }

            if (xpController == null && playerTransform != null)
            {
                xpController = playerTransform.GetComponent<PlayerXPController>();
            }

            if (pickupCollector == null && playerTransform != null)
            {
                pickupCollector = playerTransform.GetComponent<PickupCollector>();
            }
        }

        private void ApplyRunConfig()
        {
            var arenaConfig = arenaConfigOverride != null ? arenaConfigOverride : runConfig != null ? runConfig.ArenaConfig : null;
            var timeline = waveTimelineOverride != null ? waveTimelineOverride : runConfig != null ? runConfig.WaveTimeline : null;
            var duration = runConfig != null ? runConfig.TargetRunDurationSeconds : 600f;
            runTimer.Configure(duration);

            if (xpController != null && runConfig != null)
            {
                xpController.SetXPCurve(CopyXPRequirements(runConfig.XPRequirementsPerLevel));
            }

            if (abilitySelectionController != null &&
                runConfig != null &&
                runConfig.StartingAbilityPool != null &&
                runConfig.StartingAbilityPool.Count > 0)
            {
                abilitySelectionController.SetAbilityPool(runConfig.StartingAbilityPool);
            }

            spawnDirector?.Configure(arenaConfig, timeline, playerTransform);
        }

        private void SubscribeRuntimeEvents()
        {
            if (hasSubscribed)
            {
                return;
            }

            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied += HandlePlayerDied;
            }

            if (xpController != null)
            {
                xpController.OnLevelUp += HandlePlayerLevelUp;
            }

            if (abilitySelectionController != null)
            {
                abilitySelectionController.OnAbilitySelected += HandleAbilitySelected;
            }

            if (bossDirector != null)
            {
                bossDirector.OnBossDefeated += HandleBossDefeated;
            }

            if (spawnDirector != null)
            {
                spawnDirector.OnEnemySpawned += HandleEnemySpawned;
            }

            CombatEvents.OnEntityKilled += HandleEntityKilled;
            hasSubscribed = true;
        }

        private void UnsubscribeRuntimeEvents()
        {
            if (!hasSubscribed)
            {
                return;
            }

            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied -= HandlePlayerDied;
            }

            if (xpController != null)
            {
                xpController.OnLevelUp -= HandlePlayerLevelUp;
            }

            if (abilitySelectionController != null)
            {
                abilitySelectionController.OnAbilitySelected -= HandleAbilitySelected;
            }

            if (bossDirector != null)
            {
                bossDirector.OnBossDefeated -= HandleBossDefeated;
            }

            if (spawnDirector != null)
            {
                spawnDirector.OnEnemySpawned -= HandleEnemySpawned;
            }

            CombatEvents.OnEntityKilled -= HandleEntityKilled;
            hasSubscribed = false;
        }

        private void TickBossMilestones()
        {
            if (runConfig == null)
            {
                return;
            }

            var warningTime = ResolveBossWarningTime();
            if (!bossWarningRaised && runTimer.ElapsedSeconds >= warningTime)
            {
                bossWarningRaised = true;
                OnBossWarning?.Invoke(Mathf.Max(0f, runConfig.BossSpawnTimeSeconds - runTimer.ElapsedSeconds));
                bossDirector?.BeginBossWarning(
                    runConfig.BossSpawnGroup,
                    Mathf.Max(0f, runConfig.BossSpawnTimeSeconds - runTimer.ElapsedSeconds));
            }

            if (bossSpawnRaised || runTimer.ElapsedSeconds < runConfig.BossSpawnTimeSeconds)
            {
                return;
            }

            bossSpawnRaised = true;
            SetState(SurvivorRunState.BossActive);

            if (bossDirector != null &&
                runConfig.BossSpawnGroup != null &&
                bossDirector.TryStartBossEncounter(runConfig.BossSpawnGroup))
            {
                activeBoss = bossDirector.ActiveBoss;
            }
            else if (runConfig.BossSpawnGroup != null && spawnDirector != null)
            {
                activeBoss = spawnDirector.SpawnBoss(runConfig.BossSpawnGroup);
            }
            else if (logPrototypeWarnings)
            {
                Debug.Log($"{nameof(ArenaRunDirector)} reached boss milestone but no boss spawn group is assigned yet. This is a prototype TODO.", this);
            }
        }

        private void HandleBossDefeated(GameObject boss)
        {
            if (bossDirector != null &&
                !bossDirector.TriggerRunVictoryOnBossDefeated)
            {
                return;
            }

            if (state != SurvivorRunState.Defeat && state != SurvivorRunState.Victory)
            {
                EndRun(SurvivorRunState.Victory);
            }
        }

        private float ResolveBossWarningTime()
        {
            var timeline = waveTimelineOverride != null ? waveTimelineOverride : runConfig.WaveTimeline;
            if (timeline != null)
            {
                return Mathf.Min(timeline.BossWarningTimeSeconds, runConfig.BossSpawnTimeSeconds);
            }

            return Mathf.Max(0f, runConfig.BossSpawnTimeSeconds - 30f);
        }

        private int ResolveCurrentLiveEnemyCap()
        {
            if (runConfig == null)
            {
                return -1;
            }

            var difficulty = runConfig.EvaluateDifficultyMultiplier(runTimer.ElapsedSeconds);
            return Mathf.Clamp(
                Mathf.RoundToInt(runConfig.StartingEnemyCap * difficulty),
                runConfig.StartingEnemyCap,
                runConfig.MaxEnemyCap);
        }

        private void HandlePlayerDied(HitContext killingHit)
        {
            if (state != SurvivorRunState.Defeat && state != SurvivorRunState.Victory)
            {
                EndRun(SurvivorRunState.Defeat);
            }
        }

        private void HandlePlayerLevelUp(PlayerLevelUpEventArgs eventArgs)
        {
            if (!IsRunning)
            {
                return;
            }

            stateBeforeLevelUp = state;
            SetState(SurvivorRunState.LevelUpPaused);

            if (pauseTimeScaleOnLevelUp)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }

            if (abilitySelectionController == null)
            {
                ResumeFromLevelUp();
                return;
            }

            var hasOfferPresentation = abilitySelectionController.HasOfferPresentationListeners;
            var offer = abilitySelectionController.GenerateOffer();
            if (offer == null || offer.Count == 0)
            {
                ResumeFromLevelUp();
                return;
            }

            if (!hasOfferPresentation)
            {
                if (logPrototypeWarnings)
                {
                    Debug.LogWarning(
                        $"{nameof(ArenaRunDirector)} generated a level-up ability offer but no ability selection UI/listener is present. Auto-selecting the first offer so gameplay cannot remain paused.",
                        this);
                }

                if (abilitySelectionController.SelectOffer(0))
                {
                    if (resumeLevelUpCoroutine != null)
                    {
                        StopCoroutine(resumeLevelUpCoroutine);
                        resumeLevelUpCoroutine = null;
                    }

                    if (state == SurvivorRunState.LevelUpPaused)
                    {
                        ResumeFromLevelUp();
                    }
                }
                else if (state == SurvivorRunState.LevelUpPaused)
                {
                    ResumeFromLevelUp();
                }
            }
        }

        private void HandleAbilitySelected(AbilitySelectedEventArgs eventArgs)
        {
            if (state == SurvivorRunState.LevelUpPaused && resumeLevelUpCoroutine == null)
            {
                resumeLevelUpCoroutine = StartCoroutine(ResumeFromLevelUpAfterAbilityPanel());
            }
        }

        private IEnumerator ResumeFromLevelUpAfterAbilityPanel()
        {
            yield return null;
            ResumeFromLevelUp();
            resumeLevelUpCoroutine = null;
        }

        private void ResumeFromLevelUp()
        {
            if (pauseTimeScaleOnLevelUp)
            {
                Time.timeScale = previousTimeScale;
            }

            SetState(stateBeforeLevelUp == SurvivorRunState.BossActive
                ? SurvivorRunState.BossActive
                : SurvivorRunState.Running);
        }

        private void HandleEntityKilled(EntityKilledEvent entityKilledEvent)
        {
            if (!IsRunning || entityKilledEvent.Entity == null)
            {
                return;
            }

            if (playerHealth != null && entityKilledEvent.Entity == playerHealth.gameObject)
            {
                return;
            }

            var enemyConfig = ResolveEnemyConfig(entityKilledEvent.Entity);
            if (enemyConfig == null)
            {
                return;
            }

            if (!xpRewardedEnemies.Add(entityKilledEvent.Entity))
            {
                return;
            }

            enemiesKilled++;
            SpawnXPReward(enemyConfig.XpReward, entityKilledEvent.Entity.transform.position);

            if (activeBoss != null && entityKilledEvent.Entity == activeBoss)
            {
                EndRun(SurvivorRunState.Victory);
            }
        }

        private void HandleEnemySpawned(GameObject enemy)
        {
            if (enemy != null)
            {
                xpRewardedEnemies.Remove(enemy);
            }
        }

        private void SpawnXPReward(int xpAmount, Vector3 position)
        {
            if (xpAmount <= 0 || xpController == null)
            {
                return;
            }

            if (xpOrbPrefab == null || pickupCollector == null)
            {
                if (grantXPDirectlyWhenNoOrbPrefab)
                {
                    if (logPrototypeWarnings && !hasLoggedDirectXPFallback)
                    {
                        hasLoggedDirectXPFallback = true;
                        Debug.Log($"{nameof(ArenaRunDirector)} has no XP orb prefab or pickup collector assigned. Granting enemy XP directly for prototype flow.", this);
                    }

                    xpController.AddXP(xpAmount);
                }

                return;
            }

            var orb = GetXPOrb();
            orb.transform.position = position;
            orb.Initialize(xpAmount, pickupCollector);
            orb.gameObject.SetActive(true);
        }

        private XPOrb GetXPOrb()
        {
            while (xpOrbPool.Count > 0)
            {
                var pooled = xpOrbPool.Pop();
                xpOrbsInPool.Remove(pooled);
                if (pooled != null)
                {
                    return pooled;
                }
            }

            var parent = pickupRoot != null ? pickupRoot : transform;
            var orb = Instantiate(xpOrbPrefab, parent);
            orb.OnCollected += HandleXPOrbCollected;
            return orb;
        }

        private void HandleXPOrbCollected(XPOrb orb, PickupCollector collector)
        {
            if (orb != null && xpOrbsInPool.Add(orb))
            {
                xpOrbPool.Push(orb);
            }
        }

        private void RestoreManualPauseIfNeeded()
        {
            if (!isManualPaused)
            {
                return;
            }

            Time.timeScale = previousTimeScale;
            isManualPaused = false;
        }

        private void RestoreLevelUpPauseIfNeeded()
        {
            if (state == SurvivorRunState.LevelUpPaused && pauseTimeScaleOnLevelUp)
            {
                Time.timeScale = previousTimeScale;
            }
        }

        private void SetState(SurvivorRunState newState)
        {
            if (state == newState)
            {
                return;
            }

            var previous = state;
            state = newState;
            OnRunStateChanged?.Invoke(new SurvivorRunStateChangedEventArgs(previous, newState));
        }

        private static EnemyConfig ResolveEnemyConfig(GameObject entity)
        {
            if (entity == null)
            {
                return null;
            }

            if (entity.TryGetComponent<EnemyController>(out var controller) && controller.Config != null)
            {
                return controller.Config;
            }

            return entity.TryGetComponent<EnemyHealth>(out var health) ? health.Config : null;
        }

        private static int[] CopyXPRequirements(IReadOnlyList<int> requirements)
        {
            if (requirements == null || requirements.Count == 0)
            {
                return Array.Empty<int>();
            }

            var copy = new int[requirements.Count];
            for (var i = 0; i < requirements.Count; i++)
            {
                copy[i] = Mathf.Max(1, requirements[i]);
            }

            return copy;
        }
    }
}
