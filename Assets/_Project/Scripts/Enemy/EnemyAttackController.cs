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

        [Header("Telegraph")]
        [SerializeField] private EnemyAttackTelegraphConfig telegraphConfig;
        [SerializeField] private EnemyTelegraphController telegraphController;
        [SerializeField] private bool useTelegraphWindup;
        [SerializeField, Min(0f)] private float fallbackWindupDuration = 0.25f;
        [SerializeField, Min(0f)] private float fallbackCancelledRetryDelay = 0.15f;

        [Header("Fallback")]
        [SerializeField, Min(0f)] private float fallbackAttackRange = 1.2f;
        [SerializeField, Min(0.01f)] private float fallbackAttackCooldown = 1f;
        [SerializeField, Min(0f)] private float fallbackContactDamage = 8f;

        [Header("Debug")]
        [SerializeField] private bool logAttacks;

        private float cooldownRemaining;
        private float windupRemaining;
        private EnemyHealth enemyHealth;
        private IDamageable targetDamageable;
        private Transform windupTarget;
        private IDamageable windupDamageable;
        private bool isWindingUp;

        public bool IsCooldownReady => cooldownRemaining <= 0f;
        public bool CanAttack => enabled &&
            IsCooldownReady &&
            !isWindingUp &&
            (enemyHealth == null || enemyHealth.IsAlive) &&
            targetDamageable != null &&
            targetDamageable.IsAlive;
        public Transform Target => target;
        public bool IsWindingUp => isWindingUp;
        public float WindupRemaining => windupRemaining;
        public float CooldownRemaining => cooldownRemaining;

        private float AttackRange => config != null ? config.AttackRange : fallbackAttackRange;
        private float AttackCooldown => config != null ? config.AttackCooldown : fallbackAttackCooldown;
        private float ContactDamage => config != null ? config.ContactDamage : fallbackContactDamage;
        private bool UseTelegraphWindup => useTelegraphWindup || telegraphConfig != null && telegraphConfig.EnabledByDefault;
        private float AttackWindupDuration => telegraphConfig != null ? telegraphConfig.WindupDuration : fallbackWindupDuration;
        private float CancelledRetryDelay => telegraphConfig != null ? telegraphConfig.CancelledRetryDelay : fallbackCancelledRetryDelay;

        private void Awake()
        {
            enemyHealth = GetComponent<EnemyHealth>();
            if (telegraphController == null)
            {
                telegraphController = GetComponentInChildren<EnemyTelegraphController>(true);
            }
        }

        private void OnValidate()
        {
            fallbackAttackRange = Mathf.Max(0f, fallbackAttackRange);
            fallbackAttackCooldown = Mathf.Max(0.01f, fallbackAttackCooldown);
            fallbackContactDamage = Mathf.Max(0f, fallbackContactDamage);
            fallbackWindupDuration = Mathf.Max(0f, fallbackWindupDuration);
            fallbackCancelledRetryDelay = Mathf.Max(0f, fallbackCancelledRetryDelay);
        }

        private void Update()
        {
            TickCooldown(Time.deltaTime);

            if (isWindingUp)
            {
                TickWindup(Time.deltaTime);
                return;
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

            if (UseTelegraphWindup && AttackWindupDuration > 0f)
            {
                BeginWindup();
                return true;
            }

            StartCooldown();
            return true;
        }

        public bool TryDealContactDamage()
        {
            ResolveTargetDamageableIfNeeded();

            if (!CanAttack || !IsTargetInRange())
            {
                return false;
            }

            if (UseTelegraphWindup && AttackWindupDuration > 0f)
            {
                BeginWindup();
                return true;
            }

            ApplyContactDamage(target, targetDamageable);
            StartCooldown();
            return true;
        }

        private void ApplyContactDamage(Transform hitTarget, IDamageable hitDamageable)
        {
            if (hitTarget == null || hitDamageable == null)
            {
                return;
            }

            var targetObject = hitDamageable.GameObject != null ? hitDamageable.GameObject : hitTarget.gameObject;
            var hitContext = new HitContext(gameObject, targetObject, ContactDamage, DamageType.Physical)
            {
                HitDirection = ResolveHitDirection(hitTarget),
                HitPoint = hitTarget.position
            };

            EnemyAttackEvents.RaiseAttackReleased(new EnemyAttackEventArgs(
                EnemyAttackPhase.AttackReleased,
                gameObject,
                targetObject,
                hitContext.HitPoint,
                0f,
                AttackCooldown));

            hitDamageable.ReceiveHit(hitContext);
            RaiseHitEvents(hitContext);

            if (logAttacks)
            {
                Debug.Log(
                    $"{nameof(EnemyAttackController)} on {name} dealt {ContactDamage:0.##} contact damage to {targetObject.name}.",
                    this);
            }
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

        private void BeginWindup()
        {
            windupTarget = target;
            windupDamageable = targetDamageable;
            windupRemaining = AttackWindupDuration;
            isWindingUp = true;
            telegraphController?.BeginTelegraph(AttackWindupDuration);

            var targetObject = windupDamageable != null && windupDamageable.GameObject != null
                ? windupDamageable.GameObject
                : windupTarget != null ? windupTarget.gameObject : null;

            EnemyAttackEvents.RaiseTelegraphStarted(new EnemyAttackEventArgs(
                EnemyAttackPhase.TelegraphStarted,
                gameObject,
                targetObject,
                transform.position,
                AttackWindupDuration,
                AttackCooldown));
        }

        private void TickWindup(float deltaTime)
        {
            windupRemaining = Mathf.Max(0f, windupRemaining - Mathf.Max(0f, deltaTime));
            if (windupRemaining > 0f)
            {
                return;
            }

            CompleteWindup();
        }

        private void CompleteWindup()
        {
            telegraphController?.EndTelegraph();
            isWindingUp = false;

            if (windupTarget == null || windupDamageable == null || !windupDamageable.IsAlive || !IsTargetInRange(transform.position, windupTarget.position, AttackRange))
            {
                StartCancelledRetryDelay();
                ClearWindup();
                return;
            }

            ApplyContactDamage(windupTarget, windupDamageable);
            StartCooldown();
            ClearWindup();
        }

        private void StartCancelledRetryDelay()
        {
            cooldownRemaining = CancelledRetryDelay;

            var targetObject = windupDamageable != null && windupDamageable.GameObject != null
                ? windupDamageable.GameObject
                : windupTarget != null ? windupTarget.gameObject : null;

            EnemyAttackEvents.RaiseTelegraphCancelled(new EnemyAttackEventArgs(
                EnemyAttackPhase.TelegraphCancelled,
                gameObject,
                targetObject,
                transform.position,
                0f,
                cooldownRemaining));
        }

        private void StartCooldown()
        {
            cooldownRemaining = AttackCooldown;
            EnemyAttackEvents.RaiseCooldownStarted(new EnemyAttackEventArgs(
                EnemyAttackPhase.CooldownStarted,
                gameObject,
                targetDamageable != null ? targetDamageable.GameObject : null,
                transform.position,
                0f,
                AttackCooldown));
        }

        private void TickCooldown(float deltaTime)
        {
            if (cooldownRemaining <= 0f)
            {
                return;
            }

            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Mathf.Max(0f, deltaTime));
            if (cooldownRemaining > 0f)
            {
                return;
            }

            EnemyAttackEvents.RaiseReady(new EnemyAttackEventArgs(
                EnemyAttackPhase.Ready,
                gameObject,
                targetDamageable != null ? targetDamageable.GameObject : null,
                transform.position,
                0f,
                0f));
        }

        private void ClearWindup()
        {
            windupTarget = null;
            windupDamageable = null;
            windupRemaining = 0f;
        }
    }
}
