using System;
using UnityEngine;

namespace TapKnockout.Combat
{
    public enum EnemyAttackPhase
    {
        TelegraphStarted = 0,
        TelegraphCancelled = 1,
        AttackReleased = 2,
        CooldownStarted = 3,
        Ready = 4
    }

    public readonly struct EnemyAttackEventArgs
    {
        public EnemyAttackEventArgs(
            EnemyAttackPhase phase,
            GameObject source,
            GameObject target,
            Vector3 position,
            float duration,
            float cooldown)
        {
            Phase = phase;
            Source = source;
            Target = target;
            Position = position;
            Duration = Mathf.Max(0f, duration);
            Cooldown = Mathf.Max(0f, cooldown);
        }

        public EnemyAttackPhase Phase { get; }
        public GameObject Source { get; }
        public GameObject Target { get; }
        public Vector3 Position { get; }
        public float Duration { get; }
        public float Cooldown { get; }
    }

    public static class EnemyAttackEvents
    {
        public static event Action<EnemyAttackEventArgs> OnTelegraphStarted;
        public static event Action<EnemyAttackEventArgs> OnTelegraphCancelled;
        public static event Action<EnemyAttackEventArgs> OnAttackReleased;
        public static event Action<EnemyAttackEventArgs> OnCooldownStarted;
        public static event Action<EnemyAttackEventArgs> OnReady;

        public static void RaiseTelegraphStarted(EnemyAttackEventArgs eventArgs)
        {
            OnTelegraphStarted?.Invoke(eventArgs);
        }

        public static void RaiseTelegraphCancelled(EnemyAttackEventArgs eventArgs)
        {
            OnTelegraphCancelled?.Invoke(eventArgs);
        }

        public static void RaiseAttackReleased(EnemyAttackEventArgs eventArgs)
        {
            OnAttackReleased?.Invoke(eventArgs);
        }

        public static void RaiseCooldownStarted(EnemyAttackEventArgs eventArgs)
        {
            OnCooldownStarted?.Invoke(eventArgs);
        }

        public static void RaiseReady(EnemyAttackEventArgs eventArgs)
        {
            OnReady?.Invoke(eventArgs);
        }
    }
}
