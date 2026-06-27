using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Boss
{
    [DisallowMultipleComponent]
    public sealed class BossPatternController : MonoBehaviour
    {
        [SerializeField] private BossPatternConfig config;
        [SerializeField] private Transform target;
        [SerializeField] private bool playOnEnable = true;

        private readonly List<BossAttackStep> runtimeSteps = new List<BossAttackStep>();
        private BossPatternPhase currentPhase = BossPatternPhase.Idle;
        private float phaseRemaining;
        private int currentStepIndex;
        private bool isRunning;

        public BossPatternPhase CurrentPhase => currentPhase;
        public int CurrentStepIndex => currentStepIndex;
        public float PhaseRemaining => phaseRemaining;
        public bool IsRunning => isRunning;

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

        public void SetConfig(BossPatternConfig patternConfig)
        {
            config = patternConfig;
            RebuildRuntimeSteps();
        }

        public void SetTarget(Transform patternTarget)
        {
            target = patternTarget;
        }

        public bool StartPattern()
        {
            RebuildRuntimeSteps();
            if (runtimeSteps.Count == 0)
            {
                isRunning = false;
                currentPhase = BossPatternPhase.Idle;
                return false;
            }

            isRunning = true;
            currentStepIndex = 0;

            if (config != null && config.InitialDelay > 0f)
            {
                currentPhase = BossPatternPhase.Idle;
                phaseRemaining = config.InitialDelay;
                return true;
            }

            StartPhase(BossPatternPhase.Windup);
            return true;
        }

        public void StopPattern()
        {
            isRunning = false;
            currentPhase = BossPatternPhase.Idle;
            phaseRemaining = 0f;
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
                case BossPatternPhase.Windup:
                    StartPhase(BossPatternPhase.Active);
                    break;
                case BossPatternPhase.Active:
                    StartPhase(BossPatternPhase.Cooldown);
                    break;
                case BossPatternPhase.Cooldown:
                    AdvanceStep();
                    break;
                default:
                    StartPhase(BossPatternPhase.Windup);
                    break;
            }
        }

        private void AdvanceStep()
        {
            currentStepIndex++;
            if (currentStepIndex >= runtimeSteps.Count)
            {
                if (config != null && config.Loop)
                {
                    currentStepIndex = 0;
                }
                else
                {
                    CompletePattern();
                    return;
                }
            }

            StartPhase(BossPatternPhase.Windup);
        }

        private void CompletePattern()
        {
            var completedStep = runtimeSteps[Mathf.Clamp(currentStepIndex - 1, 0, runtimeSteps.Count - 1)];
            isRunning = false;
            currentPhase = BossPatternPhase.Completed;
            phaseRemaining = 0f;

            BossPatternEvents.RaisePatternCompleted(new BossPatternEventArgs(
                gameObject,
                target != null ? target.gameObject : null,
                completedStep,
                BossPatternPhase.Completed,
                Mathf.Max(0, currentStepIndex - 1),
                0f));
        }

        private void StartPhase(BossPatternPhase phase)
        {
            currentPhase = phase;
            var step = runtimeSteps[currentStepIndex];
            phaseRemaining = ResolvePhaseDuration(step, phase);

            BossPatternEvents.RaisePhaseStarted(new BossPatternEventArgs(
                gameObject,
                target != null ? target.gameObject : null,
                step,
                phase,
                currentStepIndex,
                phaseRemaining));
        }

        private void RebuildRuntimeSteps()
        {
            runtimeSteps.Clear();
            if (config == null)
            {
                return;
            }

            for (var i = 0; i < config.Steps.Count; i++)
            {
                runtimeSteps.Add(config.Steps[i]);
            }
        }

        private static float ResolvePhaseDuration(BossAttackStep step, BossPatternPhase phase)
        {
            switch (phase)
            {
                case BossPatternPhase.Windup:
                    return step.WindupDuration;
                case BossPatternPhase.Active:
                    return step.ActiveDuration;
                case BossPatternPhase.Cooldown:
                    return step.CooldownDuration;
                default:
                    return 0f;
            }
        }
    }
}
