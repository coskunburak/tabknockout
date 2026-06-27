using System;
using System.Collections.Generic;
using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class KnockbackReceiver : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private EnemyConfig config;
        [SerializeField] private WallSlamConfig wallSlamConfig;
        [SerializeField] private ChainKnockbackConfig chainKnockbackConfig;

        [Header("Fallback")]
        [SerializeField, Range(0f, 1f)] private float fallbackResistance;

        [Header("Wall Slam Fallback")]
        [SerializeField] private bool fallbackEnableWallSlam;
        [SerializeField] private LayerMask fallbackWallSlamLayers;
        [SerializeField, Min(0f)] private float fallbackWallSlamMinKnockbackForce = 2f;
        [SerializeField, Min(0f)] private float fallbackWallSlamBaseDamage = 6f;
        [SerializeField, Min(0f)] private float fallbackWallSlamDamagePerForce = 0.5f;
        [SerializeField, Min(0f)] private float fallbackWallSlamCooldown = 0.2f;
        [SerializeField] private bool fallbackWallSlamStopsKnockback = true;

        [Header("Chain Knockback Fallback")]
        [SerializeField] private bool fallbackEnableChainKnockback;
        [SerializeField] private LayerMask fallbackChainKnockbackLayers = ~0;
        [SerializeField, Min(0f)] private float fallbackChainBaseDamage = 4f;
        [SerializeField, Min(0f)] private float fallbackChainDamagePerForce = 0.25f;
        [SerializeField, Range(0f, 1f)] private float fallbackChainSecondaryForceMultiplier = 0.35f;
        [SerializeField, Min(0f)] private float fallbackChainTargetCooldown = 0.2f;
        [SerializeField, Min(0)] private int fallbackMaxChainHitsPerKnockback = 1;

        private Rigidbody cachedRigidbody;
        private EnemyHealth enemyHealth;
        private Vector3 knockbackVelocity;
        private float remainingDuration;
        private float currentEffectiveForce;
        private float lastWallSlamTime = -999f;
        private bool wallSlamConsumedThisKnockback;
        private GameObject lastKnockbackSource;
        private readonly HashSet<GameObject> chainTargetsThisKnockback = new HashSet<GameObject>();
        private readonly Dictionary<GameObject, float> lastChainHitTimesByTarget = new Dictionary<GameObject, float>();

        public event Action<KnockbackData> OnKnockbackReceived;

        public bool IsKnockbackActive => remainingDuration > 0f;
        public float RemainingDuration => remainingDuration;
        public EnemyConfig Config => config;

        private float Resistance => config != null ? config.KnockbackResistance : fallbackResistance;
        private bool WallSlamEnabled => wallSlamConfig != null ? wallSlamConfig.Enabled : fallbackEnableWallSlam;
        private LayerMask WallSlamLayers => wallSlamConfig != null ? wallSlamConfig.WallLayers : fallbackWallSlamLayers;
        private float WallSlamMinKnockbackForce => wallSlamConfig != null ? wallSlamConfig.MinKnockbackForce : fallbackWallSlamMinKnockbackForce;
        private float WallSlamBaseDamage => wallSlamConfig != null ? wallSlamConfig.BaseDamage : fallbackWallSlamBaseDamage;
        private float WallSlamDamagePerForce => wallSlamConfig != null ? wallSlamConfig.DamagePerKnockbackForce : fallbackWallSlamDamagePerForce;
        private float WallSlamCooldown => wallSlamConfig != null ? wallSlamConfig.CooldownSeconds : fallbackWallSlamCooldown;
        private bool WallSlamStopsKnockback => wallSlamConfig != null ? wallSlamConfig.StopKnockbackOnSlam : fallbackWallSlamStopsKnockback;
        private bool ChainKnockbackEnabled => chainKnockbackConfig != null ? chainKnockbackConfig.Enabled : fallbackEnableChainKnockback;
        private LayerMask ChainKnockbackLayers => chainKnockbackConfig != null ? chainKnockbackConfig.TargetLayers : fallbackChainKnockbackLayers;
        private float ChainBaseDamage => chainKnockbackConfig != null ? chainKnockbackConfig.BaseDamage : fallbackChainBaseDamage;
        private float ChainDamagePerForce => chainKnockbackConfig != null ? chainKnockbackConfig.DamagePerKnockbackForce : fallbackChainDamagePerForce;
        private float ChainSecondaryForceMultiplier => chainKnockbackConfig != null ? chainKnockbackConfig.SecondaryKnockbackForceMultiplier : fallbackChainSecondaryForceMultiplier;
        private float ChainTargetCooldown => chainKnockbackConfig != null ? chainKnockbackConfig.TargetCooldownSeconds : fallbackChainTargetCooldown;
        private int MaxChainHitsPerKnockback => chainKnockbackConfig != null ? chainKnockbackConfig.MaxHitsPerKnockback : fallbackMaxChainHitsPerKnockback;

        private void Reset()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            cachedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            cachedRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            cachedRigidbody.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        private void Awake()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            enemyHealth = GetComponent<EnemyHealth>();
        }

        private void OnValidate()
        {
            fallbackResistance = Mathf.Clamp01(fallbackResistance);
            fallbackWallSlamMinKnockbackForce = Mathf.Max(0f, fallbackWallSlamMinKnockbackForce);
            fallbackWallSlamBaseDamage = Mathf.Max(0f, fallbackWallSlamBaseDamage);
            fallbackWallSlamDamagePerForce = Mathf.Max(0f, fallbackWallSlamDamagePerForce);
            fallbackWallSlamCooldown = Mathf.Max(0f, fallbackWallSlamCooldown);
            fallbackChainBaseDamage = Mathf.Max(0f, fallbackChainBaseDamage);
            fallbackChainDamagePerForce = Mathf.Max(0f, fallbackChainDamagePerForce);
            fallbackChainSecondaryForceMultiplier = Mathf.Clamp01(fallbackChainSecondaryForceMultiplier);
            fallbackChainTargetCooldown = Mathf.Max(0f, fallbackChainTargetCooldown);
            fallbackMaxChainHitsPerKnockback = Mathf.Max(0, fallbackMaxChainHitsPerKnockback);
        }

        private void FixedUpdate()
        {
            if (!IsKnockbackActive)
            {
                return;
            }

            var stepTime = Mathf.Min(Time.fixedDeltaTime, remainingDuration);
            remainingDuration = Mathf.Max(0f, remainingDuration - Time.fixedDeltaTime);

            var currentPosition = cachedRigidbody.position;
            var targetPosition = currentPosition + knockbackVelocity * stepTime;
            targetPosition.y = currentPosition.y;
            cachedRigidbody.MovePosition(targetPosition);
        }

        public void Initialize(EnemyConfig enemyConfig)
        {
            config = enemyConfig;
        }

        public void ApplyKnockback(HitContext hitContext)
        {
            if (hitContext == null)
            {
                return;
            }

            ApplyKnockback(hitContext.Knockback, hitContext.Source);
        }

        public void ApplyKnockback(KnockbackData knockbackData)
        {
            ApplyKnockback(knockbackData, null);
        }

        private void ApplyKnockback(KnockbackData knockbackData, GameObject source)
        {
            if (!knockbackData.HasKnockback || enemyHealth != null && !enemyHealth.IsAlive)
            {
                return;
            }

            if (config != null && !config.CanBeKnockedBack)
            {
                return;
            }

            var effectiveForce = CalculateEffectiveForce(knockbackData.Force, Resistance);
            if (effectiveForce <= 0f)
            {
                return;
            }

            knockbackVelocity = knockbackData.Direction.normalized * effectiveForce;
            remainingDuration = knockbackData.Duration;
            currentEffectiveForce = effectiveForce;
            lastKnockbackSource = source;
            wallSlamConsumedThisKnockback = false;
            chainTargetsThisKnockback.Clear();
            OnKnockbackReceived?.Invoke(knockbackData);
        }

        public static float CalculateEffectiveForce(float force, float resistance)
        {
            return Mathf.Max(0f, force) * (1f - Mathf.Clamp01(resistance));
        }

        public static float CalculateWallSlamDamage(float effectiveForce, float baseDamage, float damagePerForce)
        {
            return Mathf.Max(0f, baseDamage) + Mathf.Max(0f, effectiveForce) * Mathf.Max(0f, damagePerForce);
        }

        public static float CalculateChainKnockbackDamage(float effectiveForce, float baseDamage, float damagePerForce)
        {
            return Mathf.Max(0f, baseDamage) + Mathf.Max(0f, effectiveForce) * Mathf.Max(0f, damagePerForce);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsKnockbackActive || collision == null)
            {
                return;
            }

            var contactPoint = ResolveContactPoint(collision);
            var contactNormal = ResolveContactNormal(collision);

            if (TryResolveWallSlam(collision.collider, contactPoint, contactNormal))
            {
                return;
            }

            TryResolveChainKnockback(collision.collider, contactPoint, contactNormal);
        }

        private bool TryResolveWallSlam(Collider other, Vector3 contactPoint, Vector3 contactNormal)
        {
            if (!WallSlamEnabled || wallSlamConsumedThisKnockback || other == null)
            {
                return false;
            }

            if (WallSlamLayers.value == 0 || !IsLayerInMask(other.gameObject.layer, WallSlamLayers))
            {
                return false;
            }

            if (currentEffectiveForce < WallSlamMinKnockbackForce || Time.time - lastWallSlamTime < WallSlamCooldown)
            {
                return false;
            }

            if (other.GetComponentInParent<IDamageable>() != null)
            {
                return false;
            }

            if (enemyHealth == null || !enemyHealth.IsAlive)
            {
                return false;
            }

            wallSlamConsumedThisKnockback = true;
            lastWallSlamTime = Time.time;

            var damage = CalculateWallSlamDamage(currentEffectiveForce, WallSlamBaseDamage, WallSlamDamagePerForce);
            var hitContext = new HitContext(lastKnockbackSource, gameObject, damage, DamageType.Impact)
            {
                IsAbilityHit = true,
                AbilityId = "wall_slam",
                HitPoint = contactPoint,
                HitDirection = knockbackVelocity.sqrMagnitude > 0f ? knockbackVelocity.normalized : -contactNormal
            };

            enemyHealth.ReceiveHit(hitContext);
            RaiseDamageEventsIfNeeded(hitContext);

            ImpactCollisionEvents.RaiseWallSlam(new ImpactCollisionEventArgs(
                ImpactCollisionEventType.WallSlam,
                lastKnockbackSource,
                gameObject,
                other.gameObject,
                hitContext,
                contactPoint,
                contactNormal,
                damage,
                currentEffectiveForce));

            if (WallSlamStopsKnockback)
            {
                remainingDuration = 0f;
                knockbackVelocity = Vector3.zero;
            }

            return true;
        }

        private bool TryResolveChainKnockback(Collider other, Vector3 contactPoint, Vector3 contactNormal)
        {
            if (!ChainKnockbackEnabled || other == null || MaxChainHitsPerKnockback <= 0)
            {
                return false;
            }

            if (ChainKnockbackLayers.value == 0 || !IsLayerInMask(other.gameObject.layer, ChainKnockbackLayers))
            {
                return false;
            }

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive || damageable.GameObject == gameObject)
            {
                return false;
            }

            var targetObject = damageable.GameObject != null ? damageable.GameObject : other.gameObject;
            if (targetObject == gameObject || transform.IsChildOf(targetObject.transform) || targetObject.transform.IsChildOf(transform))
            {
                return false;
            }

            if (chainTargetsThisKnockback.Count >= MaxChainHitsPerKnockback)
            {
                return false;
            }

            if (chainTargetsThisKnockback.Contains(targetObject) || IsTargetOnChainCooldown(targetObject))
            {
                return false;
            }

            chainTargetsThisKnockback.Add(targetObject);
            lastChainHitTimesByTarget[targetObject] = Time.time;

            var chainDirection = ResolveChainDirection(targetObject.transform);
            var damage = CalculateChainKnockbackDamage(currentEffectiveForce, ChainBaseDamage, ChainDamagePerForce);
            var hitContext = new HitContext(lastKnockbackSource != null ? lastKnockbackSource : gameObject, targetObject, damage, DamageType.Impact)
            {
                IsAbilityHit = true,
                AbilityId = "chain_knockback",
                HitPoint = contactPoint,
                HitDirection = chainDirection,
                Knockback = new KnockbackData(
                    chainDirection,
                    currentEffectiveForce * ChainSecondaryForceMultiplier,
                    remainingDuration)
            };

            damageable.ReceiveHit(hitContext);
            RaiseDamageEventsIfNeeded(hitContext);

            ImpactCollisionEvents.RaiseChainKnockback(new ImpactCollisionEventArgs(
                ImpactCollisionEventType.ChainKnockback,
                lastKnockbackSource,
                gameObject,
                targetObject,
                hitContext,
                contactPoint,
                contactNormal,
                damage,
                currentEffectiveForce));

            return true;
        }

        private bool IsTargetOnChainCooldown(GameObject targetObject)
        {
            return targetObject != null &&
                lastChainHitTimesByTarget.TryGetValue(targetObject, out var lastHitTime) &&
                Time.time - lastHitTime < ChainTargetCooldown;
        }

        private Vector3 ResolveChainDirection(Transform targetTransform)
        {
            if (targetTransform != null)
            {
                var direction = targetTransform.position - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    return direction.normalized;
                }
            }

            return knockbackVelocity.sqrMagnitude > 0f ? knockbackVelocity.normalized : transform.forward;
        }

        private static Vector3 ResolveContactPoint(Collision collision)
        {
            return collision.contactCount > 0 ? collision.GetContact(0).point : collision.transform.position;
        }

        private static Vector3 ResolveContactNormal(Collision collision)
        {
            return collision.contactCount > 0 ? collision.GetContact(0).normal : Vector3.up;
        }

        private static bool IsLayerInMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }

        private static void RaiseDamageEventsIfNeeded(HitContext hitContext)
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
    }
}
