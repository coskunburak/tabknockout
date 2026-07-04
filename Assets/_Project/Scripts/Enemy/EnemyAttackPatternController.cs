using System.Collections.Generic;
using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Enemy
{
    public enum EnemyAttackPatternPhase
    {
        Idle = 0,
        Windup = 1,
        Active = 2,
        Cooldown = 3,
        Completed = 4
    }

    [DisallowMultipleComponent]
    public sealed class EnemyAttackPatternController : MonoBehaviour, IEnemyRuntimeConfigReceiver, IEnemyRuntimeTargetReceiver, IPoolLifecycle
    {
        [SerializeField] private EnemyAttackPatternConfig patternConfig;
        [SerializeField] private EnemyTelegraphController telegraphController;
        [SerializeField] private Transform target;
        [SerializeField] private bool playOnEnable;

        private readonly List<EnemyAttackStep> runtimeSteps = new List<EnemyAttackStep>();
        private EnemyAttackPatternPhase currentPhase = EnemyAttackPatternPhase.Idle;
        private float phaseRemaining;
        private int currentStepIndex;
        private bool isRunning;

        public EnemyAttackPatternPhase CurrentPhase => currentPhase;
        public int CurrentStepIndex => currentStepIndex;
        public float PhaseRemaining => phaseRemaining;
        public bool IsRunning => isRunning;
        public EnemyAttackStep CurrentStep => runtimeSteps.Count > 0
            ? runtimeSteps[Mathf.Clamp(currentStepIndex, 0, runtimeSteps.Count - 1)]
            : default;

        private void Awake()
        {
            if (telegraphController == null)
            {
                telegraphController = GetComponentInChildren<EnemyTelegraphController>(true);
            }
        }

        private void OnEnable()
        {
            if (playOnEnable)
            {
                StartPattern();
            }
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        public void Initialize(EnemyConfig enemyConfig, Transform runtimeTarget)
        {
            target = runtimeTarget;
            if (enemyConfig != null && enemyConfig.AttackPattern != null)
            {
                SetConfig(enemyConfig.AttackPattern);
            }
        }

        public void SetConfig(EnemyAttackPatternConfig config)
        {
            patternConfig = config;
            RebuildRuntimeSteps();
        }

        public void SetTarget(Transform patternTarget)
        {
            target = patternTarget;
        }

        public void ResetRuntimeState(bool clearTarget = true)
        {
            StopPattern();
            currentStepIndex = 0;
            currentPhase = EnemyAttackPatternPhase.Idle;
            phaseRemaining = 0f;
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
            if (playOnEnable)
            {
                StartPattern();
            }
        }

        public void OnBeforeDespawnToPool()
        {
            ResetRuntimeState();
        }

        public void ResetForPool()
        {
            ResetRuntimeState();
        }

        public bool StartPattern()
        {
            RebuildRuntimeSteps();
            if (runtimeSteps.Count == 0)
            {
                StopPattern();
                return false;
            }

            isRunning = true;
            currentStepIndex = 0;
            if (patternConfig != null && patternConfig.InitialDelay > 0f)
            {
                currentPhase = EnemyAttackPatternPhase.Idle;
                phaseRemaining = patternConfig.InitialDelay;
                return true;
            }

            StartPhase(EnemyAttackPatternPhase.Windup);
            return true;
        }

        public void StopPattern()
        {
            isRunning = false;
            currentPhase = EnemyAttackPatternPhase.Idle;
            phaseRemaining = 0f;
            telegraphController?.EndTelegraph();
        }

        public void Advance(float deltaTime)
        {
            if (!isRunning)
            {
                return;
            }

            phaseRemaining -= Mathf.Max(0f, deltaTime);
            var guard = 0;
            while (isRunning && phaseRemaining <= 0f && guard < 8)
            {
                guard++;
                AdvancePhase();
            }
        }

        private void AdvancePhase()
        {
            switch (currentPhase)
            {
                case EnemyAttackPatternPhase.Windup:
                    StartPhase(EnemyAttackPatternPhase.Active);
                    break;
                case EnemyAttackPatternPhase.Active:
                    StartPhase(EnemyAttackPatternPhase.Cooldown);
                    break;
                case EnemyAttackPatternPhase.Cooldown:
                    AdvanceStep();
                    break;
                default:
                    StartPhase(EnemyAttackPatternPhase.Windup);
                    break;
            }
        }

        private void AdvanceStep()
        {
            currentStepIndex++;
            if (currentStepIndex >= runtimeSteps.Count)
            {
                if (patternConfig != null && patternConfig.Loop)
                {
                    currentStepIndex = 0;
                }
                else
                {
                    currentPhase = EnemyAttackPatternPhase.Completed;
                    phaseRemaining = 0f;
                    isRunning = false;
                    telegraphController?.EndTelegraph();
                    return;
                }
            }

            StartPhase(EnemyAttackPatternPhase.Windup);
        }

        private void StartPhase(EnemyAttackPatternPhase phase)
        {
            currentPhase = phase;
            var step = runtimeSteps[currentStepIndex];
            phaseRemaining = ResolvePhaseDuration(step, phase);

            if (phase == EnemyAttackPatternPhase.Windup && step.TelegraphType != EnemyTelegraphType.None)
            {
                telegraphController?.BeginTelegraph(null, step.TelegraphType, step.WindupDuration, transform, target);
                EnemyAttackEvents.RaiseTelegraphStarted(new EnemyAttackEventArgs(
                    EnemyAttackPhase.TelegraphStarted,
                    gameObject,
                    target != null ? target.gameObject : null,
                    transform.position,
                    step.WindupDuration,
                    step.CooldownDuration));
            }
            else if (phase == EnemyAttackPatternPhase.Active)
            {
                telegraphController?.EndTelegraph();
                EnemyAttackEvents.RaiseAttackReleased(new EnemyAttackEventArgs(
                    EnemyAttackPhase.AttackReleased,
                    gameObject,
                    target != null ? target.gameObject : null,
                    transform.position,
                    step.ActiveDuration,
                    step.CooldownDuration));
            }
        }

        private void RebuildRuntimeSteps()
        {
            runtimeSteps.Clear();
            if (patternConfig == null)
            {
                return;
            }

            for (var i = 0; i < patternConfig.Steps.Count; i++)
            {
                runtimeSteps.Add(patternConfig.Steps[i]);
            }
        }

        private static float ResolvePhaseDuration(EnemyAttackStep step, EnemyAttackPatternPhase phase)
        {
            switch (phase)
            {
                case EnemyAttackPatternPhase.Windup:
                    return step.WindupDuration;
                case EnemyAttackPatternPhase.Active:
                    return step.ActiveDuration;
                case EnemyAttackPatternPhase.Cooldown:
                    return step.CooldownDuration;
                default:
                    return 0f;
            }
        }
    }
}
