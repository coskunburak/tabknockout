using System;
using TapKnockout.Combat;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Boss
{
    [DisallowMultipleComponent]
    public sealed class BossPhaseController : MonoBehaviour
    {
        [SerializeField] private BossConfig config;
        [SerializeField] private EnemyHealth health;
        [SerializeField] private BossPatternController patternController;
        [SerializeField] private BossAddSpawnAction addSpawnAction;
        [SerializeField] private BossChargeAttack chargeAttack;
        [SerializeField] private bool startPatternOnPhaseChange = true;

        private bool hasRaisedEnrage;

        public event Action<BossPhaseChangedEventArgs> OnPhaseChanged;

        public BossConfig Config => config;
        public BossPhaseState CurrentPhase { get; private set; } = BossPhaseState.None;
        public float CurrentHealthPercent => ResolveHealthPercent(health);

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            SubscribeHealth();
            RefreshPhase(true);
        }

        private void OnDisable()
        {
            UnsubscribeHealth();
        }

        public void Initialize(BossConfig bossConfig)
        {
            config = bossConfig;
            hasRaisedEnrage = false;
            CurrentPhase = BossPhaseState.None;
            ResolveReferences();
            if (config != null && config.EnemyConfig != null)
            {
                health?.Initialize(config.EnemyConfig);
            }

            addSpawnAction?.Initialize(config);
            RefreshPhase(true);
        }

        public void SetHealth(EnemyHealth bossHealth)
        {
            UnsubscribeHealth();
            health = bossHealth;
            SubscribeHealth();
            RefreshPhase(true);
        }

        public void RefreshPhase(bool force = false)
        {
            if (health != null && !health.IsAlive)
            {
                ChangePhase(BossPhaseState.Defeated, null, force);
                return;
            }

            var healthPercent = CurrentHealthPercent;
            var phaseConfig = config != null ? config.ResolvePhaseForHealthPercent(healthPercent) : null;
            var nextPhase = phaseConfig != null ? phaseConfig.PhaseState : BossPhaseState.Phase1;
            ChangePhase(nextPhase, phaseConfig, force);
        }

        private void ChangePhase(BossPhaseState nextPhase, BossPhaseConfig phaseConfig, bool force)
        {
            if (CurrentPhase == nextPhase && (!force || CurrentPhase != BossPhaseState.None))
            {
                return;
            }

            var previousPhase = CurrentPhase;
            CurrentPhase = nextPhase;

            if (phaseConfig != null)
            {
                patternController?.SetConfig(phaseConfig.Pattern);
                patternController?.SetDurationMultipliers(1f, 1f, phaseConfig.CooldownDurationMultiplier);
                chargeAttack?.SetRuntimeSpeedMultiplier(phaseConfig.ChargeSpeedMultiplier);

                if (phaseConfig.Enrage && !hasRaisedEnrage)
                {
                    hasRaisedEnrage = true;
                    BossEvents.RaiseBossEnraged(new BossEventArgs(gameObject, config, nextPhase, "boss_enraged"));
                }

                if (startPatternOnPhaseChange && patternController != null && phaseConfig.Pattern != null)
                {
                    patternController.StartPattern();
                }
            }

            var args = new BossPhaseChangedEventArgs(gameObject, config, previousPhase, nextPhase, CurrentHealthPercent);
            OnPhaseChanged?.Invoke(args);
            BossEvents.RaiseBossPhaseChanged(args);
        }

        private void HandleDamaged(HitContext hitContext)
        {
            RefreshPhase();
        }

        private void HandleDied(HitContext hitContext)
        {
            ChangePhase(BossPhaseState.Defeated, null, true);
            patternController?.StopPattern();
            BossEvents.RaiseBossDefeated(new BossEventArgs(gameObject, config, BossPhaseState.Defeated, "boss_defeated"));
        }

        private void ResolveReferences()
        {
            if (health == null)
            {
                health = GetComponent<EnemyHealth>();
            }

            if (patternController == null)
            {
                patternController = GetComponent<BossPatternController>();
            }

            if (addSpawnAction == null)
            {
                addSpawnAction = GetComponent<BossAddSpawnAction>();
            }

            if (chargeAttack == null)
            {
                chargeAttack = GetComponent<BossChargeAttack>();
            }
        }

        private void SubscribeHealth()
        {
            if (health == null)
            {
                return;
            }

            health.OnDamaged -= HandleDamaged;
            health.OnDamaged += HandleDamaged;
            health.OnDied -= HandleDied;
            health.OnDied += HandleDied;
        }

        private void UnsubscribeHealth()
        {
            if (health == null)
            {
                return;
            }

            health.OnDamaged -= HandleDamaged;
            health.OnDied -= HandleDied;
        }

        public static float ResolveHealthPercent(EnemyHealth bossHealth)
        {
            if (bossHealth == null || bossHealth.MaxHealth <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(bossHealth.CurrentHealth / bossHealth.MaxHealth);
        }
    }
}
