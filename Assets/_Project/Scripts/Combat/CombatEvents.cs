using System;
using UnityEngine;

namespace TapKnockout.Combat
{
    public readonly struct DamageEvent
    {
        public DamageEvent(GameObject source, GameObject target, float amount, DamageType damageType, HitContext hitContext = null)
        {
            Source = source;
            Target = target;
            Amount = amount;
            DamageType = damageType;
            HitContext = hitContext;
        }

        public GameObject Source { get; }
        public GameObject Target { get; }
        public float Amount { get; }
        public DamageType DamageType { get; }
        public HitContext HitContext { get; }
    }

    public readonly struct EntityKilledEvent
    {
        public EntityKilledEvent(GameObject entity, GameObject killer, HitContext killingHit)
        {
            Entity = entity;
            Killer = killer;
            KillingHit = killingHit;
        }

        public GameObject Entity { get; }
        public GameObject Killer { get; }
        public HitContext KillingHit { get; }
    }

    /// <summary>
    /// Minimal static combat event surface. Systems should raise events at resolution points only,
    /// not as a replacement for direct component dependencies.
    /// </summary>
    public static class CombatEvents
    {
        public static event Action<HitContext> OnHitResolved;
        public static event Action<DamageEvent> OnDamageDealt;
        public static event Action<DamageEvent> OnDamageReceived;
        public static event Action<EntityKilledEvent> OnEntityKilled;
        public static event Action<HitContext> OnDashHit;

        public static void RaiseHitResolved(HitContext hitContext)
        {
            if (hitContext == null)
            {
                throw new ArgumentNullException(nameof(hitContext));
            }

            OnHitResolved?.Invoke(hitContext);
        }

        public static void RaiseDamageDealt(DamageEvent damageEvent)
        {
            OnDamageDealt?.Invoke(damageEvent);
        }

        public static void RaiseDamageReceived(DamageEvent damageEvent)
        {
            OnDamageReceived?.Invoke(damageEvent);
        }

        public static void RaiseEntityKilled(EntityKilledEvent entityKilledEvent)
        {
            OnEntityKilled?.Invoke(entityKilledEvent);
        }

        public static void RaiseDashHit(HitContext hitContext)
        {
            if (hitContext == null)
            {
                throw new ArgumentNullException(nameof(hitContext));
            }

            OnDashHit?.Invoke(hitContext);
        }
    }
}
