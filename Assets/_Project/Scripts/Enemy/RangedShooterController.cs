using TapKnockout.Combat;
using TapKnockout.Projectile;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    public sealed class RangedShooterController : MonoBehaviour, IEnemyRuntimeConfigReceiver, IEnemyRuntimeTargetReceiver, IPoolLifecycle
    {
        public enum ShooterState
        {
            Idle = 0,
            Windup = 1,
            Cooldown = 2
        }

        [SerializeField] private EnemyConfig config;
        [SerializeField] private Transform target;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private EnemyTelegraphController telegraphController;
        [SerializeField] private bool autoShoot = true;
        [SerializeField, Min(0f)] private float fallbackAttackRange = 7f;
        [SerializeField, Min(0f)] private float fallbackWindupDuration = 0.35f;
        [SerializeField, Min(0f)] private float fallbackCooldown = 1.25f;
        [SerializeField, Min(0f)] private float fallbackProjectileSpeed = 8f;
        [SerializeField, Min(0f)] private float projectileLifetime = 4f;

        private float stateRemaining;
        private float cooldownRemaining;

        public ShooterState State { get; private set; }

        private float AttackRange => config != null ? Mathf.Max(fallbackAttackRange, config.AttackRange) : fallbackAttackRange;
        private float WindupDuration => config != null ? Mathf.Max(config.AttackWindup, fallbackWindupDuration) : fallbackWindupDuration;
        private float Cooldown => config != null ? config.AttackCooldown : fallbackCooldown;
        private float ProjectileSpeed => config != null ? config.ProjectileSpeed : fallbackProjectileSpeed;
        private float Damage => config != null ? config.ContactDamage : 6f;

        private void Awake()
        {
            if (projectileSpawnPoint == null)
            {
                projectileSpawnPoint = transform;
            }

            if (telegraphController == null)
            {
                telegraphController = GetComponentInChildren<EnemyTelegraphController>(true);
            }
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
            stateRemaining = 0f;
            cooldownRemaining = 0f;
            State = ShooterState.Idle;
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
                case ShooterState.Windup:
                    TickWindup(deltaTime);
                    break;
                case ShooterState.Cooldown:
                    TickCooldown(deltaTime);
                    break;
                default:
                    if (autoShoot)
                    {
                        TryBeginShot();
                    }

                    break;
            }
        }

        public bool TryBeginShot()
        {
            if (cooldownRemaining > 0f || target == null || !IsTargetInRange())
            {
                return false;
            }

            stateRemaining = WindupDuration;
            State = ShooterState.Windup;
            telegraphController?.BeginTelegraph(config != null ? config.TelegraphConfig : null, EnemyTelegraphType.Line, WindupDuration, transform, target);
            return true;
        }

        public EnemyProjectileRequest CreateProjectileRequest()
        {
            var origin = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
            var direction = target != null ? target.position - origin : transform.forward;
            return new EnemyProjectileRequest(gameObject, target != null ? target.gameObject : null, origin, direction, Damage, ProjectileSpeed, projectileLifetime);
        }

        private void TickWindup(float deltaTime)
        {
            stateRemaining = Mathf.Max(0f, stateRemaining - Mathf.Max(0f, deltaTime));
            if (stateRemaining > 0f)
            {
                return;
            }

            FireProjectile();
            telegraphController?.EndTelegraph();
            State = ShooterState.Cooldown;
            stateRemaining = Cooldown;
            cooldownRemaining = Cooldown;
        }

        private void TickCooldown(float deltaTime)
        {
            stateRemaining = Mathf.Max(0f, stateRemaining - Mathf.Max(0f, deltaTime));
            if (stateRemaining <= 0f)
            {
                State = ShooterState.Idle;
            }
        }

        private void FireProjectile()
        {
            if (config == null || config.ProjectilePrefab == null)
            {
                return;
            }

            var request = CreateProjectileRequest();
            if (!request.CanSpawn)
            {
                return;
            }

            var spawned = ProjectilePoolService.Shared.Spawn(
                config.ProjectilePrefab,
                request.Origin,
                Quaternion.LookRotation(request.Direction, Vector3.up));
            var enemyProjectile = spawned.GetComponent<EnemyProjectileController>();
            if (enemyProjectile != null)
            {
                enemyProjectile.Initialize(request);
                return;
            }

            var genericProjectile = spawned.GetComponent<ProjectileController>();
            genericProjectile?.Initialize(request.CreateHitContext(), request.Direction, request.Speed, request.Lifetime, gameObject);
        }

        private bool IsTargetInRange()
        {
            if (target == null)
            {
                return false;
            }

            var offset = target.position - transform.position;
            offset.y = 0f;
            return offset.sqrMagnitude <= AttackRange * AttackRange;
        }
    }
}
