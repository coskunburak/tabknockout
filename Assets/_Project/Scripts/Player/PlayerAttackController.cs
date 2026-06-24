using TapKnockout.Combat;
using TapKnockout.Projectile;
using UnityEngine;

namespace TapKnockout.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMovementController))]
    [RequireComponent(typeof(PlayerTargetProvider))]
    public sealed class PlayerAttackController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerTargetProvider targetProvider;
        [SerializeField] private WeaponConfig weaponConfig;
        [SerializeField] private PlayerRuntimeStats runtimeStats;
        [SerializeField] private Transform projectileSpawnPoint;

        [Header("Behavior")]
        [SerializeField] private bool faceTargetOnAttack = true;
        [SerializeField] private bool allowDirectHitFallback = true;
        [SerializeField] private bool logSetupWarnings = true;

        private float cooldownRemaining;
        private bool loggedMissingWeapon;
        private bool loggedMissingTargetLayers;
        private bool loggedMissingProjectileController;

        public bool IsCooldownReady => cooldownRemaining <= 0f;
        public WeaponConfig WeaponConfig => weaponConfig;
        public float EffectiveAttackDamage => weaponConfig != null
            ? weaponConfig.AttackDamage * (runtimeStats != null ? runtimeStats.AttackDamageMultiplier : 1f)
            : 0f;
        public float EffectiveAttackCooldown => weaponConfig != null
            ? Mathf.Max(0.01f, weaponConfig.AttackCooldown * (runtimeStats != null ? runtimeStats.AttackCooldownMultiplier : 1f))
            : 0f;

        private void Reset()
        {
            movementController = GetComponent<PlayerMovementController>();
            targetProvider = GetComponent<PlayerTargetProvider>();
            runtimeStats = GetComponent<PlayerRuntimeStats>();
            projectileSpawnPoint = transform;
        }

        private void Awake()
        {
            ResolveReferences();

            if (projectileSpawnPoint == null)
            {
                projectileSpawnPoint = transform;
            }
        }

        private void Update()
        {
            TickCooldown(Time.deltaTime);

            if (!CanAttack())
            {
                return;
            }

            if (!targetProvider.TryGetNearestTarget(weaponConfig.AttackRange, weaponConfig.TargetLayers, out var target))
            {
                return;
            }

            Attack(target);
        }

        public void SetWeaponConfig(WeaponConfig config)
        {
            weaponConfig = config;
            cooldownRemaining = 0f;
            loggedMissingWeapon = false;
            loggedMissingTargetLayers = false;
        }

        private void ResolveReferences()
        {
            if (movementController == null)
            {
                movementController = GetComponent<PlayerMovementController>();
            }

            if (targetProvider == null)
            {
                targetProvider = GetComponent<PlayerTargetProvider>();
            }

            if (runtimeStats == null)
            {
                runtimeStats = GetComponent<PlayerRuntimeStats>();
            }
        }

        private void TickCooldown(float deltaTime)
        {
            if (cooldownRemaining > 0f)
            {
                cooldownRemaining -= deltaTime;
            }
        }

        private bool CanAttack()
        {
            ResolveReferences();

            if (movementController == null || targetProvider == null)
            {
                return false;
            }

            if (weaponConfig == null)
            {
                if (logSetupWarnings && !loggedMissingWeapon)
                {
                    loggedMissingWeapon = true;
                    Debug.LogWarning($"{nameof(PlayerAttackController)} on {name} has no WeaponConfig assigned.", this);
                }

                return false;
            }

            if (weaponConfig.TargetLayers.value == 0)
            {
                if (logSetupWarnings && !loggedMissingTargetLayers)
                {
                    loggedMissingTargetLayers = true;
                    Debug.LogWarning($"{nameof(WeaponConfig)} '{weaponConfig.name}' has no TargetLayers set.", weaponConfig);
                }

                return false;
            }

            return IsCooldownReady && !movementController.IsMovingAboveAttackThreshold;
        }

        private void Attack(TargetingResult target)
        {
            if (!target.HasTarget)
            {
                return;
            }

            var attackDirection = target.Direction.sqrMagnitude > 0f
                ? target.Direction
                : GetFallbackAttackDirection();

            if (faceTargetOnAttack)
            {
                FaceDirection(attackDirection);
            }

            var hasProjectilePrefab = weaponConfig.ProjectilePrefab != null;
            var hitContext = CreateHitContext(target, attackDirection, hasProjectilePrefab);

            if (hasProjectilePrefab && TrySpawnProjectile(hitContext, attackDirection))
            {
                StartCooldown();
                return;
            }

            if (allowDirectHitFallback && TryResolveDirectHit(target, hitContext))
            {
                StartCooldown();
            }
        }

        private HitContext CreateHitContext(TargetingResult target, Vector3 attackDirection, bool isProjectile)
        {
            return new HitContext(gameObject, target.TargetGameObject, EffectiveAttackDamage, weaponConfig.DamageType)
            {
                CriticalChance = weaponConfig.CriticalChance,
                CriticalMultiplier = weaponConfig.CriticalMultiplier,
                IsProjectileHit = isProjectile,
                HitDirection = attackDirection,
                HitPoint = target.TargetTransform != null ? target.TargetTransform.position : transform.position
            };
        }

        private bool TrySpawnProjectile(HitContext hitContext, Vector3 attackDirection)
        {
            var spawnTransform = projectileSpawnPoint != null ? projectileSpawnPoint : transform;
            var rotation = Quaternion.LookRotation(attackDirection, Vector3.up);
            var projectileObject = Instantiate(weaponConfig.ProjectilePrefab, spawnTransform.position, rotation);

            if (!projectileObject.TryGetComponent<ProjectileController>(out var projectileController))
            {
                if (logSetupWarnings && !loggedMissingProjectileController)
                {
                    loggedMissingProjectileController = true;
                    Debug.LogWarning(
                        $"{weaponConfig.ProjectilePrefab.name} has no {nameof(ProjectileController)}. Direct hit fallback will be used if enabled.",
                        weaponConfig.ProjectilePrefab);
                }

                Destroy(projectileObject);
                return false;
            }

            projectileController.Initialize(
                hitContext,
                attackDirection,
                weaponConfig.ProjectileSpeed,
                weaponConfig.ProjectileLifetime,
                gameObject);

            return true;
        }

        private bool TryResolveDirectHit(TargetingResult target, HitContext hitContext)
        {
            if (target.Damageable == null || !target.Damageable.IsAlive)
            {
                return false;
            }

            hitContext.IsProjectileHit = false;
            target.Damageable.ReceiveHit(hitContext);
            RaiseHitEvents(hitContext);
            return true;
        }

        private void FaceDirection(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private Vector3 GetFallbackAttackDirection()
        {
            if (movementController != null && movementController.LastFacingDirection.sqrMagnitude > 0f)
            {
                return movementController.LastFacingDirection.normalized;
            }

            var forward = transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0f ? forward.normalized : Vector3.forward;
        }

        private void StartCooldown()
        {
            cooldownRemaining = EffectiveAttackCooldown;
        }

        private static void RaiseHitEvents(HitContext hitContext)
        {
            CombatEvents.RaiseHitResolved(hitContext);

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
