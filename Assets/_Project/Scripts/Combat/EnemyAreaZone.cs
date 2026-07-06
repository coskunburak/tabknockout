using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Combat
{
    /// <summary>
    /// A poolable ground zone that applies a status effect (slow, poison, frost) and optional tick
    /// damage to any IDamageable that enters or stays inside its radius.
    ///
    /// Used by: Cthulhu slow pool, Mushroom spore zone, Yeti frost zone.
    ///
    /// Lifetime is self-managed. The owning controller must call Initialize() after spawning.
    /// Pool-safe: all state resets via IPoolLifecycle.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyAreaZone : MonoBehaviour, IPoolLifecycle
    {
        // ─── Inspector (optional defaults) ────────────────────────────────────
        [Header("Fallback Config (overridden by Initialize)")]
        [SerializeField, Min(0f)] private float fallbackRadius = 2f;
        [SerializeField, Min(0.01f)] private float fallbackDuration = 4f;
        [SerializeField, Min(0.05f)] private float fallbackTickInterval = 1f;
        [SerializeField, Min(0f)] private float fallbackTickDamage;
        [SerializeField] private StatusEffectType fallbackStatusEffect = StatusEffectType.None;
        [SerializeField, Min(0f)] private float fallbackStatusDuration = 2f;
        [SerializeField, Range(0f, 1f)] private float fallbackSlowMultiplier = 0.6f;
        [SerializeField] private LayerMask fallbackHitLayers = ~0;

        [Header("Visual")]
        [SerializeField] private Transform visualRoot;

        // ─── Runtime state ────────────────────────────────────────────────────
        private static readonly Collider[] OverlapBuffer = new Collider[32];

        private GameObject source;
        private float radius;
        private float remainingDuration;
        private float tickInterval;
        private float tickDamage;
        private StatusEffectType statusEffect;
        private float statusDuration;
        private float slowMultiplier;
        private LayerMask hitLayers;
        private float tickRemaining;
        private bool initialized;

        /// <summary>HashSet prevents spamming the same target multiple times per tick.</summary>
        private readonly HashSet<IDamageable> tickedThisInterval = new HashSet<IDamageable>(8);

        // ─── Public state ─────────────────────────────────────────────────────
        public bool IsActive => initialized && remainingDuration > 0f;
        public float RemainingDuration => remainingDuration;
        public float RadiusValue => radius;

        // ─── Lifecycle ────────────────────────────────────────────────────────

        /// <summary>
        /// Call immediately after spawning to configure the zone.
        /// </summary>
        public void Initialize(
            GameObject zoneSsource,
            float zoneRadius,
            float duration,
            float tickIntervalSeconds,
            float damagePerTick,
            StatusEffectType effectType,
            float effectDuration,
            float slowMult,
            LayerMask layers)
        {
            source = zoneSsource;
            radius = Mathf.Max(0.1f, zoneRadius);
            remainingDuration = Mathf.Max(0.01f, duration);
            tickInterval = Mathf.Max(0.05f, tickIntervalSeconds);
            tickDamage = Mathf.Max(0f, damagePerTick);
            statusEffect = effectType;
            statusDuration = Mathf.Max(0f, effectDuration);
            slowMultiplier = Mathf.Clamp01(slowMult);
            hitLayers = layers;
            tickRemaining = 0f; // tick immediately on first frame
            tickedThisInterval.Clear();
            initialized = true;

            ApplyVisualScale();
        }

        /// <summary>Initialize using fallback values baked into the prefab inspector.</summary>
        public void Initialize(GameObject zoneSsource)
        {
            Initialize(zoneSsource, fallbackRadius, fallbackDuration, fallbackTickInterval,
                fallbackTickDamage, fallbackStatusEffect, fallbackStatusDuration,
                fallbackSlowMultiplier, fallbackHitLayers);
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            var deltaTime = Time.deltaTime;
            remainingDuration -= deltaTime;

            if (remainingDuration <= 0f)
            {
                Expire();
                return;
            }

            tickRemaining -= deltaTime;
            if (tickRemaining <= 0f)
            {
                tickRemaining = tickInterval;
                TickZone();
            }
        }

        private void TickZone()
        {
            tickedThisInterval.Clear();

            var hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                radius,
                OverlapBuffer,
                hitLayers,
                QueryTriggerInteraction.Collide);

            for (var i = 0; i < hitCount; i++)
            {
                var col = OverlapBuffer[i];
                if (col == null)
                {
                    continue;
                }

                var damageable = col.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.IsAlive)
                {
                    continue;
                }

                if (!tickedThisInterval.Add(damageable))
                {
                    continue;
                }

                ApplyToTarget(damageable);
            }
        }

        private void ApplyToTarget(IDamageable damageable)
        {
            // Status effect
            if (statusEffect != StatusEffectType.None && statusDuration > 0f)
            {
                var receiver = damageable.GameObject != null
                    ? damageable.GameObject.GetComponentInChildren<IStatusEffectReceiver>()
                    : null;

                receiver?.TryApplyStatusEffect(new StatusEffectRequest(
                    statusEffect,
                    source,
                    statusDuration,
                    tickDamage: 0f,
                    tickInterval: 1f,
                    slowMultiplier: slowMultiplier));
            }

            // Tick damage (separate hit so it shows in damage numbers)
            if (tickDamage > 0f)
            {
                var hitContext = new HitContext(
                    source,
                    damageable.GameObject,
                    tickDamage,
                    ResolveDamageType())
                {
                    IsAbilityHit = true,
                    HitPoint = damageable.GameObject != null
                        ? damageable.GameObject.transform.position
                        : transform.position
                };

                damageable.ReceiveHit(hitContext);
                RaiseDamageEvents(hitContext);
            }
        }

        private DamageType ResolveDamageType()
        {
            return statusEffect switch
            {
                StatusEffectType.Burn => DamageType.Fire,
                StatusEffectType.Poison => DamageType.Poison,
                StatusEffectType.Freeze => DamageType.Ice,
                StatusEffectType.Slow => DamageType.Ice,
                _ => DamageType.Physical
            };
        }

        private void Expire()
        {
            initialized = false;
            remainingDuration = 0f;

            // Self-deactivate; owning pool or owner can clean up.
            gameObject.SetActive(false);
        }

        private void ApplyVisualScale()
        {
            if (visualRoot == null)
            {
                return;
            }

            var diameter = radius * 2f;
            visualRoot.localScale = new Vector3(diameter, 1f, diameter);
        }

        // ─── IPoolLifecycle ───────────────────────────────────────────────────

        public void OnBeforeSpawnFromPool()
        {
            ResetRuntimeState();
        }

        public void OnSpawnedFromPool()
        {
        }

        public void OnBeforeDespawnToPool()
        {
            ResetRuntimeState();
        }

        public void ResetForPool()
        {
            ResetRuntimeState();
        }

        public void ResetRuntimeState()
        {
            initialized = false;
            source = null;
            remainingDuration = 0f;
            tickRemaining = 0f;
            tickedThisInterval.Clear();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static void RaiseDamageEvents(HitContext hitContext)
        {
            CombatEvents.RaiseHitResolved(hitContext);
            if (hitContext.WasIgnored)
            {
                return;
            }

            var damageEvent = new DamageEvent(
                hitContext.Source,
                hitContext.Target,
                hitContext.DamageAmount,
                hitContext.DamageType,
                hitContext);
            CombatEvents.RaiseDamageDealt(damageEvent);
            CombatEvents.RaiseDamageReceived(damageEvent);
        }

        // ─── Gizmos ───────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.25f);
            Gizmos.DrawSphere(transform.position, radius > 0f ? radius : fallbackRadius);
        }
    }
}
