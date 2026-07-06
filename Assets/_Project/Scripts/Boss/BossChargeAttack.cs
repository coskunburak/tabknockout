using System.Collections.Generic;
using TapKnockout.Combat;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Boss
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BossChargeAttack : MonoBehaviour
    {
        [SerializeField] private EnemyTelegraphController telegraphController;
        [SerializeField] private Transform target;
        [SerializeField, Min(0f)] private float fallbackChargeSpeed = 8f;
        [SerializeField, Min(0f)] private float fallbackDamage = 18f;

        private readonly HashSet<GameObject> hitTargetsThisCharge = new HashSet<GameObject>();
        private Rigidbody cachedRigidbody;
        private Vector3 lockedDirection = Vector3.forward;
        private float remainingChargeTime;
        private float runtimeSpeedMultiplier = 1f;
        private float currentChargeSpeed;
        private float currentDamage;

        public bool IsCharging => remainingChargeTime > 0f;
        public Vector3 LockedDirection => lockedDirection;
        public int HitCountThisCharge => hitTargetsThisCharge.Count;

        private void Awake()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            if (telegraphController == null)
            {
                telegraphController = GetComponentInChildren<EnemyTelegraphController>(true);
            }
        }

        private void Update()
        {
            if (remainingChargeTime > 0f)
            {
                remainingChargeTime = Mathf.Max(0f, remainingChargeTime - Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (!IsCharging || cachedRigidbody == null)
            {
                return;
            }

            var step = lockedDirection * (currentChargeSpeed * Time.fixedDeltaTime);
            var currentPosition = cachedRigidbody.position;
            var nextPosition = currentPosition + step;
            nextPosition.y = currentPosition.y;
            cachedRigidbody.MovePosition(nextPosition);
        }

        public void SetTarget(Transform chargeTarget)
        {
            target = chargeTarget;
        }

        public void SetRuntimeSpeedMultiplier(float multiplier)
        {
            runtimeSpeedMultiplier = Mathf.Max(0.1f, multiplier);
        }

        public void BeginTelegraph(BossAttackStep step, Transform chargeTarget)
        {
            target = chargeTarget != null ? chargeTarget : target;
            lockedDirection = ResolveDirection(transform.position, target != null ? target.position : transform.position + transform.forward, transform.forward);
            telegraphController?.BeginTelegraph(null, EnemyTelegraphType.ChargePath, step.WindupDuration, transform, target);
        }

        public void Execute(BossAttackStep step, Transform chargeTarget)
        {
            target = chargeTarget != null ? chargeTarget : target;
            lockedDirection = ResolveDirection(transform.position, target != null ? target.position : transform.position + lockedDirection, lockedDirection);
            currentChargeSpeed = ResolveChargeSpeed(step.ChargeSpeed);
            currentDamage = (step.Damage > 0f ? step.Damage : fallbackDamage) * step.DamageMultiplier;
            remainingChargeTime = Mathf.Max(0.01f, step.ActiveDuration);
            hitTargetsThisCharge.Clear();
            telegraphController?.EndTelegraph();
        }

        public void EndTelegraph()
        {
            telegraphController?.EndTelegraph();
        }

        private void OnTriggerEnter(Collider other)
        {
            TryDamageTarget(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision != null)
            {
                TryDamageTarget(collision.collider);
            }
        }

        private bool TryDamageTarget(Collider other)
        {
            if (!IsCharging || other == null || other.transform.IsChildOf(transform))
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

            var hitContext = new HitContext(gameObject, targetObject, currentDamage > 0f ? currentDamage : fallbackDamage, DamageType.Impact)
            {
                HitDirection = lockedDirection,
                HitPoint = other.ClosestPoint(transform.position)
            };
            damageable.ReceiveHit(hitContext);
            RaiseDamageEvents(hitContext);
            return true;
        }

        private float ResolveChargeSpeed(float configuredSpeed)
        {
            var speed = configuredSpeed > 0f ? configuredSpeed : fallbackChargeSpeed;
            return speed * runtimeSpeedMultiplier;
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
    }
}
