using System;
using UnityEngine;

namespace TapKnockout.Boss
{
    public readonly struct BossEventArgs
    {
        public BossEventArgs(GameObject boss, BossConfig bossConfig, BossPhaseState phaseState, string eventId)
        {
            Boss = boss;
            BossConfig = bossConfig;
            PhaseState = phaseState;
            EventId = eventId ?? string.Empty;
        }

        public GameObject Boss { get; }
        public BossConfig BossConfig { get; }
        public BossPhaseState PhaseState { get; }
        public string EventId { get; }
    }

    public readonly struct BossPhaseChangedEventArgs
    {
        public BossPhaseChangedEventArgs(GameObject boss, BossConfig bossConfig, BossPhaseState previousPhase, BossPhaseState nextPhase, float healthPercent)
        {
            Boss = boss;
            BossConfig = bossConfig;
            PreviousPhase = previousPhase;
            NextPhase = nextPhase;
            HealthPercent = Mathf.Clamp01(healthPercent);
        }

        public GameObject Boss { get; }
        public BossConfig BossConfig { get; }
        public BossPhaseState PreviousPhase { get; }
        public BossPhaseState NextPhase { get; }
        public float HealthPercent { get; }
    }

    public static class BossEvents
    {
        public static event Action<BossEventArgs> OnBossWarningStarted;
        public static event Action<BossEventArgs> OnBossIntroStarted;
        public static event Action<BossEventArgs> OnBossIntroCompleted;
        public static event Action<BossPhaseChangedEventArgs> OnBossPhaseChanged;
        public static event Action<BossEventArgs> OnBossEnraged;
        public static event Action<BossEventArgs> OnBossDefeated;
        public static event Action<BossEventArgs> OnBossOutroStarted;
        public static event Action<BossEventArgs> OnBossOutroCompleted;

        public static void RaiseBossWarningStarted(BossEventArgs eventArgs)
        {
            OnBossWarningStarted?.Invoke(eventArgs);
        }

        public static void RaiseBossIntroStarted(BossEventArgs eventArgs)
        {
            OnBossIntroStarted?.Invoke(eventArgs);
        }

        public static void RaiseBossIntroCompleted(BossEventArgs eventArgs)
        {
            OnBossIntroCompleted?.Invoke(eventArgs);
        }

        public static void RaiseBossPhaseChanged(BossPhaseChangedEventArgs eventArgs)
        {
            OnBossPhaseChanged?.Invoke(eventArgs);
        }

        public static void RaiseBossEnraged(BossEventArgs eventArgs)
        {
            OnBossEnraged?.Invoke(eventArgs);
        }

        public static void RaiseBossDefeated(BossEventArgs eventArgs)
        {
            OnBossDefeated?.Invoke(eventArgs);
        }

        public static void RaiseBossOutroStarted(BossEventArgs eventArgs)
        {
            OnBossOutroStarted?.Invoke(eventArgs);
        }

        public static void RaiseBossOutroCompleted(BossEventArgs eventArgs)
        {
            OnBossOutroCompleted?.Invoke(eventArgs);
        }
    }
}
