using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    public sealed class AreaBomberController : MonoBehaviour, IEnemyRuntimeConfigReceiver, IEnemyRuntimeTargetReceiver, IPoolLifecycle
    {
        public enum BomberState
        {
            Idle = 0,
            Windup = 1,
            Cooldown = 2
        }

        [SerializeField] private EnemyConfig config;
        [SerializeField] private Transform target;
        [SerializeField] private EnemyTelegraphController telegraphController;
        [SerializeField] private bool autoBomb = true;
        [SerializeField] private LayerMask damageLayers = ~0;
        [SerializeField, Range(4, 32)] private int overlapBufferSize = 12;
        [SerializeField, Min(0f)] private float fallbackWindupDuration = 0.7f;
        [SerializeField, Min(0f)] private float fallbackCooldown = 1.8f;
        [SerializeField, Min(0f)] private float fallbackRadius = 1.5f;
        [SerializeField, Min(0f)] private float fallbackDamage = 12f;

        private Collider[] overlapBuffer;
        private Vector3 lockedBombPosition;
        private float stateRemaining;
        private float cooldownRemaining;

        public BomberState State { get; private set; }
        public Vector3 LockedBombPosition => lockedBombPosition;

        private float WindupDuration => config != null ? Mathf.Max(config.AttackWindup, fallbackWindupDuration) : fallbackWindupDuration;
        private float Cooldown => config != null ? config.AttackCooldown : fallbackCooldown;
        private float Radius => config != null ? config.ExplosionRadius : fallbackRadius;
        private float Damage => config != null ? config.ContactDamage : fallbackDamage;

        private void Awake()
        {
            if (telegraphController == null)
            {
                telegraphController = GetComponentInChildren<EnemyTelegraphController>(true);
            }

            EnsureBuffer();
        }

        private void OnValidate()
        {
            overlapBufferSize = Mathf.Clamp(overlapBufferSize, 4, 32);
        }

        private void Update()
        {
            Advance(Time.deltaTime);
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
            lockedBombPosition = Vector3.zero;
            stateRemaining = 0f;
            cooldownRemaining = 0f;
            State = BomberState.Idle;
            telegraphController?.EndTelegraph();
            if (clearTarget)
            {
                target = null;
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

            switch (State)
            {
                case BomberState.Windup:
                    TickWindup(deltaTime);
                    break;
                case BomberState.Cooldown:
                    TickCooldown(deltaTime);
                    break;
                default:
                    if (autoBomb)
                    {
                        TryBeginBomb(target != null ? target.position : transform.position);
                    }

                    break;
            }
        }

        public bool TryBeginBomb(Vector3 targetPosition)
        {
            if (cooldownRemaining > 0f || State != BomberState.Idle)
            {
                return false;
            }

            lockedBombPosition = targetPosition;
            lockedBombPosition.y = transform.position.y;
            stateRemaining = WindupDuration;
            State = BomberState.Windup;
            telegraphController?.BeginTelegraphAtPosition(
                config != null ? config.TelegraphConfig : null,
                EnemyTelegraphType.Circle,
                WindupDuration,
                lockedBombPosition,
                Quaternion.identity);
            return true;
        }

        private void TickWindup(float deltaTime)
        {
            stateRemaining = Mathf.Max(0f, stateRemaining - Mathf.Max(0f, deltaTime));
            if (stateRemaining > 0f)
            {
                return;
            }

            ResolveAreaDamage();
            telegraphController?.EndTelegraph();
            State = BomberState.Cooldown;
            stateRemaining = Cooldown;
            cooldownRemaining = Cooldown;
        }

        private void TickCooldown(float deltaTime)
        {
            stateRemaining = Mathf.Max(0f, stateRemaining - Mathf.Max(0f, deltaTime));
            if (stateRemaining <= 0f)
            {
                State = BomberState.Idle;
            }
        }

        private void ResolveAreaDamage()
        {
            EnsureBuffer();
            var count = Physics.OverlapSphereNonAlloc(lockedBombPosition, Radius, overlapBuffer, damageLayers, QueryTriggerInteraction.Collide);
            for (var i = 0; i < count; i++)
            {
                var damageable = overlapBuffer[i] != null ? overlapBuffer[i].GetComponentInParent<IDamageable>() : null;
                if (damageable == null || !damageable.IsAlive || damageable.GameObject == gameObject)
                {
                    continue;
                }

                var targetObject = damageable.GameObject != null ? damageable.GameObject : overlapBuffer[i].gameObject;
                var direction = targetObject.transform.position - lockedBombPosition;
                direction.y = 0f;
                var hitContext = new HitContext(gameObject, targetObject, Damage, DamageType.Physical)
                {
                    HitPoint = lockedBombPosition,
                    HitDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward
                };
                damageable.ReceiveHit(hitContext);
                RaiseDamageEvents(hitContext);
            }
        }

        private void EnsureBuffer()
        {
            if (overlapBuffer == null || overlapBuffer.Length != overlapBufferSize)
            {
                overlapBuffer = new Collider[overlapBufferSize];
            }
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

        public static bool ShouldResolveDamageAfterTick(float remainingWindup, float deltaTime)
        {
            return Mathf.Max(0f, remainingWindup) > 0f && Mathf.Max(0f, remainingWindup) - Mathf.Max(0f, deltaTime) <= 0f;
        }
    }
}
