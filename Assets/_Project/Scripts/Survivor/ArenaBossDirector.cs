using System;
using System.Collections;
using TapKnockout.Boss;
using TapKnockout.Combat;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Survivor
{
    public readonly struct SurvivorBossWarningEventArgs
    {
        public SurvivorBossWarningEventArgs(float warningDuration, SpawnGroupConfig spawnGroup)
        {
            WarningDuration = Mathf.Max(0f, warningDuration);
            SpawnGroup = spawnGroup;
        }

        public float WarningDuration { get; }
        public SpawnGroupConfig SpawnGroup { get; }
    }

    public readonly struct SurvivorBossPhaseThresholdEventArgs
    {
        public SurvivorBossPhaseThresholdEventArgs(GameObject boss, BossConfig bossConfig, float threshold, float currentHealthPercent)
        {
            Boss = boss;
            BossConfig = bossConfig;
            Threshold = Mathf.Clamp01(threshold);
            CurrentHealthPercent = Mathf.Clamp01(currentHealthPercent);
        }

        public GameObject Boss { get; }
        public BossConfig BossConfig { get; }
        public float Threshold { get; }
        public float CurrentHealthPercent { get; }
    }

    [DisallowMultipleComponent]
    public sealed class ArenaBossDirector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SurvivorSpawnDirector spawnDirector;
        [SerializeField] private SurvivorFeedbackPlayer feedbackPlayer;

        [Header("Behavior")]
        [SerializeField] private bool pauseNormalSpawnsDuringBoss = true;
        [SerializeField] private bool raiseBossEventsWhenNoIntroController = true;
        [SerializeField] private bool triggerRunVictoryOnBossDefeated = true;
        [SerializeField, Min(0f)] private float warningDurationSeconds = 3f;
        [SerializeField, Min(0f)] private float introDelaySeconds = 0.75f;
        [SerializeField] private float[] phaseHealthThresholds = { 0.7f, 0.4f, 0.15f };

        [Header("Feedback")]
        [SerializeField] private GameObject warningVfxPrefab;
        [SerializeField] private GameObject spawnVfxPrefab;
        [SerializeField] private GameObject defeatVfxPrefab;
        [SerializeField] private AudioClip warningSfx;
        [SerializeField] private AudioClip spawnSfx;
        [SerializeField] private AudioClip defeatSfx;
        [SerializeField, Min(0f)] private float feedbackVfxLifetime = 2f;
        [SerializeField, Min(0f)] private float warningCameraShakeIntensity = 0.025f;
        [SerializeField, Min(0f)] private float warningCameraShakeDuration = 0.15f;
        [SerializeField, Min(0f)] private float spawnCameraShakeIntensity = 0.06f;
        [SerializeField, Min(0f)] private float spawnCameraShakeDuration = 0.18f;
        [SerializeField, Min(0f)] private float defeatCameraShakeIntensity = 0.08f;
        [SerializeField, Min(0f)] private float defeatCameraShakeDuration = 0.22f;

        [Header("Debug")]
        [SerializeField] private bool logWarnings = true;

        private GameObject activeBoss;
        private BossConfig activeBossConfig;
        private EnemyHealth activeBossHealth;
        private BossPhaseController activeBossPhaseController;
        private bool[] phaseThresholdRaised;
        private bool bossDefeatedRaised;
        private bool encounterStarting;
        private Coroutine bossStartCoroutine;

        public event Action<SurvivorBossWarningEventArgs> OnBossWarningStarted;
        public event Action<GameObject> OnBossSpawned;
        public event Action<GameObject> OnBossDefeated;
        public event Action<SurvivorBossPhaseThresholdEventArgs> OnBossPhaseThresholdCrossed;

        public GameObject ActiveBoss => activeBoss;
        public bool HasActiveBoss => activeBoss != null && activeBossHealth != null && activeBossHealth.IsAlive;
        public bool TriggerRunVictoryOnBossDefeated => triggerRunVictoryOnBossDefeated;
        public bool IsEncounterStarting => encounterStarting;

        private void OnDisable()
        {
            if (bossStartCoroutine != null)
            {
                StopCoroutine(bossStartCoroutine);
                bossStartCoroutine = null;
            }

            UnsubscribeBossHealth();
            activeBoss = null;
            activeBossConfig = null;
            activeBossPhaseController = null;
            bossDefeatedRaised = false;
            encounterStarting = false;
        }

        public void Configure(SurvivorSpawnDirector survivorSpawnDirector)
        {
            spawnDirector = survivorSpawnDirector;
        }

        public void BeginBossWarning(SpawnGroupConfig bossSpawnGroup, float durationOverride = -1f)
        {
            var warningDuration = durationOverride >= 0f ? durationOverride : warningDurationSeconds;
            OnBossWarningStarted?.Invoke(new SurvivorBossWarningEventArgs(warningDuration, bossSpawnGroup));
            BossEvents.RaiseBossWarningStarted(new BossEventArgs(null, null, BossPhaseState.None, "survivor_boss_warning"));

            var playerPosition = spawnDirector != null && spawnDirector.LiveEnemyCount >= 0
                ? transform.position
                : Vector3.zero;
            var feedback = ResolveFeedbackPlayer();
            feedback?.SpawnVFX(warningVfxPrefab, playerPosition, Quaternion.identity, null, 1f, feedbackVfxLifetime);
            feedback?.PlayOneShot(warningSfx, playerPosition);
            feedback?.RequestCameraShake(warningCameraShakeIntensity, warningCameraShakeDuration);
        }

        public bool TryStartBossEncounter(SpawnGroupConfig bossSpawnGroup)
        {
            if (activeBoss != null || encounterStarting)
            {
                return false;
            }

            if (spawnDirector == null)
            {
                if (logWarnings)
                {
                    Debug.LogWarning($"{nameof(ArenaBossDirector)} cannot start boss encounter without a {nameof(SurvivorSpawnDirector)}.", this);
                }

                return false;
            }

            if (bossSpawnGroup == null || !bossSpawnGroup.HasValidEnemy)
            {
                if (logWarnings)
                {
                    Debug.LogWarning($"{nameof(ArenaBossDirector)} cannot start boss encounter because Boss Spawn Group is missing EnemyConfig or EnemyPrefab.", this);
                }

                return false;
            }

            if (pauseNormalSpawnsDuringBoss)
            {
                spawnDirector.StopSpawning();
            }

            if (introDelaySeconds > 0f)
            {
                encounterStarting = true;
                bossStartCoroutine = StartCoroutine(StartBossEncounterAfterDelay(bossSpawnGroup, introDelaySeconds));
                return true;
            }

            return SpawnBossNow(bossSpawnGroup);
        }

        public void ClearBoss()
        {
            if (bossStartCoroutine != null)
            {
                StopCoroutine(bossStartCoroutine);
                bossStartCoroutine = null;
            }

            encounterStarting = false;
            UnsubscribeBossHealth();
            activeBoss = null;
            activeBossConfig = null;
            activeBossPhaseController = null;
            bossDefeatedRaised = false;
            phaseThresholdRaised = null;
        }

        private IEnumerator StartBossEncounterAfterDelay(SpawnGroupConfig bossSpawnGroup, float delaySeconds)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
            SpawnBossNow(bossSpawnGroup);
            bossStartCoroutine = null;
        }

        private bool SpawnBossNow(SpawnGroupConfig bossSpawnGroup)
        {
            encounterStarting = false;
            var boss = spawnDirector.SpawnBoss(bossSpawnGroup);
            if (boss == null)
            {
                return false;
            }

            BindBoss(boss);
            PlayBossFeedback(spawnVfxPrefab, spawnSfx, spawnCameraShakeIntensity, spawnCameraShakeDuration, boss.transform.position);
            OnBossSpawned?.Invoke(boss);
            RaiseIntroIfNeeded();
            return true;
        }

        private void BindBoss(GameObject boss)
        {
            UnsubscribeBossHealth();
            activeBoss = boss;
            activeBossHealth = boss != null ? boss.GetComponentInChildren<EnemyHealth>(true) : null;
            activeBossPhaseController = boss != null ? boss.GetComponentInChildren<BossPhaseController>(true) : null;
            activeBossConfig = activeBossPhaseController != null ? activeBossPhaseController.Config : null;
            bossDefeatedRaised = false;
            phaseThresholdRaised = new bool[phaseHealthThresholds != null ? phaseHealthThresholds.Length : 0];

            if (activeBossHealth != null)
            {
                activeBossHealth.OnDamaged += HandleBossDamaged;
                activeBossHealth.OnDied += HandleBossDied;
            }
        }

        private void UnsubscribeBossHealth()
        {
            if (activeBossHealth != null)
            {
                activeBossHealth.OnDamaged -= HandleBossDamaged;
                activeBossHealth.OnDied -= HandleBossDied;
            }

            activeBossHealth = null;
        }

        private void HandleBossDamaged(HitContext hitContext)
        {
            activeBossPhaseController?.RefreshPhase();
            EvaluatePhaseThresholds();
        }

        private void HandleBossDied(HitContext killingHit)
        {
            RaiseBossDefeatedOnce();
        }

        private void RaiseIntroIfNeeded()
        {
            if (!raiseBossEventsWhenNoIntroController || activeBoss == null)
            {
                return;
            }

            if (activeBoss.GetComponentInChildren<BossIntroController>(true) != null)
            {
                return;
            }

            BossEvents.RaiseBossIntroStarted(new BossEventArgs(activeBoss, activeBossConfig, ResolvePhase(), "survivor_boss_spawned"));
        }

        private void RaiseBossDefeatedOnce()
        {
            if (bossDefeatedRaised)
            {
                return;
            }

            bossDefeatedRaised = true;
            var defeatedBoss = activeBoss;
            BossEvents.RaiseBossDefeated(new BossEventArgs(defeatedBoss, activeBossConfig, BossPhaseState.Defeated, "survivor_boss_defeated"));
            if (defeatedBoss != null)
            {
                PlayBossFeedback(defeatVfxPrefab, defeatSfx, defeatCameraShakeIntensity, defeatCameraShakeDuration, defeatedBoss.transform.position);
            }

            OnBossDefeated?.Invoke(defeatedBoss);
        }

        private BossPhaseState ResolvePhase()
        {
            return activeBossPhaseController != null ? activeBossPhaseController.CurrentPhase : BossPhaseState.Phase1;
        }

        private void EvaluatePhaseThresholds()
        {
            if (activeBoss == null || activeBossHealth == null || phaseHealthThresholds == null || phaseThresholdRaised == null)
            {
                return;
            }

            var healthPercent = BossPhaseController.ResolveHealthPercent(activeBossHealth);
            for (var i = 0; i < phaseHealthThresholds.Length; i++)
            {
                var threshold = Mathf.Clamp01(phaseHealthThresholds[i]);
                if (phaseThresholdRaised[i] || healthPercent > threshold)
                {
                    continue;
                }

                phaseThresholdRaised[i] = true;
                OnBossPhaseThresholdCrossed?.Invoke(new SurvivorBossPhaseThresholdEventArgs(
                    activeBoss,
                    activeBossConfig,
                    threshold,
                    healthPercent));
            }
        }

        private void PlayBossFeedback(GameObject vfxPrefab, AudioClip sfx, float shakeIntensity, float shakeDuration, Vector3 position)
        {
            var feedback = ResolveFeedbackPlayer();
            feedback?.SpawnVFX(vfxPrefab, position, Quaternion.identity, null, 1f, feedbackVfxLifetime);
            feedback?.PlayOneShot(sfx, position);
            feedback?.RequestCameraShake(shakeIntensity, shakeDuration);
        }

        private SurvivorFeedbackPlayer ResolveFeedbackPlayer()
        {
            if (feedbackPlayer == null)
            {
                feedbackPlayer = SurvivorFeedbackPlayer.Shared;
            }

            return feedbackPlayer;
        }
    }
}
