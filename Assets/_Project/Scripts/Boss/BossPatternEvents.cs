using System;
using UnityEngine;

namespace TapKnockout.Boss
{
    public readonly struct BossPatternEventArgs
    {
        public BossPatternEventArgs(
            GameObject source,
            GameObject target,
            BossAttackStep step,
            BossPatternPhase phase,
            int stepIndex,
            float phaseDuration)
        {
            Source = source;
            Target = target;
            Step = step;
            Phase = phase;
            StepIndex = Mathf.Max(0, stepIndex);
            PhaseDuration = Mathf.Max(0f, phaseDuration);
        }

        public GameObject Source { get; }
        public GameObject Target { get; }
        public BossAttackStep Step { get; }
        public BossPatternPhase Phase { get; }
        public int StepIndex { get; }
        public float PhaseDuration { get; }
    }

    public static class BossPatternEvents
    {
        public static event Action<BossPatternEventArgs> OnPhaseStarted;
        public static event Action<BossPatternEventArgs> OnPatternCompleted;

        public static void RaisePhaseStarted(BossPatternEventArgs eventArgs)
        {
            OnPhaseStarted?.Invoke(eventArgs);
        }

        public static void RaisePatternCompleted(BossPatternEventArgs eventArgs)
        {
            OnPatternCompleted?.Invoke(eventArgs);
        }
    }
}
