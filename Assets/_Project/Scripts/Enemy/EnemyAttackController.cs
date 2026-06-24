using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemyAttackController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private EnemyConfig config;

        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private bool autoDealContactDamage = true;

        [Header("Fallback")]
        [SerializeField, Min(0f)] private float fallbackAttackRange = 1.2f;
        [SerializeField, Min(0.01f)] private float fallbackAttackCooldown = 1f;
        [SerializeField, Min(0f)] private float fallbackContactDamage = 8f;

        [Header("Debug")]
        [SerializeField] private bool logAttacks;

        private float cooldownRemaining;
        private EnemyHealth enemyHealth;
        private IDamageable targetDamageable;

        public bool IsCooldownReady => cooldownRemaining <= 0f;
        public bool CanAttack => enabled &&
            IsCooldownReady &&
            (enemyHealth == null || enemyHealth.IsAlive) &&
            targetDamageable != null &&
            targetDamageable.IsAlive;
        public Transform Target => target;

        private float AttackRange => config != null ? config.AttackRange : fallbackAttackRange;
        private float AttackCooldown => config != null ? config.AttackCooldown : fallbackAttackCooldown;
        private float ContactDamage => config != null ? config.ContactDamage : fallbackContactDamage;

        private void Awake()
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }

        private void OnValidate()
        {
            fallbackAttackRange = Mathf.Max(0f, fallbackAttackRange);
            fallbackAttackCooldown = Mathf.Max(0.01f, fallbackAttackCooldown);
            fallbackContactDamage = Mathf.Max(0f, fallbackContactDamage);
        }

        private void Update()
        {
            if (cooldownRemaining > 0f)
            {
                cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
            }

            if (autoDealContactDamage)
            {
                TryDealContactDamage();
            }
        }

        public void Initialize(EnemyConfig enemyConfig, Transform attackTarget)
        {
            config = enemyConfig;
            SetTarget(attackTarget);
            cooldownRemaining = 0f;
        }

        public void SetTarget(Transform attackTarget)
        {
            target = attackTarget;
            targetDamageable = ResolveDamageable(attackTarget);
        }

        public bool IsTargetInRange()
        {
            return target != null && IsTargetInRange(transform.position, target.position, AttackRange);
        }

        public bool TryBeginAttack()
        {
            if (!CanAttack || !IsTargetInRange())
            {
                return false;
            }

            cooldownRemaining = AttackCooldown;
            return true;
        }

        public bool TryDealContactDamage()
        {
            ResolveTargetDamageableIfNeeded();

            if (!TryBeginAttack())
            {
                return false;
            }

            var targetObject = targetDamageable.GameObject != null ? targetDamageable.GameObject : target.gameObject;
            var hitContext = new HitContext(gameObject, targetObject, ContactDamage, DamageType.Physical)
            {
                HitDirection = ResolveHitDirection(target),
                HitPoint = target != null ? target.position : transform.position
            };

            targetDamageable.ReceiveHit(hitContext);
            RaiseHitEvents(hitContext);

            if (logAttacks)
            {
                Debug.Log(
                    $"{nameof(EnemyAttackController)} on {name} dealt {ContactDamage:0.##} contact damage to {targetObject.name}.",
                    this);
            }

            return true;
        }

        public static bool IsTargetInRange(Vector3 sourcePosition, Vector3 targetPosition, float attackRange)
        {
            var offset = targetPosition - sourcePosition;
            offset.y = 0f;
            return offset.sqrMagnitude <= Mathf.Max(0f, attackRange) * Mathf.Max(0f, attackRange);
        }

        private void ResolveTargetDamageableIfNeeded()
        {
            if (targetDamageable == null && target != null)
            {
                targetDamageable = ResolveDamageable(target);
            }
        }

        private static IDamageable ResolveDamageable(Transform candidate)
        {
            return candidate != null ? candidate.GetComponentInParent<IDamageable>() : null;
        }

        private Vector3 ResolveHitDirection(Transform hitTarget)
        {
            if (hitTarget == null)
            {
                return transform.forward;
            }

            var direction = hitTarget.position - transform.position;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;
        }

        private static void RaiseHitEvents(HitContext hitContext)
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
