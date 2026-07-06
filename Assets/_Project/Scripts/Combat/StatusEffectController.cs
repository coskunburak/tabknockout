using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Combat
{
    [DisallowMultipleComponent]
    public sealed class StatusEffectController : MonoBehaviour, IStatusEffectReceiver, IPoolLifecycle
    {
        private readonly List<ActiveStatusEffect> activeEffects = new List<ActiveStatusEffect>(4);
        private IDamageable damageable;

        public int ActiveEffectCount => activeEffects.Count;
        public bool IsStunned => HasActiveEffect(StatusEffectType.Stun) || HasActiveEffect(StatusEffectType.Freeze);
        public float MoveSpeedMultiplier => ResolveMoveSpeedMultiplier();

        private void Awake()
        {
            damageable = GetComponentInParent<IDamageable>();
        }

        private void Update()
        {
            if (activeEffects.Count == 0)
            {
                return;
            }

            TickActiveEffects(Time.deltaTime);
        }

        public bool TryApplyStatusEffect(StatusEffectRequest request)
        {
            if (!request.IsValid)
            {
                return false;
            }

            activeEffects.Add(new ActiveStatusEffect(request));
            return true;
        }

        public bool HasActiveEffect(StatusEffectType effectType)
        {
            for (var i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i].EffectType == effectType)
                {
                    return true;
                }
            }

            return false;
        }

        public void ClearAll()
        {
            activeEffects.Clear();
        }

        public void OnBeforeSpawnFromPool()
        {
            ClearAll();
        }

        public void OnSpawnedFromPool()
        {
        }

        public void OnBeforeDespawnToPool()
        {
            ClearAll();
        }

        public void ResetForPool()
        {
            ClearAll();
        }

        private void TickActiveEffects(float deltaTime)
        {
            for (var i = activeEffects.Count - 1; i >= 0; i--)
            {
                var activeEffect = activeEffects[i];
                activeEffect.Tick(deltaTime, this);

                if (activeEffect.RemainingDuration <= 0f)
                {
                    activeEffects.RemoveAt(i);
                    continue;
                }

                activeEffects[i] = activeEffect;
            }
        }

        private void ApplyTickDamage(StatusEffectRequest request)
        {
            if (request.TickDamage <= 0f)
            {
                return;
            }

            damageable ??= GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return;
            }

            var hitContext = new HitContext(request.Source, damageable.GameObject, request.TickDamage, ResolveDamageType(request.EffectType))
            {
                IsAbilityHit = true,
                HitPoint = transform.position
            };

            damageable.ReceiveHit(hitContext);
            RaiseDamageEvents(hitContext);
        }

        private float ResolveMoveSpeedMultiplier()
        {
            var multiplier = 1f;
            for (var i = 0; i < activeEffects.Count; i++)
            {
                var request = activeEffects[i].Request;
                if (request.EffectType == StatusEffectType.Slow || request.EffectType == StatusEffectType.Freeze)
                {
                    multiplier = Mathf.Min(multiplier, request.SlowMultiplier);
                }
            }

            return multiplier;
        }

        private static DamageType ResolveDamageType(StatusEffectType effectType)
        {
            return effectType switch
            {
                StatusEffectType.Burn => DamageType.Fire,
                StatusEffectType.Poison => DamageType.Poison,
                StatusEffectType.Freeze => DamageType.Ice,
                StatusEffectType.LightningShock => DamageType.Lightning,
                _ => DamageType.Physical
            };
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

        private struct ActiveStatusEffect
        {
            private float tickRemaining;

            public ActiveStatusEffect(StatusEffectRequest request)
            {
                Request = request;
                RemainingDuration = request.Duration;
                tickRemaining = request.TickInterval;
            }

            public StatusEffectRequest Request { get; }
            public StatusEffectType EffectType => Request.EffectType;
            public float RemainingDuration { get; private set; }

            public void Tick(float deltaTime, StatusEffectController owner)
            {
                RemainingDuration -= deltaTime;
                tickRemaining -= deltaTime;

                if (tickRemaining > 0f)
                {
                    return;
                }

                tickRemaining = Request.TickInterval;
                owner.ApplyTickDamage(Request);
            }
        }
    }
}
