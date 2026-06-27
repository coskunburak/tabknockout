using System;
using UnityEngine;

namespace TapKnockout.Combat
{
    public enum ImpactCollisionEventType
    {
        WallSlam = 0,
        ChainKnockback = 1
    }

    public readonly struct ImpactCollisionEventArgs
    {
        public ImpactCollisionEventArgs(
            ImpactCollisionEventType eventType,
            GameObject source,
            GameObject primaryTarget,
            GameObject secondaryTarget,
            HitContext hitContext,
            Vector3 position,
            Vector3 normal,
            float appliedDamage,
            float knockbackForce)
        {
            EventType = eventType;
            Source = source;
            PrimaryTarget = primaryTarget;
            SecondaryTarget = secondaryTarget;
            HitContext = hitContext;
            Position = position;
            Normal = normal.sqrMagnitude > 0f ? normal.normalized : Vector3.up;
            AppliedDamage = Mathf.Max(0f, appliedDamage);
            KnockbackForce = Mathf.Max(0f, knockbackForce);
        }

        public ImpactCollisionEventType EventType { get; }
        public GameObject Source { get; }
        public GameObject PrimaryTarget { get; }
        public GameObject SecondaryTarget { get; }
        public HitContext HitContext { get; }
        public Vector3 Position { get; }
        public Vector3 Normal { get; }
        public float AppliedDamage { get; }
        public float KnockbackForce { get; }
    }

    public static class ImpactCollisionEvents
    {
        public static event Action<ImpactCollisionEventArgs> OnWallSlam;
        public static event Action<ImpactCollisionEventArgs> OnChainKnockback;

        public static void RaiseWallSlam(ImpactCollisionEventArgs eventArgs)
        {
            OnWallSlam?.Invoke(eventArgs);
        }

        public static void RaiseChainKnockback(ImpactCollisionEventArgs eventArgs)
        {
            OnChainKnockback?.Invoke(eventArgs);
        }
    }
}
