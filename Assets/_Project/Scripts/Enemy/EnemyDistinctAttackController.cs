using System.Collections.Generic;
using TapKnockout.Combat;
using TapKnockout.Projectile;
using UnityEngine;

namespace TapKnockout.Enemy
{
    /// <summary>
    /// Production enemy attack controller implementing all 11 distinct attack mechanics.
    ///
    /// Designed to run ALONGSIDE the existing EnemyAttackController (which is set
    /// autoDealContactDamage=false by the builder for enemies using this controller).
    ///
    /// State machine: Idle → Windup → Commit/Active → Recovery → Cooldown → Idle
    ///
    /// Each state transition dispatches animation, VFX, and telegraph hooks.
    /// Pool-lifecycle safe: all runtime state is reset on despawn.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyDistinctAttackController : MonoBehaviour, IPoolLifecycle, IEnemyRuntimeConfigReceiver, IEnemyRuntimeTargetReceiver
    {
        // ─── Inspector ────────────────────────────────────────────────────────

        [Header("Attack Configs")]
        [Tooltip("One or more attack configs. Controller picks best valid one each attempt.")]
        [SerializeField] private EnemyAttackConfig[] attackConfigs = System.Array.Empty<EnemyAttackConfig>();

        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private bool recoverPlayerTargetWhenMissing = true;
        [SerializeField] private string fallbackPlayerTag = "Player";
        [SerializeField, Min(0.05f)] private float targetRecoveryInterval = 0.25f;
        [SerializeField] private bool warnIfTargetMissing;

        [Header("References (auto-found if null)")]
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private Transform groundOrigin;
        [SerializeField] private Transform vfxRoot;
        [SerializeField] private Animator animator;
        [SerializeField] private EnemyMovement enemyMovement;

        [Header("Debug")]
        [SerializeField] private bool debugMode;

        // ─── State ────────────────────────────────────────────────────────────

        public enum AttackState
        {
            Idle = 0,
            Windup = 1,
            Active = 2,
            Recovery = 3
        }

        private AttackState currentState = AttackState.Idle;
        private EnemyAttackConfig activeConfig;
        private float stateRemaining;
        private readonly float[] cooldownPerConfig = new float[8];
        private int configCount;

        // Snapshot data committed at end of windup
        private Vector3 committedDirection;
        private Vector3 committedTargetPosition;
        private bool hasCommittedTarget;

        // Movement control
        private bool wasMovementEnabled = true;

        // Components
        private EnemyHealth enemyHealth;
        private Rigidbody cachedRigidbody;

        // Hit tracking (charge/dive/beam, prevents multi-hit same target)
        private readonly HashSet<GameObject> hitThisAction = new HashSet<GameObject>(8);
        private static readonly Collider[] OverlapBuffer = new Collider[32];
        private static readonly RaycastHit[] RaycastBuffer = new RaycastHit[32];
        private bool impactVfxPlayedThisAction;

        // Active zone tracking per-enemy
        private readonly List<EnemyAreaZone> activeZones = new List<EnemyAreaZone>(4);

        // Dive/leap physics
        private bool isLeaping;
        private Vector3 leapDirection;
        private float leapSpeed;

        // Phase visual (Ghost)
        private Renderer[] visualRenderers = System.Array.Empty<Renderer>();
        private float nextTargetRecoveryTime;
        private bool warnedMissingTarget;
        private bool warnedInvalidConfigs;

        // ─── Public accessors ─────────────────────────────────────────────────

        public bool IsAttacking => currentState != AttackState.Idle;
        public AttackState CurrentAttackState => currentState;
        public EnemyAttackConfig ActiveConfig => activeConfig;
        public Transform Target => target;

        // ─── Unity ────────────────────────────────────────────────────────────

        private void Awake()
        {
            ResolveReferences();

            // Cache renderers for ghost phase
            visualRenderers = GetComponentsInChildren<Renderer>(true);

            RefreshConfigCount();
            TryRecoverTarget(true);
        }

        private void OnEnable()
        {
            ResolveReferences();
            RefreshConfigCount();
            TryRecoverTarget(true);
        }

        private void OnDisable()
        {
            ResetRuntimeState(restoreMovement: false);
        }

        private void OnValidate()
        {
            targetRecoveryInterval = Mathf.Max(0.05f, targetRecoveryInterval);
        }

        private void ResolveReferences()
        {
            if (enemyHealth == null)
            {
                enemyHealth = GetComponent<EnemyHealth>();
            }

            if (cachedRigidbody == null)
            {
                cachedRigidbody = GetComponent<Rigidbody>();
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (enemyMovement == null)
            {
                enemyMovement = GetComponent<EnemyMovement>();
            }

            if (attackOrigin == null)
            {
                attackOrigin = FindChildTransform("AttackOrigin");
            }

            if (projectileSpawnPoint == null)
            {
                projectileSpawnPoint = FindChildTransform("ProjectileSpawnPoint") ?? attackOrigin;
            }

            if (groundOrigin == null)
            {
                groundOrigin = FindChildTransform("GroundOrigin");
            }

            if (vfxRoot == null)
            {
                vfxRoot = FindChildTransform("VFXRoot");
            }
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            TickCooldowns(dt);
            PruneExpiredZones();
            TryRecoverTarget();

            if (currentState != AttackState.Idle)
            {
                TickCurrentState(dt);
                return;
            }

            if (IsAlive())
            {
                TryBeginAttack();
            }
        }

        private void FixedUpdate()
        {
            if (!isLeaping || cachedRigidbody == null)
            {
                return;
            }

            var pos = cachedRigidbody.position;
            var nextPos = pos + leapDirection * (leapSpeed * Time.fixedDeltaTime);
            nextPos.y = pos.y;
            cachedRigidbody.MovePosition(nextPos);
        }

        // ─── IPoolLifecycle ───────────────────────────────────────────────────

        public void OnBeforeSpawnFromPool()
        {
            ResetRuntimeState(restoreMovement: false);
        }

        public void OnSpawnedFromPool()
        {
            ResolveReferences();
            TryRecoverTarget(true);
        }

        public void OnBeforeDespawnToPool()
        {
            ResetRuntimeState(restoreMovement: false);
        }

        public void ResetForPool()
        {
            ResetRuntimeState(restoreMovement: false);
        }

        public void ResetRuntimeState()
        {
            ResetRuntimeState(restoreMovement: IsAlive() && enabled && gameObject.activeInHierarchy);
        }

        private void ResetRuntimeState(bool restoreMovement)
        {
            currentState = AttackState.Idle;
            activeConfig = null;
            stateRemaining = 0f;
            committedDirection = Vector3.forward;
            committedTargetPosition = Vector3.zero;
            hasCommittedTarget = false;
            isLeaping = false;
            hitThisAction.Clear();
            impactVfxPlayedThisAction = false;
            if (restoreMovement)
            {
                RestoreMovement();
            }

            for (var i = 0; i < configCount; i++)
            {
                cooldownPerConfig[i] = attackConfigs != null && i < attackConfigs.Length && attackConfigs[i] != null
                    ? attackConfigs[i].InitialCooldownOffset
                    : 0f;
            }

            // Stop visual override (ghost phase)
            SetRenderersAlpha(1f);

            // Clean up zone references (zones self-deactivate)
            activeZones.Clear();
        }

        // ─── Public API ───────────────────────────────────────────────────────

        public void Initialize(Transform attackTarget)
        {
            SetTarget(attackTarget);
            ResetRuntimeState();
        }

        public void Initialize(EnemyConfig enemyConfig, Transform attackTarget)
        {
            Initialize(attackTarget);
        }

        public void SetTarget(Transform attackTarget)
        {
            target = attackTarget;
            warnedMissingTarget = false;
        }

        // ─── State Machine ────────────────────────────────────────────────────

        private void TickCurrentState(float dt)
        {
            stateRemaining = Mathf.Max(0f, stateRemaining - dt);

            switch (currentState)
            {
                case AttackState.Windup:
                    TickWindup(dt);
                    if (stateRemaining <= 0f)
                    {
                        TransitionToActive();
                    }
                    break;

                case AttackState.Active:
                    TickActive(dt);
                    if (stateRemaining <= 0f)
                    {
                        TransitionToRecovery();
                    }
                    break;

                case AttackState.Recovery:
                    if (stateRemaining <= 0f)
                    {
                        EndAttack();
                    }
                    break;
            }
        }

        private bool TryBeginAttack()
        {
            var bestConfig = SelectBestConfig(out var configIndex);
            if (bestConfig == null)
            {
                return false;
            }

            activeConfig = bestConfig;
            hitThisAction.Clear();
            impactVfxPlayedThisAction = false;
            TransitionToWindup(configIndex);
            return true;
        }

        private void TransitionToWindup(int configIndex)
        {
            currentState = AttackState.Windup;
            stateRemaining = activeConfig.WindupTime;

            // Movement lock
            if (!activeConfig.CanMoveDuringWindup)
            {
                LockMovement();
            }

            // Animation
            if (activeConfig.UseAnimationTrigger && animator != null)
            {
                TrySafeSetTrigger(activeConfig.AnimationTrigger);
            }

            // Telegraph — show at target or self
            ShowTelegraph(activeConfig, configIndex);

            Log($"[{name}] Windup → {activeConfig.DisplayName} ({activeConfig.AttackType})");
        }

        private void TickWindup(float dt)
        {
            // Ghost: fade out during windup
            if (activeConfig != null && activeConfig.AttackType == EnemyDistinctAttackType.HomingProjectile)
            {
                var progress = activeConfig.WindupTime > 0f ? 1f - (stateRemaining / activeConfig.WindupTime) : 1f;
                SetRenderersAlpha(Mathf.Lerp(1f, 0.25f, progress));
            }
        }

        private void TransitionToActive()
        {
            currentState = AttackState.Active;
            stateRemaining = activeConfig.ActiveTime;

            // Snap commit direction
            if (target != null)
            {
                var toTarget = target.position - transform.position;
                toTarget.y = 0f;
                committedDirection = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : transform.forward;
                committedTargetPosition = target.position;
                hasCommittedTarget = true;
            }

            // Movement lock
            if (activeConfig.CommitLocksMovement)
            {
                LockMovement();
            }

            if (activeConfig.CommitLocksRotation && cachedRigidbody != null)
            {
                // Face committed direction
                if (committedDirection.sqrMagnitude > 0.0001f)
                {
                    cachedRigidbody.rotation = Quaternion.LookRotation(committedDirection, Vector3.up);
                }
            }

            EndTelegraph();

            // Ghost: restore alpha and fire
            if (activeConfig.AttackType == EnemyDistinctAttackType.HomingProjectile)
            {
                SetRenderersAlpha(1f);
            }

            SpawnVfx(
                activeConfig.ActiveVfxPrefab,
                GetAttackOrigin(),
                Quaternion.LookRotation(committedDirection, Vector3.up),
                ResolveActiveVfxParent(activeConfig));

            // Dispatch attack
            ExecuteAttack();

            Log($"[{name}] Active → {activeConfig.DisplayName}");
        }

        private void TickActive(float dt)
        {
            switch (activeConfig.AttackType)
            {
                case EnemyDistinctAttackType.Beam:
                    TickBeam(dt);
                    break;

                case EnemyDistinctAttackType.Dive:
                    TickDiveLeap(dt);
                    break;

                case EnemyDistinctAttackType.LeapSlash:
                    TickDiveLeap(dt);
                    break;
            }
        }

        private void TransitionToRecovery()
        {
            currentState = AttackState.Recovery;
            stateRemaining = activeConfig.RecoveryTime;

            isLeaping = false;

            if (activeConfig != null &&
                !impactVfxPlayedThisAction &&
                (activeConfig.AttackType == EnemyDistinctAttackType.Dive ||
                    activeConfig.AttackType == EnemyDistinctAttackType.Charge))
            {
                SpawnVfx(activeConfig.ImpactVfxPrefab, transform.position, Quaternion.LookRotation(committedDirection, Vector3.up));
                impactVfxPlayedThisAction = true;
            }

            if (!activeConfig.CanMoveDuringRecovery)
            {
                LockMovement();
            }
            else
            {
                RestoreMovement();
            }

            Log($"[{name}] Recovery → {activeConfig.DisplayName}");
        }

        private void EndAttack()
        {
            // Land hit for leap if we reached target
            if (activeConfig != null && (activeConfig.AttackType == EnemyDistinctAttackType.LeapSlash)
                && hasCommittedTarget)
            {
                if (!impactVfxPlayedThisAction)
                {
                    SpawnVfx(activeConfig.ImpactVfxPrefab, committedTargetPosition, Quaternion.LookRotation(committedDirection, Vector3.up));
                    impactVfxPlayedThisAction = true;
                }

                ApplyRadialHitbox(activeConfig, committedTargetPosition);
            }

            isLeaping = false;
            RestoreMovement();

            // Start cooldown for this config
            var configIndex = FindConfigIndex(activeConfig);
            if (configIndex >= 0)
            {
                cooldownPerConfig[configIndex] = activeConfig.Cooldown;
            }

            currentState = AttackState.Idle;
            activeConfig = null;
            hasCommittedTarget = false;
            hitThisAction.Clear();

            Log($"[{name}] Attack ended → back to Idle");
        }

        // ─── Attack Execution Dispatch ────────────────────────────────────────

        private void ExecuteAttack()
        {
            if (activeConfig == null)
            {
                return;
            }

            switch (activeConfig.AttackType)
            {
                case EnemyDistinctAttackType.MeleeArc:
                    ExecuteMeleeArc();
                    break;

                case EnemyDistinctAttackType.Charge:
                case EnemyDistinctAttackType.Dive:
                    BeginDiveCharge();
                    break;

                case EnemyDistinctAttackType.Projectile:
                case EnemyDistinctAttackType.SpikeProjectile:
                    FireProjectile(false);
                    break;

                case EnemyDistinctAttackType.RadialBurst:
                    ExecuteRadialBurst();
                    break;

                case EnemyDistinctAttackType.SlimeProjectileArea:
                    FireProjectileWithZone();
                    break;

                case EnemyDistinctAttackType.Beam:
                    // Beam damage ticks in TickBeam during Active state
                    break;

                case EnemyDistinctAttackType.LeapSlash:
                    BeginLeap();
                    break;

                case EnemyDistinctAttackType.HomingProjectile:
                    FireHomingProjectile();
                    break;

                case EnemyDistinctAttackType.SporeZone:
                    SpawnSporeZone();
                    break;

                case EnemyDistinctAttackType.FrostSlamShockwave:
                    ExecuteFrostSlam();
                    break;
            }
        }

        // ─── Melee Arc ────────────────────────────────────────────────────────

        private void ExecuteMeleeArc()
        {
            if (activeConfig == null)
            {
                return;
            }

            var origin = GetAttackOrigin();
            var hitCount = Physics.OverlapSphereNonAlloc(
                origin,
                activeConfig.HitboxRadius,
                OverlapBuffer,
                activeConfig.HitLayerMask,
                QueryTriggerInteraction.Collide);

            for (var i = 0; i < hitCount; i++)
            {
                var col = OverlapBuffer[i];
                if (col == null || col.transform.IsChildOf(transform))
                {
                    continue;
                }

                // Arc check — only hits within frontal arc half-angle
                var toTarget = col.transform.position - origin;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.0001f)
                {
                    var angle = Vector3.Angle(committedDirection, toTarget.normalized);
                    if (angle > activeConfig.HitboxArcHalfAngle)
                    {
                        continue;
                    }
                }

                TryApplyHit(col, col.ClosestPoint(origin));
            }

            TryApplyTargetHit(activeConfig, origin, activeConfig.HitboxRadius, useArc: true);
        }

        // ─── Radial Burst ─────────────────────────────────────────────────────

        private void ExecuteRadialBurst()
        {
            if (activeConfig == null)
            {
                return;
            }

            ApplyRadialHitbox(activeConfig, transform.position);
        }

        private void ApplyRadialHitbox(EnemyAttackConfig cfg, Vector3 centre)
        {
            var hitCount = Physics.OverlapSphereNonAlloc(
                centre,
                cfg.HitboxRadius,
                OverlapBuffer,
                cfg.HitLayerMask,
                QueryTriggerInteraction.Collide);

            for (var i = 0; i < hitCount; i++)
            {
                var col = OverlapBuffer[i];
                if (col == null || col.transform.IsChildOf(transform))
                {
                    continue;
                }

                TryApplyHit(col, col.ClosestPoint(centre));
            }

            TryApplyTargetHit(cfg, centre, cfg.HitboxRadius, useArc: false);
        }

        // ─── Charge / Dive ────────────────────────────────────────────────────

        private void BeginDiveCharge()
        {
            if (activeConfig == null)
            {
                return;
            }

            var speed = activeConfig.AttackType == EnemyDistinctAttackType.Dive
                ? (enemyMovement != null ? 3.5f : 8f) * activeConfig.DiveSpeedMultiplier
                : activeConfig.ProjectileSpeed; // Charge reuses projectile speed for simplicity
            leapDirection = committedDirection;
            leapSpeed = speed;
            isLeaping = true;
        }

        private void TickDiveLeap(float dt)
        {
            if (!isLeaping)
            {
                return;
            }

            // Damage-on-contact: check OverlapSphere along path
            var origin = transform.position;
            var hitCount = Physics.OverlapSphereNonAlloc(
                origin,
                0.5f,
                OverlapBuffer,
                activeConfig.HitLayerMask,
                QueryTriggerInteraction.Collide);

            for (var i = 0; i < hitCount; i++)
            {
                var col = OverlapBuffer[i];
                if (col == null || col.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (TryApplyHit(col, col.ClosestPoint(origin)))
                {
                    isLeaping = false;
                    break;
                }
            }

            if (TryApplyTargetHit(activeConfig, origin, Mathf.Max(0.6f, activeConfig.HitboxRadius), useArc: false))
            {
                isLeaping = false;
            }
        }

        // ─── Leap Slash ───────────────────────────────────────────────────────

        private void BeginLeap()
        {
            if (activeConfig == null || !hasCommittedTarget)
            {
                return;
            }

            var toTarget = committedTargetPosition - transform.position;
            toTarget.y = 0f;
            leapDirection = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : committedDirection;
            leapSpeed = (enemyMovement != null ? 3f : 6f) * activeConfig.DiveSpeedMultiplier;
            isLeaping = true;
        }

        // ─── Beam ─────────────────────────────────────────────────────────────

        private void TickBeam(float dt)
        {
            if (activeConfig == null)
            {
                return;
            }

            var origin = GetAttackOrigin();
            var beamDir = committedDirection;
            var hitCount = Physics.SphereCastNonAlloc(
                origin,
                activeConfig.BeamWidth,
                beamDir,
                RaycastBuffer,
                activeConfig.BeamLength,
                activeConfig.HitLayerMask,
                QueryTriggerInteraction.Collide);

            for (var i = 0; i < hitCount; i++)
            {
                var hit = RaycastBuffer[i];
                if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                TryApplyHit(hit.collider, hit.point);
            }

            TryApplyTargetBeamHit();
        }

        // ─── Projectile ───────────────────────────────────────────────────────

        private void FireProjectile(bool homing)
        {
            if (activeConfig == null || activeConfig.ProjectilePrefab == null)
            {
                Log($"[{name}] FireProjectile: no prefab configured.");
                return;
            }

            var origin = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
            var dir = committedDirection;

            var spawned = ProjectilePoolService.Shared.Spawn(
                activeConfig.ProjectilePrefab,
                origin,
                Quaternion.LookRotation(dir, Vector3.up));

            if (spawned == null)
            {
                return;
            }

            var request = new EnemyProjectileRequest(
                gameObject,
                target != null ? target.gameObject : null,
                origin,
                dir,
                activeConfig.Damage,
                activeConfig.ProjectileSpeed,
                activeConfig.ProjectileLifetime);

            var ep = spawned.GetComponent<EnemyProjectileController>();
            if (ep != null)
            {
                ep.SetImpactVfx(activeConfig.ImpactVfxPrefab, activeConfig.VfxLifetime);
            }

            ep?.Initialize(request);

            // If homing, wire up homing component if present
            if (homing)
            {
                var hc = spawned.GetComponent<EnemyHomingProjectile>();
                if (hc != null && target != null)
                {
                    hc.Initialize(target, activeConfig.HomingStrength, activeConfig.HomingMaxTurnDegreesPerSecond);
                }
            }
        }

        private void FireProjectileWithZone()
        {
            // Fire a projectile that will spawn an area zone on expire/impact.
            if (activeConfig == null || activeConfig.ProjectilePrefab == null)
            {
                Log($"[{name}] FireProjectileWithZone: no prefab configured.");
                return;
            }

            var origin = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
            var dir = committedDirection;

            var spawned = ProjectilePoolService.Shared.Spawn(
                activeConfig.ProjectilePrefab,
                origin,
                Quaternion.LookRotation(dir, Vector3.up));

            if (spawned == null)
            {
                return;
            }

            var request = new EnemyProjectileRequest(
                gameObject,
                target != null ? target.gameObject : null,
                origin,
                dir,
                activeConfig.Damage,
                activeConfig.ProjectileSpeed,
                activeConfig.ProjectileLifetime);

            var ep = spawned.GetComponent<EnemyProjectileController>();
            if (ep != null)
            {
                ep.SetImpactVfx(activeConfig.ImpactVfxPrefab, activeConfig.VfxLifetime);
            }

            ep?.Initialize(request);

            // Wire zone spawner if present on the projectile
            var zoneSpawner = spawned.GetComponent<ProjectileAreaZoneSpawner>();
            if (zoneSpawner != null)
            {
                zoneSpawner.Initialize(gameObject, activeConfig);
            }
        }

        private void FireHomingProjectile()
        {
            FireProjectile(true);
        }

        // ─── Spore Zone ───────────────────────────────────────────────────────

        private void SpawnSporeZone()
        {
            if (activeConfig == null || activeConfig.AreaZonePrefab == null)
            {
                Log($"[{name}] SpawnSporeZone: no area zone prefab.");
                return;
            }

            // Cap active zones
            PruneExpiredZones();
            if (activeZones.Count >= activeConfig.MaxActiveZones)
            {
                return;
            }

            // Choose position: near player's current position
            var zonePos = hasCommittedTarget
                ? committedTargetPosition
                : transform.position + committedDirection * 3f;
            zonePos.y = transform.position.y;

            SpawnZoneAt(zonePos);
        }

        private void SpawnZoneAt(Vector3 worldPosition)
        {
            if (activeConfig?.AreaZonePrefab == null)
            {
                return;
            }

            var zoneGo = Instantiate(activeConfig.AreaZonePrefab, worldPosition, Quaternion.identity);
            var zone = zoneGo.GetComponent<EnemyAreaZone>();
            if (zone == null)
            {
                zone = zoneGo.AddComponent<EnemyAreaZone>();
            }

            zone.Initialize(
                gameObject,
                activeConfig.AreaZoneRadius,
                activeConfig.AreaZoneDuration,
                activeConfig.AreaZoneTickInterval,
                activeConfig.AreaZoneTickDamage,
                activeConfig.StatusEffectType,
                activeConfig.StatusEffectDuration,
                activeConfig.StatusEffectSlowMultiplier,
                activeConfig.HitLayerMask);

            activeZones.Add(zone);
        }

        // ─── Frost Slam ───────────────────────────────────────────────────────

        private void ExecuteFrostSlam()
        {
            // 1. Radial hitbox for immediate damage
            ExecuteRadialBurst();

            // 2. Spawn frost area zone centred on self
            if (activeConfig?.AreaZonePrefab != null)
            {
                SpawnZoneAt(transform.position);
            }
        }

        // ─── Hit Application ──────────────────────────────────────────────────

        private bool TryApplyHit(Collider col, Vector3 hitPoint)
        {
            var damageable = col.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return false;
            }

            var targetGo = damageable.GameObject != null ? damageable.GameObject : col.gameObject;
            if (!IsExpectedTarget(targetGo))
            {
                return false;
            }

            return TryApplyDamage(damageable, targetGo, hitPoint, col.transform.position);
        }

        private bool TryApplyTargetHit(EnemyAttackConfig cfg, Vector3 origin, float radius, bool useArc)
        {
            if (cfg == null || target == null)
            {
                return false;
            }

            var damageable = target.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return false;
            }

            var targetGo = damageable.GameObject != null ? damageable.GameObject : target.gameObject;
            if (!hitThisAction.Add(targetGo))
            {
                return false;
            }

            var toTarget = target.position - origin;
            toTarget.y = 0f;
            var horizontalDistance = toTarget.magnitude;
            if (horizontalDistance > Mathf.Max(0f, radius))
            {
                hitThisAction.Remove(targetGo);
                return false;
            }

            if (useArc && toTarget.sqrMagnitude > 0.0001f)
            {
                var angle = Vector3.Angle(committedDirection, toTarget.normalized);
                if (angle > cfg.HitboxArcHalfAngle)
                {
                    hitThisAction.Remove(targetGo);
                    return false;
                }
            }

            return TryApplyDamage(damageable, targetGo, target.position, target.position, alreadyRegistered: true);
        }

        private bool TryApplyTargetBeamHit()
        {
            if (activeConfig == null || target == null)
            {
                return false;
            }

            var damageable = target.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return false;
            }

            var origin = GetAttackOrigin();
            var toTarget = target.position - origin;
            toTarget.y = 0f;
            var projected = Vector3.Dot(toTarget, committedDirection);
            if (projected < 0f || projected > activeConfig.BeamLength)
            {
                return false;
            }

            var closestPoint = origin + committedDirection * projected;
            var perpendicular = target.position - closestPoint;
            perpendicular.y = 0f;
            if (perpendicular.magnitude > Mathf.Max(0.2f, activeConfig.BeamWidth + 0.35f))
            {
                return false;
            }

            var targetGo = damageable.GameObject != null ? damageable.GameObject : target.gameObject;
            return TryApplyDamage(damageable, targetGo, closestPoint, target.position);
        }

        private bool TryApplyDamage(
            IDamageable damageable,
            GameObject targetGo,
            Vector3 hitPoint,
            Vector3 targetPosition,
            bool alreadyRegistered = false)
        {
            if (damageable == null || targetGo == null || activeConfig == null)
            {
                return false;
            }

            if (!alreadyRegistered && !hitThisAction.Add(targetGo))
            {
                return false;
            }

            Log($"[{name}] Damage attempted → {activeConfig.DisplayName} on {targetGo.name}");

            var knockbackDir = (targetPosition - transform.position);
            knockbackDir.y = 0f;
            knockbackDir = knockbackDir.sqrMagnitude > 0.0001f ? knockbackDir.normalized : committedDirection;

            var hitContext = new HitContext(gameObject, targetGo, activeConfig.Damage, DamageType.Physical)
            {
                HitPoint = hitPoint,
                HitDirection = committedDirection,
                Knockback = activeConfig.KnockbackForce > 0f
                    ? new KnockbackData(knockbackDir, activeConfig.KnockbackForce, activeConfig.KnockbackDuration)
                    : KnockbackData.None
            };

            damageable.ReceiveHit(hitContext);
            RaiseDamageEvents(hitContext);
            SpawnVfx(activeConfig.ImpactVfxPrefab, hitPoint, Quaternion.LookRotation(knockbackDir, Vector3.up));
            impactVfxPlayedThisAction = true;
            Log($"[{name}] Damage {(hitContext.WasIgnored ? "ignored" : "applied")} → {activeConfig.DisplayName} on {targetGo.name}");

            // Status effect on direct hit
            if (activeConfig.HasStatusEffect)
            {
                var receiver = targetGo.GetComponentInChildren<IStatusEffectReceiver>();
                receiver?.TryApplyStatusEffect(new StatusEffectRequest(
                    activeConfig.StatusEffectType,
                    gameObject,
                    activeConfig.StatusEffectDuration,
                    tickDamage: 0f,
                    tickInterval: 1f,
                    slowMultiplier: activeConfig.StatusEffectSlowMultiplier));
            }

            return true;
        }

        private bool IsExpectedTarget(GameObject targetGo)
        {
            if (target == null || targetGo == null)
            {
                return true;
            }

            return targetGo == target.gameObject ||
                target.IsChildOf(targetGo.transform) ||
                targetGo.transform.IsChildOf(target);
        }

        // ─── Config Selection ─────────────────────────────────────────────────

        private EnemyAttackConfig SelectBestConfig(out int configIndex)
        {
            configIndex = -1;

            if (!IsAlive() || target == null)
            {
                LogMissingTargetIfNeeded();
                return null;
            }

            if (configCount <= 0 && !warnedInvalidConfigs)
            {
                warnedInvalidConfigs = true;
                Debug.LogWarning($"{nameof(EnemyDistinctAttackController)} on {name} has no attack configs.", this);
            }

            var distSq = HorizontalDistanceSq(transform.position, target.position);
            EnemyAttackConfig best = null;
            var bestIndex = -1;

            for (var i = 0; i < configCount; i++)
            {
                var cfg = attackConfigs[i];
                if (cfg == null || cooldownPerConfig[i] > 0f)
                {
                    continue;
                }

                var rangeSq = cfg.TriggerRange * cfg.TriggerRange;
                if (distSq > rangeSq)
                {
                    continue;
                }

                // Prefer the first config in range (configs are ordered: primary then fallback)
                if (best == null)
                {
                    best = cfg;
                    bestIndex = i;
                }
            }

            configIndex = bestIndex;
            return best;
        }

        private int FindConfigIndex(EnemyAttackConfig cfg)
        {
            for (var i = 0; i < configCount; i++)
            {
                if (ReferenceEquals(attackConfigs[i], cfg))
                {
                    return i;
                }
            }

            return -1;
        }

        // ─── Telegraph ───────────────────────────────────────────────────────

        private EnemyTelegraphController _telegraphController;

        private void ShowTelegraph(EnemyAttackConfig cfg, int configIndex)
        {
            if (_telegraphController == null)
            {
                _telegraphController = GetComponentInChildren<EnemyTelegraphController>(true);
            }

            if (_telegraphController == null || cfg == null)
            {
                return;
            }

            switch (cfg.AttackType)
            {
                case EnemyDistinctAttackType.MeleeArc:
                    _telegraphController.BeginTelegraph(null, cfg.TelegraphType, cfg.WindupTime, transform, target);
                    break;

                // Ground-area telegraphs (show at player/target position)
                case EnemyDistinctAttackType.SporeZone:
                {
                    var pos = target != null ? target.position : transform.position + transform.forward * 3f;
                    pos.y = transform.position.y;
                    _telegraphController.BeginTelegraphAtPosition(null, cfg.TelegraphType, cfg.WindupTime, pos, Quaternion.identity);
                    break;
                }

                case EnemyDistinctAttackType.LeapSlash:
                {
                    var pos = target != null ? target.position : transform.position;
                    pos.y = transform.position.y;
                    _telegraphController.BeginTelegraphAtPosition(null, cfg.TelegraphType, cfg.WindupTime, pos, Quaternion.identity);
                    break;
                }

                case EnemyDistinctAttackType.FrostSlamShockwave:
                case EnemyDistinctAttackType.RadialBurst:
                    _telegraphController.BeginTelegraphAtPosition(null, cfg.TelegraphType, cfg.WindupTime, transform.position, Quaternion.identity);
                    break;

                // Line/charge telegraphs (show aimed at player)
                case EnemyDistinctAttackType.Dive:
                case EnemyDistinctAttackType.Charge:
                case EnemyDistinctAttackType.Beam:
                    _telegraphController.BeginTelegraph(null, cfg.TelegraphType, cfg.WindupTime, transform, target);
                    break;

                case EnemyDistinctAttackType.Projectile:
                case EnemyDistinctAttackType.SpikeProjectile:
                case EnemyDistinctAttackType.SlimeProjectileArea:
                case EnemyDistinctAttackType.HomingProjectile:
                    _telegraphController.BeginTelegraph(null, cfg.TelegraphType, cfg.WindupTime, transform, target);
                    break;

                default:
                    break;
            }
        }

        private void EndTelegraph()
        {
            _telegraphController?.EndTelegraph();
        }

        // ─── Movement Lock ────────────────────────────────────────────────────

        private void LockMovement()
        {
            if (enemyMovement != null && enemyMovement.enabled)
            {
                wasMovementEnabled = true;
                enemyMovement.enabled = false;
                if (cachedRigidbody != null)
                {
                    cachedRigidbody.linearVelocity = Vector3.zero;
                }
            }
        }

        private void RestoreMovement()
        {
            if (enemyMovement != null && !enemyMovement.enabled && wasMovementEnabled)
            {
                enemyMovement.enabled = true;
            }
        }

        // ─── Ghost Phase ──────────────────────────────────────────────────────

        private void SetRenderersAlpha(float alpha)
        {
            foreach (var r in visualRenderers)
            {
                if (r == null)
                {
                    continue;
                }

                foreach (var mat in r.materials)
                {
                    if (mat == null)
                    {
                        continue;
                    }

                    var col = mat.color;
                    col.a = alpha;
                    mat.color = col;
                }
            }
        }

        // ─── Zone Cleanup ─────────────────────────────────────────────────────

        private void PruneExpiredZones()
        {
            for (var i = activeZones.Count - 1; i >= 0; i--)
            {
                var zone = activeZones[i];
                if (zone == null || !zone.IsActive)
                {
                    activeZones.RemoveAt(i);
                }
            }
        }

        // ─── Cooldown Tick ────────────────────────────────────────────────────

        private void TickCooldowns(float dt)
        {
            for (var i = 0; i < configCount; i++)
            {
                if (cooldownPerConfig[i] > 0f)
                {
                    cooldownPerConfig[i] = Mathf.Max(0f, cooldownPerConfig[i] - dt);
                }
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private bool IsAlive() => enemyHealth == null || enemyHealth.IsAlive;

        private Vector3 GetAttackOrigin() =>
            attackOrigin != null ? attackOrigin.position : transform.position;

        private void RefreshConfigCount()
        {
            configCount = Mathf.Min(attackConfigs != null ? attackConfigs.Length : 0, cooldownPerConfig.Length);
            for (var i = 0; i < configCount; i++)
            {
                cooldownPerConfig[i] = attackConfigs[i] != null ? attackConfigs[i].InitialCooldownOffset : 0f;
            }

            for (var i = configCount; i < cooldownPerConfig.Length; i++)
            {
                cooldownPerConfig[i] = 0f;
            }
        }

        private bool TryRecoverTarget(bool force = false)
        {
            if (!recoverPlayerTargetWhenMissing || HasUsableTarget(target))
            {
                return target != null;
            }

            if (!force && Time.time < nextTargetRecoveryTime)
            {
                return false;
            }

            nextTargetRecoveryTime = Time.time + targetRecoveryInterval;

            if (TryResolveTargetFromEnemyController(out var recoveredTarget) ||
                TryResolveTargetFromTag(out recoveredTarget) ||
                TryResolveTargetFromDamageable(out recoveredTarget))
            {
                SetTarget(recoveredTarget);
                Log($"[{name}] player acquired: {recoveredTarget.name}");
                return true;
            }

            LogMissingTargetIfNeeded();
            return false;
        }

        private bool TryResolveTargetFromEnemyController(out Transform recoveredTarget)
        {
            recoveredTarget = null;

            var controller = GetComponent<EnemyController>();
            if (HasUsableTarget(controller != null ? controller.Target : null))
            {
                recoveredTarget = controller.Target;
                return true;
            }

            if (HasUsableTarget(enemyMovement != null ? enemyMovement.Target : null))
            {
                recoveredTarget = enemyMovement.Target;
                return true;
            }

            var legacyAttack = GetComponent<EnemyAttackController>();
            if (HasUsableTarget(legacyAttack != null ? legacyAttack.Target : null))
            {
                recoveredTarget = legacyAttack.Target;
                return true;
            }

            return false;
        }

        private bool TryResolveTargetFromTag(out Transform recoveredTarget)
        {
            recoveredTarget = null;
            if (string.IsNullOrWhiteSpace(fallbackPlayerTag))
            {
                return false;
            }

            try
            {
                var playerObject = GameObject.FindGameObjectWithTag(fallbackPlayerTag);
                if (playerObject != null && playerObject.activeInHierarchy)
                {
                    recoveredTarget = playerObject.transform;
                    return true;
                }
            }
            catch (UnityException)
            {
                return false;
            }

            return false;
        }

        private static bool TryResolveTargetFromDamageable(out Transform recoveredTarget)
        {
            recoveredTarget = null;
            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IDamageable damageable &&
                    damageable.IsAlive &&
                    damageable.GameObject != null &&
                    damageable.GameObject.CompareTag("Player"))
                {
                    recoveredTarget = damageable.GameObject.transform;
                    return true;
                }
            }

            return false;
        }

        private void LogMissingTargetIfNeeded()
        {
            if (!warnIfTargetMissing || warnedMissingTarget)
            {
                return;
            }

            warnedMissingTarget = true;
            Debug.LogWarning($"{nameof(EnemyDistinctAttackController)} on {name} cannot find a player target.", this);
        }

        private static bool HasUsableTarget(Transform candidate)
        {
            return candidate != null && candidate.gameObject.activeInHierarchy;
        }

        private Transform ResolveActiveVfxParent(EnemyAttackConfig cfg)
        {
            if (cfg == null)
            {
                return null;
            }

            switch (cfg.AttackType)
            {
                case EnemyDistinctAttackType.Dive:
                case EnemyDistinctAttackType.Charge:
                case EnemyDistinctAttackType.LeapSlash:
                case EnemyDistinctAttackType.HomingProjectile:
                    return vfxRoot != null ? vfxRoot : transform;
                default:
                    return null;
            }
        }

        private void SpawnVfx(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null)
            {
                return;
            }

            var instance = parent != null
                ? Instantiate(prefab, position, rotation, parent)
                : Instantiate(prefab, position, rotation);
            instance.SetActive(true);

            var lifetime = activeConfig != null ? activeConfig.VfxLifetime : 1.25f;
            var cleanup = instance.GetComponent<EnemyAttackVFXAutoCleanup>();
            if (cleanup != null)
            {
                cleanup.Configure(lifetime);
            }
            else
            {
                Destroy(instance, lifetime);
            }
        }

        private Transform FindChildTransform(string childName)
        {
            var t = transform.Find(childName);
            if (t != null)
            {
                return t;
            }

            foreach (Transform child in transform)
            {
                var found = child.Find(childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static float HorizontalDistanceSq(Vector3 a, Vector3 b)
        {
            var d = b - a;
            d.y = 0f;
            return d.sqrMagnitude;
        }

        private void TrySafeSetTrigger(string triggerName)
        {
            if (animator == null || string.IsNullOrWhiteSpace(triggerName))
            {
                return;
            }

            try
            {
                animator.SetTrigger(triggerName);
            }
            catch
            {
                // Ignore missing parameter; do not throw
            }
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

        private void Log(string msg)
        {
            if (debugMode)
            {
                Debug.Log(msg, this);
            }
        }

        // ─── Gizmos ───────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            if (attackConfigs == null)
            {
                return;
            }

            foreach (var cfg in attackConfigs)
            {
                if (cfg == null)
                {
                    continue;
                }

                Gizmos.color = cfg.DebugColor;

                // Trigger range
                var col = cfg.DebugColor;
                col.a = 0.15f;
                Gizmos.color = col;
                Gizmos.DrawSphere(transform.position, cfg.TriggerRange);

                col.a = 0.8f;
                Gizmos.color = col;
                Gizmos.DrawWireSphere(transform.position, cfg.TriggerRange);

                // Hitbox preview
                if (currentState == AttackState.Active)
                {
                    Gizmos.color = Color.red;
                    switch (cfg.HitboxShape)
                    {
                        case EnemyHitboxShape.Circle:
                            Gizmos.DrawWireSphere(transform.position, cfg.HitboxRadius);
                            break;
                        case EnemyHitboxShape.Arc:
                            Gizmos.DrawWireSphere(GetAttackOrigin(), cfg.HitboxRadius);
                            break;
                        case EnemyHitboxShape.Line:
                            Gizmos.DrawLine(transform.position, transform.position + transform.forward * cfg.BeamLength);
                            break;
                    }
                }
            }
        }
    }
}
