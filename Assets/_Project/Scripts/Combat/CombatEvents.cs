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

    public readonly struct ShotFiredEvent
    {
        public ShotFiredEvent(
            GameObject source,
            Vector3 position,
            Quaternion rotation,
            Vector3 direction,
            IReticlePulseTarget reticlePulseTarget = null,
            ParticleSystem muzzleFlash = null,
            AudioSource audioSource = null,
            AudioClip shotSfx = null,
            float shotSfxVolume = 1f,
            Vector3 reticlePosition = default,
            bool hasReticlePosition = false)
        {
            Source = source;
            Position = position;
            Rotation = rotation;
            Direction = direction;
            ReticlePulseTarget = reticlePulseTarget;
            MuzzleFlash = muzzleFlash;
            AudioSource = audioSource;
            ShotSfx = shotSfx;
            ShotSfxVolume = Mathf.Clamp01(shotSfxVolume);
            ReticlePosition = reticlePosition;
            HasReticlePosition = hasReticlePosition;
        }

        public GameObject Source { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Vector3 Direction { get; }
        public IReticlePulseTarget ReticlePulseTarget { get; }
        public ParticleSystem MuzzleFlash { get; }
        public AudioSource AudioSource { get; }
        public AudioClip ShotSfx { get; }
        public float ShotSfxVolume { get; }
        public Vector3 ReticlePosition { get; }
        public bool HasReticlePosition { get; }
    }

    public readonly struct ProjectileSpawnedEvent
    {
        public ProjectileSpawnedEvent(
            GameObject source,
            GameObject projectile,
            Vector3 position,
            Quaternion rotation,
            Vector3 direction,
            float lifetime)
        {
            Source = source;
            Projectile = projectile;
            Position = position;
            Rotation = rotation;
            Direction = direction;
            Lifetime = Mathf.Max(0f, lifetime);
        }

        public GameObject Source { get; }
        public GameObject Projectile { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Vector3 Direction { get; }
        public float Lifetime { get; }
    }

    public delegate bool ShotFiredFeedbackHandler(ShotFiredEvent shotFiredEvent);

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
        public static event Action<ProjectileSpawnedEvent> OnProjectileSpawned;
        public static event ShotFiredFeedbackHandler OnShotFired;

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

        public static void RaiseProjectileSpawned(ProjectileSpawnedEvent projectileSpawnedEvent)
        {
            OnProjectileSpawned?.Invoke(projectileSpawnedEvent);
        }

        public static bool RaiseShotFired(ShotFiredEvent shotFiredEvent)
        {
            var handlers = OnShotFired;
            if (handlers == null)
            {
                return false;
            }

            var handled = false;
            foreach (ShotFiredFeedbackHandler handler in handlers.GetInvocationList())
            {
                handled |= handler(shotFiredEvent);
            }

            return handled;
        }
    }
}
