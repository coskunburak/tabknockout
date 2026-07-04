using System;
using System.Collections.Generic;
using TapKnockout.Camera;
using TapKnockout.Combat;
using TapKnockout.Input;
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
        [SerializeField] private PlayerConfig playerConfig;
        [SerializeField] private DesktopInputReader desktopInputReader;
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerTargetProvider targetProvider;
        [SerializeField] private WeaponConfig weaponConfig;
        [SerializeField] private PlayerRuntimeStats runtimeStats;
        [SerializeField] private MouseAimController mouseAimController;
        [SerializeField] private MouseAimReticleController aimReticle;
        [SerializeField] private Transform projectileSpawnPoint;

        [Header("Behavior")]
        [SerializeField] private bool faceTargetOnAttack = true;
        [SerializeField] private bool allowDirectHitFallback = true;
        [SerializeField] private bool requireStationaryToAttack;
        [SerializeField] private bool preferMouseAimForProjectiles = true;
        [SerializeField] private bool allowAimFallbackWithoutTarget = true;
        [SerializeField] private bool useProjectilePooling = true;
        [SerializeField] private bool logSetupWarnings = true;

        [Header("Survivor Fire Policy")]
        [SerializeField] private PrimaryAttackFirePolicy firePolicy = PrimaryAttackFirePolicy.HoldMouseAim;
        [SerializeField] private PlayerFacingPolicy facingPolicy = PlayerFacingPolicy.MouseAimDirection;
        [SerializeField] private bool fallbackAttackWhileMoving = true;
        [SerializeField] private bool fallbackManualFireRequiresInput = true;
        [SerializeField, Min(0f)] private float fallbackAimRotationSpeed = 1080f;
        [SerializeField, Min(0f)] private float fallbackNearestTargetRadius = 12f;

        [Header("Shot Feedback")]
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private AudioSource shotAudioSource;
        [SerializeField] private AudioClip shotSfx;
        [SerializeField, Range(0f, 1f)] private float shotSfxVolume = 1f;
        [SerializeField] private CameraShakeReceiver shotCameraShakeReceiver;
        [SerializeField, Range(0f, 0.04f)] private float shotCameraShakeAmplitude;
        [SerializeField, Range(0f, 0.08f)] private float shotCameraShakeDuration;
        [SerializeField] private bool pulseReticleOnFire = true;
        private float cooldownRemaining;
        private bool loggedMissingWeapon;
        private bool loggedMissingTargetLayers;
        private bool loggedMissingProjectileController;
        private readonly List<Vector3> projectileDirections = new List<Vector3>(12);
        private Rigidbody cachedRigidbody;
        private Quaternion pendingFacingRotation;
        private bool hasPendingFacingRotation;

        public bool IsCooldownReady => cooldownRemaining <= 0f;
        public WeaponConfig WeaponConfig => weaponConfig;
        public event Action<Vector3> OnPrimaryAttackFired;

        public float EffectiveAttackDamage => weaponConfig != null
            ? weaponConfig.AttackDamage * (runtimeStats != null ? runtimeStats.AttackDamageMultiplier : 1f)
            : 0f;
        public float EffectiveAttackCooldown => weaponConfig != null
            ? Mathf.Max(0.01f, weaponConfig.AttackCooldown * (runtimeStats != null ? runtimeStats.AttackCooldownMultiplier : 1f))
            : 0f;
        public float EffectiveProjectileSpeed => weaponConfig != null
            ? Mathf.Max(0f, weaponConfig.ProjectileSpeed * (runtimeStats != null ? runtimeStats.ProjectileSpeedMultiplier : 1f))
            : 0f;
        public float EffectiveProjectileSizeMultiplier => runtimeStats != null ? runtimeStats.ProjectileSizeMultiplier : 1f;

        private PrimaryAttackFirePolicy EffectiveFirePolicy =>
            playerConfig != null ? playerConfig.PrimaryAttackFirePolicy : firePolicy;

        private PlayerFacingPolicy EffectiveFacingPolicy =>
            playerConfig != null ? playerConfig.FacingPolicy : facingPolicy;

        private bool AttackWhileMoving =>
            playerConfig != null ? playerConfig.AttackWhileMoving : fallbackAttackWhileMoving;

        private bool ManualFireRequiresInput =>
            playerConfig != null ? playerConfig.ManualFireRequiresInput : fallbackManualFireRequiresInput;

        private float AimRotationSpeed =>
            playerConfig != null ? playerConfig.AimRotationSpeed : fallbackAimRotationSpeed;

        private float NearestTargetRadius =>
            playerConfig != null ? playerConfig.NearestTargetRadius : fallbackNearestTargetRadius;

        private float EffectiveTargetRange =>
            weaponConfig != null && weaponConfig.AttackRange > 0f
                ? weaponConfig.AttackRange
                : NearestTargetRadius;

        private void Reset()
        {
            movementController = GetComponent<PlayerMovementController>();
            playerHealth = GetComponent<PlayerHealth>();
            targetProvider = GetComponent<PlayerTargetProvider>();
            runtimeStats = GetComponent<PlayerRuntimeStats>();
            mouseAimController = GetComponent<MouseAimController>();
            aimReticle = GetComponent<MouseAimReticleController>();
            projectileSpawnPoint = transform;
            desktopInputReader = GetComponent<DesktopInputReader>();
            shotAudioSource = GetComponent<AudioSource>();
            cachedRigidbody = GetComponent<Rigidbody>();
        }

        private void Awake()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            ResolveReferences();

            if (projectileSpawnPoint == null)
            {
                projectileSpawnPoint = transform;
            }
        }

        private void OnValidate()
        {
            fallbackAimRotationSpeed = Mathf.Max(0f, fallbackAimRotationSpeed);
            fallbackNearestTargetRadius = Mathf.Max(0f, fallbackNearestTargetRadius);
            shotSfxVolume = Mathf.Clamp01(shotSfxVolume);
            shotCameraShakeAmplitude = Mathf.Clamp(shotCameraShakeAmplitude, 0f, 0.04f);
            shotCameraShakeDuration = Mathf.Clamp(shotCameraShakeDuration, 0f, 0.08f);
        }

        private void Update()
        {
            TickCooldown(Time.deltaTime);
            UpdateReticleRuntimeState();

            if (!CanAttack())
            {
                return;
            }

            if (!TryResolveAttackIntent(out var target, out var hasTarget, out var attackDirection))
            {
                return;
            }

            Attack(target, hasTarget, attackDirection);
        }

        private void FixedUpdate()
        {
            ApplyPendingFacingRotation();
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

            if (playerHealth == null)
            {
                playerHealth = GetComponent<PlayerHealth>();
            }

            if (runtimeStats == null)
            {
                runtimeStats = GetComponent<PlayerRuntimeStats>();
            }

            if (mouseAimController == null)
            {
                mouseAimController = GetComponent<MouseAimController>();
            }

            if (aimReticle == null)
            {
                aimReticle = GetComponent<MouseAimReticleController>();
            }

            if (desktopInputReader == null)
            {
                desktopInputReader = GetComponent<DesktopInputReader>();
            }

            if (shotAudioSource == null)
            {
                shotAudioSource = GetComponent<AudioSource>();
            }

            if (shotCameraShakeReceiver == null &&
                (shotCameraShakeAmplitude > 0f || shotCameraShakeDuration > 0f))
            {
                shotCameraShakeReceiver = FindFirstObjectByType<CameraShakeReceiver>();
            }

            if (cachedRigidbody == null)
            {
                cachedRigidbody = GetComponent<Rigidbody>();
            }

            if (playerConfig == null && movementController != null)
            {
                playerConfig = movementController.Config;
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

            if (playerHealth != null && !playerHealth.IsAlive)
            {
                return false;
            }

            if (Time.timeScale <= 0f)
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

                if (!CanUseAimFallbackWithoutTarget())
                {
                    return false;
                }
            }

            return IsCooldownReady &&
                (AttackWhileMoving || !requireStationaryToAttack || !movementController.IsMovingAboveAttackThreshold);
        }

        private bool CanUseAimFallbackWithoutTarget()
        {
            return allowAimFallbackWithoutTarget &&
                weaponConfig != null &&
                weaponConfig.ProjectilePrefab != null;
        }

        private bool TryResolveAttackIntent(
            out TargetingResult target,
            out bool hasTarget,
            out Vector3 attackDirection)
        {
            target = TargetingResult.None;
            hasTarget = false;
            attackDirection = Vector3.zero;

            var policy = EffectiveFirePolicy;

            if (IsManualFirePolicy(policy) &&
                ManualFireRequiresInput &&
                !IsPrimaryFireRequested())
            {
                return false;
            }

            if (policy == PrimaryAttackFirePolicy.AutoNearestEnemy ||
                policy == PrimaryAttackFirePolicy.HybridAutoTargetWithMouseFallback)
            {
                hasTarget = TryGetNearestValidTarget(out target);
                if (hasTarget && target.Direction.sqrMagnitude > 0f)
                {
                    attackDirection = target.Direction.normalized;
                    return true;
                }

                if (policy == PrimaryAttackFirePolicy.AutoNearestEnemy)
                {
                    return false;
                }
            }

            if (policy == PrimaryAttackFirePolicy.AutoMouseAim ||
                policy == PrimaryAttackFirePolicy.HoldMouseAim ||
                policy == PrimaryAttackFirePolicy.HybridAutoTargetWithMouseFallback)
            {
                if (!CanUseAimFallbackWithoutTarget())
                {
                    return false;
                }

                attackDirection = ResolveMouseAimOrFallbackDirection();
                return attackDirection.sqrMagnitude > 0f;
            }

            attackDirection = GetFallbackAttackDirection();
            return attackDirection.sqrMagnitude > 0f;
        }
        private bool TryGetNearestValidTarget(out TargetingResult target)
        {
            target = TargetingResult.None;

            if (targetProvider == null ||
                weaponConfig == null ||
                weaponConfig.TargetLayers.value == 0)
            {
                return false;
            }

            return targetProvider.TryGetNearestTarget(
                EffectiveTargetRange,
                weaponConfig.TargetLayers,
                out target);
        }
        private Vector3 ResolveMouseAimOrFallbackDirection()
        {
            if (preferMouseAimForProjectiles &&
                mouseAimController != null &&
                mouseAimController.TryGetAimDirection(out var aimDirection) &&
                aimDirection.sqrMagnitude > 0.0001f)
            {
                aimDirection.y = 0f;
                return aimDirection.normalized;
            }

            return GetForwardFallbackAttackDirection();
        }
        private void Attack(TargetingResult target, bool hasTarget, Vector3 attackDirection)
        {
            if (!hasTarget && !allowAimFallbackWithoutTarget)
            {
                return;
            }

            if (faceTargetOnAttack)
            {
                FaceDirection(attackDirection);
            }

            var hasProjectilePrefab = weaponConfig.ProjectilePrefab != null;
            var hitContext = CreateHitContext(target, attackDirection, hasProjectilePrefab);

            if (hasProjectilePrefab && TrySpawnProjectiles(hitContext, attackDirection))
            {
                CompleteAttack(attackDirection);
                return;
            }

            if (hasTarget && allowDirectHitFallback && TryResolveDirectHit(target, hitContext))
            {
                CompleteAttack(attackDirection);
            }
        }

        private HitContext CreateHitContext(TargetingResult target, Vector3 attackDirection, bool isProjectile)
        {
            return new HitContext(gameObject, target.TargetGameObject, EffectiveAttackDamage, weaponConfig.DamageType)
            {
                CriticalChance = ResolveBaseCriticalChance(),
                CriticalMultiplier = ResolveBaseCriticalMultiplier(),
                IsProjectileHit = isProjectile,
                HitDirection = attackDirection,
                HitPoint = target.TargetTransform != null ? target.TargetTransform.position : transform.position
            };
        }

        private bool TrySpawnProjectiles(HitContext hitContext, Vector3 attackDirection)
        {
            var spawnTransform = projectileSpawnPoint != null ? projectileSpawnPoint : transform;
            var modifierState = CreateProjectileModifierState();
            ProjectilePatternBuilder.BuildDirections(attackDirection, modifierState, projectileDirections);

            var spawnedAny = false;
            for (var i = 0; i < projectileDirections.Count; i++)
            {
                var projectileDirection = projectileDirections[i];
                var rotation = Quaternion.LookRotation(projectileDirection, Vector3.up);
                var projectileObject = useProjectilePooling
                    ? ProjectilePoolService.Shared.Spawn(weaponConfig.ProjectilePrefab, spawnTransform.position, rotation)
                    : Instantiate(weaponConfig.ProjectilePrefab, spawnTransform.position, rotation);

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
                    continue;
                }

                projectileObject.transform.localScale =
                    weaponConfig.ProjectilePrefab.transform.localScale * modifierState.ProjectileSizeMultiplier;

                projectileController.Initialize(
                    CreateProjectileHitContext(hitContext, projectileDirection),
                    projectileDirection,
                    EffectiveProjectileSpeed,
                    weaponConfig.ProjectileLifetime,
                    gameObject,
                    modifierState);

                CombatEvents.RaiseProjectileSpawned(new ProjectileSpawnedEvent(
                    gameObject,
                    projectileObject,
                    spawnTransform.position,
                    rotation,
                    projectileDirection,
                    weaponConfig.ProjectileLifetime));

                spawnedAny = true;
            }

            return spawnedAny;
        }
        private ProjectileModifierState CreateProjectileModifierState()
        {
            if (runtimeStats == null)
            {
                return ProjectileModifierState.Neutral;
            }

            return new ProjectileModifierState(
                runtimeStats.ExtraProjectileCount,
                runtimeStats.FrontProjectileCount,
                runtimeStats.DiagonalProjectileCount,
                runtimeStats.SideProjectileCount,
                runtimeStats.RearProjectileCount,
                runtimeStats.ProjectilePierceCount,
                runtimeStats.ProjectileRicochetCount,
                runtimeStats.ProjectileWallBounceCount,
                runtimeStats.ProjectileHomingStrength,
                runtimeStats.ProjectileSizeMultiplier,
                runtimeStats.ProjectileSpeedMultiplier);
        }

        private static HitContext CreateProjectileHitContext(HitContext sourceContext, Vector3 projectileDirection)
        {
            return new HitContext(sourceContext.Source, sourceContext.Target, sourceContext.DamageAmount, sourceContext.DamageType)
            {
                CriticalChance = sourceContext.CriticalChance,
                CriticalMultiplier = sourceContext.CriticalMultiplier,
                IsCritical = sourceContext.IsCritical,
                IsProjectileHit = true,
                IsAbilityHit = sourceContext.IsAbilityHit,
                AbilityId = sourceContext.AbilityId,
                Knockback = sourceContext.Knockback,
                HitPoint = sourceContext.HitPoint,
                HitDirection = projectileDirection
            };
        }

        private bool TryResolveDirectHit(TargetingResult target, HitContext hitContext)
        {
            if (target.Damageable == null || !target.Damageable.IsAlive)
            {
                return false;
            }

            hitContext.IsProjectileHit = false;
            CombatHitModifierUtility.ApplySourceModifiers(hitContext);
            target.Damageable.ReceiveHit(hitContext);
            RaiseHitEvents(hitContext);
            return true;
        }

        private float ResolveBaseCriticalChance()
        {
            return weaponConfig != null ? Mathf.Clamp01(weaponConfig.CriticalChance) : 0f;
        }

        private float ResolveBaseCriticalMultiplier()
        {
            return weaponConfig != null ? Mathf.Max(1f, weaponConfig.CriticalMultiplier) : 1f;
        }

        private void FaceDirection(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var policy = EffectiveFacingPolicy;
            if (policy == PlayerFacingPolicy.MouseAimDirection)
            {
                return;
            }

            if (policy == PlayerFacingPolicy.HybridMouseAimThenTarget &&
                mouseAimController != null &&
                mouseAimController.HasAimPoint)
            {
                return;
            }

            QueueFacingRotation(direction.normalized);
        }
        private void QueueFacingRotation(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            pendingFacingRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            hasPendingFacingRotation = true;

            if (cachedRigidbody == null)
            {
                transform.rotation = AimRotationSpeed <= 0f
                    ? pendingFacingRotation
                    : Quaternion.RotateTowards(
                        transform.rotation,
                        pendingFacingRotation,
                        AimRotationSpeed * Time.deltaTime);

                hasPendingFacingRotation = false;
            }
        }

        private void ApplyPendingFacingRotation()
        {
            if (!hasPendingFacingRotation || cachedRigidbody == null)
            {
                return;
            }

            var nextRotation = AimRotationSpeed <= 0f
                ? pendingFacingRotation
                : Quaternion.RotateTowards(
                    cachedRigidbody.rotation,
                    pendingFacingRotation,
                    AimRotationSpeed * Time.fixedDeltaTime);

            cachedRigidbody.MoveRotation(nextRotation);

            if (Quaternion.Angle(nextRotation, pendingFacingRotation) <= 0.1f)
            {
                hasPendingFacingRotation = false;
            }
        }

        private Vector3 GetFallbackAttackDirection()
        {
            if (movementController != null && movementController.LastFacingDirection.sqrMagnitude > 0f)
            {
                return movementController.LastFacingDirection.normalized;
            }

            return GetForwardFallbackAttackDirection();
        }

        private Vector3 GetForwardFallbackAttackDirection()
        {
            var forward = transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0f ? forward.normalized : Vector3.forward;
        }

        private void StartCooldown()
        {
            cooldownRemaining = EffectiveAttackCooldown;
        }

        private void CompleteAttack(Vector3 attackDirection)
        {
            StartCooldown();
            TriggerShotFeedback(attackDirection);
            OnPrimaryAttackFired?.Invoke(attackDirection);
        }

        private void UpdateReticleRuntimeState()
        {
            if (aimReticle == null)
            {
                return;
            }

            if (playerConfig != null)
            {
                aimReticle.Configure(
                    playerConfig.AimReticleEnabled,
                    playerConfig.AimReticleScale,
                    playerConfig.AimReticleYOffset,
                    playerConfig.AimReticleSmoothTime,
                    playerConfig.HideSystemCursorDuringGameplay,
                    playerConfig.ShowReticleOnlyDuringGameplay,
                    playerConfig.ShowReticleOnlyWhileAimingOrFiring,
                    playerConfig.ReticleInvalidAimBehavior);
            }

            aimReticle.SetAimController(mouseAimController);
            aimReticle.SetInputReader(desktopInputReader);
            aimReticle.SetOwnerAlive(playerHealth == null || playerHealth.IsAlive);
            aimReticle.SetGameplayBlocked(Time.timeScale <= 0f);
            aimReticle.SetPrimaryFireActive(IsPrimaryFireRequested());
        }

        private void TriggerShotFeedback(Vector3 attackDirection)
        {
            var spawnTransform = projectileSpawnPoint != null ? projectileSpawnPoint : transform;
            var shotRotation = attackDirection.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(attackDirection.normalized, Vector3.up)
                : spawnTransform.rotation;
            var reticlePosition = Vector3.zero;
            var hasReticlePosition = mouseAimController != null && mouseAimController.TryGetAimPoint(out reticlePosition);
            var handledByFeedbackProfile = CombatEvents.RaiseShotFired(new ShotFiredEvent(
                gameObject,
                spawnTransform.position,
                shotRotation,
                attackDirection,
                aimReticle,
                muzzleFlash,
                shotAudioSource,
                shotSfx,
                shotSfxVolume,
                reticlePosition,
                hasReticlePosition));

            if (!handledByFeedbackProfile)
            {
                TriggerFallbackShotFeedback();
            }
        }

        private void TriggerFallbackShotFeedback()
        {
            if (pulseReticleOnFire)
            {
                aimReticle?.Pulse();
            }

            if (muzzleFlash != null)
            {
                muzzleFlash.Play(true);
            }

            if (shotAudioSource != null && shotSfx != null)
            {
                shotAudioSource.PlayOneShot(shotSfx, shotSfxVolume);
            }

            if (shotCameraShakeReceiver != null && shotCameraShakeAmplitude > 0f && shotCameraShakeDuration > 0f)
            {
                shotCameraShakeReceiver.Shake(shotCameraShakeAmplitude, shotCameraShakeDuration);
            }
        }

        private bool IsPrimaryFireRequested()
        {
            return desktopInputReader != null &&
                (desktopInputReader.PrimaryFireHeld || desktopInputReader.PrimaryFirePressedThisFrame);
        }

        private static bool IsManualFirePolicy(PrimaryAttackFirePolicy policy)
        {
            return policy == PrimaryAttackFirePolicy.HoldMouseAim;
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
