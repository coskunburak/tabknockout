using System.Collections.Generic;
using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerAbilityCombatEffectController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerRuntimeStats runtimeStats;
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Elemental Proc Tuning")]
        [SerializeField, Min(0.05f)] private float burnDuration = 3f;
        [SerializeField, Min(0.05f)] private float poisonDuration = 4f;
        [SerializeField, Min(0.05f)] private float freezeDuration = 1.25f;
        [SerializeField, Range(0f, 1f)] private float freezeSlowMultiplier = 0.25f;
        [SerializeField, Min(0f)] private float burnTickDamageRatio = 0.18f;
        [SerializeField, Min(0f)] private float poisonTickDamageRatio = 0.12f;
        [SerializeField, Min(0.05f)] private float burnTickInterval = 1f;
        [SerializeField, Min(0.05f)] private float poisonTickInterval = 0.75f;

        [Header("Lightning Chain")]
        [SerializeField] private LayerMask chainTargetLayers = ~0;
        [SerializeField, Range(0, 6)] private int lightningChainTargets = 2;
        [SerializeField, Min(0.5f)] private float lightningChainRadius = 6f;
        [SerializeField, Range(0f, 1f)] private float lightningChainDamageRatio = 0.35f;
        [SerializeField, Range(4, 64)] private int hitBufferSize = 32;

        private readonly HashSet<GameObject> chainHitTargets = new HashSet<GameObject>();
        private Collider[] hitBuffer;
        private bool subscribedToCombatEvents;
        private bool resolvingTriggeredEffect;

        public PlayerRuntimeStats RuntimeStats => runtimeStats;
        public PlayerHealth PlayerHealth => playerHealth;

        private void Reset()
        {
            runtimeStats = GetComponent<PlayerRuntimeStats>();
            playerHealth = GetComponent<PlayerHealth>();
        }

        private void Awake()
        {
            ResolveReferences();
            EnsureHitBuffer();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeCombatEvents();
        }

        private void OnDisable()
        {
            UnsubscribeCombatEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeCombatEvents();
        }

        private void OnValidate()
        {
            burnDuration = Mathf.Max(0.05f, burnDuration);
            poisonDuration = Mathf.Max(0.05f, poisonDuration);
            freezeDuration = Mathf.Max(0.05f, freezeDuration);
            freezeSlowMultiplier = Mathf.Clamp01(freezeSlowMultiplier);
            burnTickDamageRatio = Mathf.Max(0f, burnTickDamageRatio);
            poisonTickDamageRatio = Mathf.Max(0f, poisonTickDamageRatio);
            burnTickInterval = Mathf.Max(0.05f, burnTickInterval);
            poisonTickInterval = Mathf.Max(0.05f, poisonTickInterval);
            lightningChainTargets = Mathf.Clamp(lightningChainTargets, 0, 6);
            lightningChainRadius = Mathf.Max(0.5f, lightningChainRadius);
            lightningChainDamageRatio = Mathf.Clamp01(lightningChainDamageRatio);
            hitBufferSize = Mathf.Clamp(hitBufferSize, 4, 64);
            EnsureHitBuffer();
        }

        public void SetRuntimeStats(PlayerRuntimeStats stats)
        {
            runtimeStats = stats;
            EnsureActiveSubscription();
        }

        public void SetPlayerHealth(PlayerHealth health)
        {
            playerHealth = health;
            EnsureActiveSubscription();
        }

        private void SubscribeCombatEvents()
        {
            if (subscribedToCombatEvents)
            {
                return;
            }

            CombatEvents.OnDamageDealt -= HandleDamageDealt;
            CombatEvents.OnDamageDealt += HandleDamageDealt;
            CombatEvents.OnEntityKilled -= HandleEntityKilled;
            CombatEvents.OnEntityKilled += HandleEntityKilled;
            subscribedToCombatEvents = true;
        }

        private void UnsubscribeCombatEvents()
        {
            CombatEvents.OnDamageDealt -= HandleDamageDealt;
            CombatEvents.OnEntityKilled -= HandleEntityKilled;
            subscribedToCombatEvents = false;
        }

        private void EnsureActiveSubscription()
        {
            if (isActiveAndEnabled)
            {
                SubscribeCombatEvents();
            }
        }

        private void HandleDamageDealt(DamageEvent damageEvent)
        {
            if (this == null)
            {
                UnsubscribeCombatEvents();
                return;
            }

            if (resolvingTriggeredEffect ||
                damageEvent.HitContext == null ||
                damageEvent.HitContext.IsAbilityHit ||
                !IsPlayerSource(damageEvent.Source) ||
                damageEvent.Target == null ||
                damageEvent.Amount <= 0f)
            {
                return;
            }

            ResolveReferences();
            if (runtimeStats == null)
            {
                return;
            }

            TryApplyElementalStatus(damageEvent);
            TryResolveLightningChain(damageEvent);
        }

        private void HandleEntityKilled(EntityKilledEvent entityKilledEvent)
        {
            if (this == null)
            {
                UnsubscribeCombatEvents();
                return;
            }

            if (!IsPlayerSource(entityKilledEvent.Killer))
            {
                return;
            }

            ResolveReferences();
            if (runtimeStats == null || playerHealth == null || runtimeStats.HealOnKillAmount <= 0f)
            {
                return;
            }

            playerHealth.Heal(runtimeStats.HealOnKillAmount);
        }

        private void TryApplyElementalStatus(DamageEvent damageEvent)
        {
            var receiver = damageEvent.Target.GetComponentInChildren<IStatusEffectReceiver>();
            if (receiver == null)
            {
                receiver = damageEvent.Target.GetComponentInParent<IStatusEffectReceiver>();
            }

            if (receiver == null)
            {
                return;
            }

            if (Roll(runtimeStats.BurnOnHitChance))
            {
                receiver.TryApplyStatusEffect(new StatusEffectRequest(
                    StatusEffectType.Burn,
                    gameObject,
                    burnDuration,
                    Mathf.Max(1f, damageEvent.Amount * burnTickDamageRatio),
                    burnTickInterval));
            }

            if (Roll(runtimeStats.PoisonOnHitChance))
            {
                receiver.TryApplyStatusEffect(new StatusEffectRequest(
                    StatusEffectType.Poison,
                    gameObject,
                    poisonDuration,
                    Mathf.Max(1f, damageEvent.Amount * poisonTickDamageRatio),
                    poisonTickInterval));
            }

            if (Roll(runtimeStats.FreezeOnHitChance))
            {
                receiver.TryApplyStatusEffect(new StatusEffectRequest(
                    StatusEffectType.Freeze,
                    gameObject,
                    freezeDuration,
                    0f,
                    1f,
                    freezeSlowMultiplier));
            }
        }

        private void TryResolveLightningChain(DamageEvent damageEvent)
        {
            if (!Roll(runtimeStats.LightningOnHitChance) ||
                lightningChainTargets <= 0 ||
                chainTargetLayers.value == 0)
            {
                return;
            }

            EnsureHitBuffer();
            chainHitTargets.Clear();
            chainHitTargets.Add(damageEvent.Target);
            var origin = damageEvent.Target.transform.position;
            var hitCount = Physics.OverlapSphereNonAlloc(
                origin,
                lightningChainRadius,
                hitBuffer,
                chainTargetLayers,
                QueryTriggerInteraction.Collide);

            resolvingTriggeredEffect = true;
            try
            {
                var chainedHits = 0;
                for (var i = 0; i < hitCount && chainedHits < lightningChainTargets; i++)
                {
                    var candidate = hitBuffer[i];
                    if (candidate == null || IsSelf(candidate.transform))
                    {
                        continue;
                    }

                    var damageable = candidate.GetComponentInParent<IDamageable>();
                    if (damageable == null || !damageable.IsAlive)
                    {
                        continue;
                    }

                    var targetObject = damageable.GameObject != null ? damageable.GameObject : candidate.gameObject;
                    if (targetObject == null || !chainHitTargets.Add(targetObject))
                    {
                        continue;
                    }

                    var hitPosition = targetObject.transform.position;
                    var direction = hitPosition - origin;
                    direction.y = 0f;
                    direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;

                    var chainHit = new HitContext(gameObject, targetObject, damageEvent.Amount * lightningChainDamageRatio, DamageType.Lightning)
                    {
                        IsAbilityHit = true,
                        AbilityId = "lightning_chain",
                        HitPoint = hitPosition,
                        HitDirection = direction
                    };

                    CombatHitModifierUtility.ApplySourceModifiers(chainHit);
                    damageable.ReceiveHit(chainHit);
                    RaiseDamageEvents(chainHit);
                    chainedHits++;
                }
            }
            finally
            {
                resolvingTriggeredEffect = false;
                chainHitTargets.Clear();
            }
        }

        private void ResolveReferences()
        {
            if (runtimeStats == null)
            {
                runtimeStats = GetComponent<PlayerRuntimeStats>();
            }

            if (playerHealth == null)
            {
                playerHealth = GetComponent<PlayerHealth>();
            }
        }

        private void EnsureHitBuffer()
        {
            if (hitBuffer == null || hitBuffer.Length != hitBufferSize)
            {
                hitBuffer = new Collider[hitBufferSize];
            }
        }

        private bool IsPlayerSource(GameObject source)
        {
            return this != null &&
                source != null &&
                (source == gameObject || source.transform.IsChildOf(transform));
        }

        private bool IsSelf(Transform candidate)
        {
            return candidate == transform || candidate != null && candidate.IsChildOf(transform);
        }

        private static bool Roll(float chance)
        {
            return chance >= 1f || chance > 0f && UnityEngine.Random.value <= chance;
        }

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
    }
}
