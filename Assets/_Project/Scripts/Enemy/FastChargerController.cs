using System.Collections.Generic;
using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class FastChargerController : MonoBehaviour, IEnemyRuntimeConfigReceiver, IEnemyRuntimeTargetReceiver, IPoolLifecycle
    {
        public enum ChargeState
        {
            Idle = 0,
            Telegraphing = 1,
            Charging = 2,
            Recovery = 3
        }

        [SerializeField] private EnemyConfig config;
        [SerializeField] private Transform target;
        [SerializeField] private EnemyTelegraphController telegraphController;
        [SerializeField] private bool autoStartCharges = true;
        [SerializeField, Min(0f)] private float fallbackChargeRange = 6f;
        [SerializeField, Min(0f)] private float fallbackWindupDuration = 0.45f;
        [SerializeField, Min(0f)] private float fallbackChargeSpeed = 8f;
        [SerializeField, Min(0f)] private float fallbackChargeDuration = 0.4f;
        [SerializeField, Min(0f)] private float fallbackRecoveryDuration = 0.55f;
        [SerializeField, Min(0f)] private float fallbackCooldown = 1.2f;
        [SerializeField, Min(0f)] private float fallbackDamage = 10f;

        private readonly HashSet<GameObject> hitTargetsThisCharge = new HashSet<GameObject>();
        private Rigidbody cachedRigidbody;
        private EnemyHealth enemyHealth;
        private Vector3 currentAimDirection = Vector3.forward;
        private Vector3 lockedChargeDirection = Vector3.forward;
        private float stateRemaining;
        private float cooldownRemaining;

        public ChargeState State { get; private set; }
        public Vector3 LockedChargeDirection => lockedChargeDirection;
        public bool IsCharging => State == ChargeState.Charging;

        private float ChargeRange => config != null ? Mathf.Max(fallbackChargeRange, config.AttackRange) : fallbackChargeRange;
        private float WindupDuration => config != null ? Mathf.Max(config.AttackWindup, fallbackWindupDuration) : fallbackWindupDuration;
        private float ChargeSpeed => config != null ? Mathf.Max(0f, config.MoveSpeed * config.ChargeSpeedMultiplier) : fallbackChargeSpeed;
        private float ChargeDuration => config != null ? config.ChargeDuration : fallbackChargeDuration;
        private float RecoveryDuration => config != null ? config.ChargeRecoveryDuration : fallbackRecoveryDuration;
        private float Cooldown => config != null ? config.AttackCooldown : fallbackCooldown;
        private float Damage => config != null ? config.ContactDamage : fallbackDamage;

        private void Awake()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            enemyHealth = GetComponent<EnemyHealth>();
            if (telegraphController == null)
            {
                telegraphController = GetComponentInChildren<EnemyTelegraphController>(true);
            }
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (State != ChargeState.Charging || cachedRigidbody == null)
            {
                return;
            }

            var currentPosition = cachedRigidbody.position;
            var nextPosition = currentPosition + lockedChargeDirection * (ChargeSpeed * Time.fixedDeltaTime);
            nextPosition.y = currentPosition.y;
            cachedRigidbody.MovePosition(nextPosition);
        }

        public void Initialize(EnemyConfig enemyConfig, Transform runtimeTarget)
        {
            config = enemyConfig;
            target = runtimeTarget;
            ResetRuntimeState(false);
        }

        public void SetTarget(Transform runtimeTarget)
        {
            target = runtimeTarget;
        }

        public void ResetRuntimeState(bool clearTarget = true)
        {
            currentAimDirection = Vector3.forward;
            lockedChargeDirection = Vector3.forward;
            stateRemaining = 0f;
            cooldownRemaining = 0f;
            State = ChargeState.Idle;
            hitTargetsThisCharge.Clear();
            telegraphController?.EndTelegraph();
            if (clearTarget)
            {
                target = null;
            }

            if (cachedRigidbody == null)
            {
                cachedRigidbody = GetComponent<Rigidbody>();
            }

            if (cachedRigidbody != null)
            {
                cachedRigidbody.linearVelocity = Vector3.zero;
                cachedRigidbody.angularVelocity = Vector3.zero;
            }
        }

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

        public void Advance(float deltaTime)
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Mathf.Max(0f, deltaTime));

            if (!IsAlive())
            {
                return;
            }

            switch (State)
            {
                case ChargeState.Telegraphing:
                    TickTelegraph(deltaTime);
                    break;
                case ChargeState.Charging:
                    TickCharge(deltaTime);
                    break;
                case ChargeState.Recovery:
                    TickRecovery(deltaTime);
                    break;
                default:
                    if (autoStartCharges)
                    {
                        TryBeginChargeWindup();
                    }

                    break;
            }
        }

        public bool TryBeginChargeWindup()
        {
            if (cooldownRemaining > 0f || target == null || !IsAlive())
            {
                return false;
            }

            var distance = HorizontalDistance(transform.position, target.position);
            if (distance > ChargeRange)
            {
                return false;
            }

            currentAimDirection = ResolveDirection(transform.position, target.position, transform.forward);
            stateRemaining = WindupDuration;
            State = ChargeState.Telegraphing;
            telegraphController?.BeginTelegraph(config != null ? config.TelegraphConfig : null, EnemyTelegraphType.ChargePath, WindupDuration, transform, target);
            return true;
        }

        private void TickTelegraph(float deltaTime)
        {
            if (target != null)
            {
                currentAimDirection = ResolveDirection(transform.position, target.position, currentAimDirection);
            }

            stateRemaining = Mathf.Max(0f, stateRemaining - Mathf.Max(0f, deltaTime));
            if (stateRemaining > 0f)
            {
                return;
            }

            lockedChargeDirection = currentAimDirection.sqrMagnitude > 0f ? currentAimDirection.normalized : transform.forward;
            hitTargetsThisCharge.Clear();
            stateRemaining = Mathf.Max(0.01f, ChargeDuration);
            State = ChargeState.Charging;
            telegraphController?.EndTelegraph();
        }

        private void TickCharge(float deltaTime)
        {
            stateRemaining = Mathf.Max(0f, stateRemaining - Mathf.Max(0f, deltaTime));
            if (stateRemaining > 0f)
            {
                return;
            }

            BeginRecovery();
        }

        private void TickRecovery(float deltaTime)
        {
            stateRemaining = Mathf.Max(0f, stateRemaining - Mathf.Max(0f, deltaTime));
            if (stateRemaining > 0f)
            {
                return;
            }

            State = ChargeState.Idle;
        }

        private void BeginRecovery()
        {
            State = ChargeState.Recovery;
            stateRemaining = RecoveryDuration;
            cooldownRemaining = Cooldown;
            hitTargetsThisCharge.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            TryDamageChargeTarget(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision != null)
            {
                TryDamageChargeTarget(collision.collider);
            }
        }

        private bool TryDamageChargeTarget(Collider other)
        {
            if (State != ChargeState.Charging || other == null || other.transform.IsChildOf(transform))
            {
                return false;
            }

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return false;
            }

            var targetObject = damageable.GameObject != null ? damageable.GameObject : other.gameObject;
            if (!hitTargetsThisCharge.Add(targetObject))
            {
                return false;
            }

            var hitContext = new HitContext(gameObject, targetObject, Damage, DamageType.Physical)
            {
                HitDirection = lockedChargeDirection,
                HitPoint = other.ClosestPoint(transform.position)
            };
            damageable.ReceiveHit(hitContext);
            RaiseDamageEvents(hitContext);
            BeginRecovery();
            return true;
        }

        private bool IsAlive()
        {
            return enemyHealth == null || enemyHealth.IsAlive;
        }

        private static void RaiseDamageEvents(HitContext hitContext)
        {
            CombatEvents.RaiseHitResolved(hitContext);
            if (hitContext.WasIgnored)
            {
                return;
            }

            var damageEvent = new DamageEvent(hitContext.Source, hitContext.Target, hitContext.DamageAmount, hitContext.DamageType, hitContext);
            CombatEvents.RaiseDamageDealt(damageEvent);
            CombatEvents.RaiseDamageReceived(damageEvent);
        }

        public static Vector3 ResolveDirection(Vector3 sourcePosition, Vector3 targetPosition, Vector3 fallback)
        {
            var direction = targetPosition - sourcePosition;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
            {
                return direction.normalized;
            }

            fallback.y = 0f;
            return fallback.sqrMagnitude > 0.0001f ? fallback.normalized : Vector3.forward;
        }

        private static float HorizontalDistance(Vector3 sourcePosition, Vector3 targetPosition)
        {
            var offset = targetPosition - sourcePosition;
            offset.y = 0f;
            return offset.magnitude;
        }
    }
}
