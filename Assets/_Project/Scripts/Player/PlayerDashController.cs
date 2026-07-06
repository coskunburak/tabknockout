using System.Collections.Generic;
using TapKnockout.Ability;
using TapKnockout.Combat;
using TapKnockout.Input;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TapKnockout.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerMovementController))]
    public sealed class PlayerDashController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerConfig config;
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerRuntimeStats runtimeStats;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Transform hitQueryOrigin;
        [SerializeField] private MouseAimController mouseAimController;

        [Header("Editor / Development Input")]
        [SerializeField] private bool enableKeyboardTestDash;
#if ENABLE_LEGACY_INPUT_MANAGER
        [SerializeField] private KeyCode legacyKeyboardDashKey = KeyCode.Space;
#endif

        [Header("Fallback Dash Values")]
        [SerializeField, Min(0.1f)] private float fallbackDashDistance = 3.5f;
        [SerializeField, Min(0.01f)] private float fallbackDashDuration = 0.18f;
        [SerializeField, Min(0f)] private float fallbackDashCooldown = 4f;
        [SerializeField, Min(0f)] private float fallbackDashImpactDamage = 12f;
        [SerializeField, Min(0f)] private float fallbackDashKnockbackForce = 8f;
        [SerializeField, Min(0f)] private float fallbackDashKnockbackDuration = 0.2f;
        [SerializeField, Min(0.05f)] private float fallbackDashHitRadius = 0.9f;
        [SerializeField] private LayerMask fallbackDashHitLayers;
        [SerializeField] private bool fallbackDashHasIFrames = true;
        [SerializeField, Min(0f)] private float fallbackDashIFrameDuration = 0.12f;

        [Header("Impact Scaling")]
        [SerializeField, Min(0.01f)] private float fallbackDashImpactReferenceSpeed = 19.44f;
        [SerializeField, Range(0f, 1f)] private float fallbackDashImpactSpeedDamageScale = 0.15f;
        [SerializeField, Min(0f)] private float fallbackDashImpactMinSpeedMultiplier = 0.85f;
        [SerializeField, Min(0f)] private float fallbackDashImpactMaxSpeedMultiplier = 1.35f;
        [SerializeField, Range(0.01f, 1f)] private float fallbackLowHealthDashDamageThreshold = 0.35f;

        [Header("Perfect Dash Reward")]
        [SerializeField, Min(0f)] private float fallbackPerfectDashCooldownRefundSeconds = 0.35f;

        [Header("Hit Query")]
        [SerializeField, Range(4, 64)] private int hitBufferSize = 24;
        [SerializeField, Min(0f)] private float dashCollisionSuppressionGrace = 0.25f;
        [SerializeField, Range(0f, 1f)] private float embeddedDashKnockbackForceMultiplier = 0.25f;
        [SerializeField, Min(0f)] private float embeddedDashKnockbackMaxDuration = 0.08f;

        [Header("Debug")]
        [SerializeField] private bool logSetupWarnings = true;

        private const float CooldownEventEpsilon = 0.001f;
        private static readonly IComparer<RaycastHit> DashHitDistanceComparer =
            Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance));

        private readonly DashState dashState = new DashState();
        private readonly DashHitRegistry hitRegistry = new DashHitRegistry();
        private readonly List<SuppressedDashCollision> suppressedDashCollisions = new List<SuppressedDashCollision>();
        private Rigidbody cachedRigidbody;
        private Collider[] hitBuffer;
        private RaycastHit[] dashSweepHitBuffer;
        private Collider[] selfColliders;
        private Vector3 dashDirection = Vector3.forward;
        private float dashSpeed;
        private float lastPublishedCooldownRemaining = -1f;
        private float lastPublishedNormalizedCooldown = -1f;
        private bool wasCooldownActive;
        private bool loggedMissingConfig;
        private bool loggedMissingHitLayers;
        private bool dashShieldGrantedThisDash;

        public bool IsDashing => dashState.IsDashing;
        public bool IsDashInvulnerable => dashState.IsIFrameActive;
        public float CooldownRemaining => dashState.CooldownRemaining;
        public float NormalizedCooldown => dashState.NormalizedCooldown;
        public float EffectiveDashCooldown => DashCooldown;
        public float EffectiveDashImpactDamage => ResolveDashImpactDamage(false);
        public float EffectiveDashKnockbackForce => DashKnockbackForce;
        public float PerfectDashCooldownRefundSeconds => fallbackPerfectDashCooldownRefundSeconds;

        private float DashDistance => config != null ? config.DashDistance : fallbackDashDistance;
        private float DashDuration => config != null ? config.DashDuration : fallbackDashDuration;
        private float DashCooldown => Mathf.Max(0.01f, (config != null ? config.DashCooldown : fallbackDashCooldown) * (runtimeStats != null ? runtimeStats.DashCooldownMultiplier : 1f));
        private float DashImpactDamage => ResolveDashImpactDamage(true);
        private float DashKnockbackForce => Mathf.Max(0f, (config != null ? config.DashKnockbackForce : fallbackDashKnockbackForce) * (runtimeStats != null ? runtimeStats.DashKnockbackMultiplier : 1f));
        private float DashKnockbackDuration => config != null ? config.DashKnockbackDuration : fallbackDashKnockbackDuration;
        private float DashHitRadius => config != null ? config.DashHitRadius : fallbackDashHitRadius;
        private LayerMask DashHitLayers => config != null ? config.DashHitLayers : fallbackDashHitLayers;
        private bool DashHasIFrames => config != null ? config.DashHasIFrames : fallbackDashHasIFrames;
        private float DashIFrameDuration => Mathf.Max(0f, (config != null ? config.DashIFrameDuration : fallbackDashIFrameDuration) + (runtimeStats != null ? runtimeStats.DashIFrameBonus : 0f));

        private void Reset()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            movementController = GetComponent<PlayerMovementController>();
            runtimeStats = GetComponent<PlayerRuntimeStats>();
            playerHealth = GetComponent<PlayerHealth>();
            mouseAimController = GetComponent<MouseAimController>();
            hitQueryOrigin = transform;
        }

        private void Awake()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            movementController = movementController != null ? movementController : GetComponent<PlayerMovementController>();
            runtimeStats = runtimeStats != null ? runtimeStats : GetComponent<PlayerRuntimeStats>();
            playerHealth = playerHealth != null ? playerHealth : GetComponent<PlayerHealth>();
            mouseAimController = mouseAimController != null ? mouseAimController : GetComponent<MouseAimController>();

            if (hitQueryOrigin == null)
            {
                hitQueryOrigin = transform;
            }

            EnsureHitBuffer();
        }

        private void OnValidate()
        {
            fallbackDashDistance = Mathf.Max(0.1f, fallbackDashDistance);
            fallbackDashDuration = Mathf.Max(0.01f, fallbackDashDuration);
            fallbackDashCooldown = Mathf.Max(0f, fallbackDashCooldown);
            fallbackDashImpactDamage = Mathf.Max(0f, fallbackDashImpactDamage);
            fallbackDashKnockbackForce = Mathf.Max(0f, fallbackDashKnockbackForce);
            fallbackDashKnockbackDuration = Mathf.Max(0f, fallbackDashKnockbackDuration);
            fallbackDashHitRadius = Mathf.Max(0.05f, fallbackDashHitRadius);
            fallbackDashIFrameDuration = Mathf.Max(0f, fallbackDashIFrameDuration);
            fallbackDashImpactReferenceSpeed = Mathf.Max(0.01f, fallbackDashImpactReferenceSpeed);
            fallbackDashImpactMaxSpeedMultiplier = Mathf.Max(fallbackDashImpactMinSpeedMultiplier, fallbackDashImpactMaxSpeedMultiplier);
            fallbackPerfectDashCooldownRefundSeconds = Mathf.Max(0f, fallbackPerfectDashCooldownRefundSeconds);
            dashCollisionSuppressionGrace = Mathf.Max(0f, dashCollisionSuppressionGrace);
            embeddedDashKnockbackForceMultiplier = Mathf.Clamp01(embeddedDashKnockbackForceMultiplier);
            embeddedDashKnockbackMaxDuration = Mathf.Max(0f, embeddedDashKnockbackMaxDuration);
            hitBufferSize = Mathf.Clamp(hitBufferSize, 4, 64);
        }

        private void Update()
        {
            if (enableKeyboardTestDash && WasDashPressedThisFrame())
            {
                TryDash();
            }
        }

        private void OnEnable()
        {
            CombatEvents.OnEntityKilled -= HandleEntityKilled;
            CombatEvents.OnEntityKilled += HandleEntityKilled;
        }

        private void FixedUpdate()
        {
            TickSuppressedDashCollisions(Time.fixedDeltaTime);

            if (dashState.IsDashing)
            {
                MoveDash(Time.fixedDeltaTime);
            }

            dashState.Tick(Time.fixedDeltaTime, out var dashEnded, out var iFrameEnded);

            if (iFrameEnded)
            {
                DashEvents.RaiseDashIFrameEnded(new DashIFrameEventArgs(gameObject, dashState.IFrameDuration));
            }

            if (dashEnded)
            {
                EndDash(true);
            }

            PublishCooldownStateIfChanged();
        }

        private void OnDisable()
        {
            CombatEvents.OnEntityKilled -= HandleEntityKilled;

            if (movementController != null)
            {
                movementController.SetMovementLocked(false);
            }

            if (dashState.IsDashing)
            {
                dashState.ForceEnd(out _);
            }

            hitRegistry.Clear();
            RestoreSuppressedDashCollisions();
            wasCooldownActive = false;
            lastPublishedCooldownRemaining = -1f;
            lastPublishedNormalizedCooldown = -1f;
            dashShieldGrantedThisDash = false;
        }

        public bool TryDash()
        {
            ResolveReferences();

            if (!dashState.CanDash || cachedRigidbody == null || movementController == null)
            {
                return false;
            }

            if (config == null && logSetupWarnings && !loggedMissingConfig)
            {
                loggedMissingConfig = true;
                Debug.LogWarning($"{nameof(PlayerDashController)} on {name} has no PlayerConfig assigned. Fallback dash values are being used.", this);
            }

            dashDirection = DashDirectionResolver.Resolve(movementController, transform, mouseAimController);
            dashSpeed = DashDistance / Mathf.Max(0.01f, DashDuration);
            hitRegistry.Clear();
            dashShieldGrantedThisDash = false;

            if (!dashState.TryBegin(DashDuration, DashCooldown, DashHasIFrames, DashIFrameDuration))
            {
                return false;
            }

            movementController.SetMovementLocked(true);
            FaceDashDirection();

            DashEvents.RaiseDashStarted(new DashStartedEventArgs(gameObject, dashDirection, DashDistance, DashDuration, DashCooldown));
            PublishCooldownStarted();

            if (dashState.IsIFrameActive)
            {
                DashEvents.RaiseDashIFrameStarted(new DashIFrameEventArgs(gameObject, dashState.IFrameDuration));
            }

            DetectDashHits();
            return true;
        }

        private void ResolveReferences()
        {
            if (cachedRigidbody == null)
            {
                cachedRigidbody = GetComponent<Rigidbody>();
            }

            if (movementController == null)
            {
                movementController = GetComponent<PlayerMovementController>();
            }

            if (runtimeStats == null)
            {
                runtimeStats = GetComponent<PlayerRuntimeStats>();
            }

            if (playerHealth == null)
            {
                playerHealth = GetComponent<PlayerHealth>();
            }

            if (hitQueryOrigin == null)
            {
                hitQueryOrigin = transform;
            }
            if (mouseAimController == null)
            {
                mouseAimController = GetComponent<MouseAimController>();
            }
        }

        private void MoveDash(float deltaTime)
        {
            var stepTime = Mathf.Min(deltaTime, dashState.DashRemaining);
            if (stepTime <= 0f)
            {
                return;
            }

            var currentPosition = cachedRigidbody.position;
            var targetPosition = currentPosition + dashDirection * (dashSpeed * stepTime);
            targetPosition.y = currentPosition.y;
            DetectDashHitsAt(currentPosition);
            DetectDashHitsAlongSegment(currentPosition, targetPosition);
            cachedRigidbody.MovePosition(targetPosition);
            DetectDashHitsAt(targetPosition);
        }

        private void DetectDashHits()
        {
            var origin = hitQueryOrigin != null ? hitQueryOrigin.position : transform.position;
            DetectDashHitsAt(origin);
        }

        private void DetectDashHitsAt(Vector3 origin)
        {
            var hitLayers = DashHitLayers;
            if (hitLayers.value == 0)
            {
                if (logSetupWarnings && !loggedMissingHitLayers)
                {
                    loggedMissingHitLayers = true;
                    Debug.LogWarning($"{nameof(PlayerDashController)} on {name} has no DashHitLayers set.", this);
                }

                return;
            }

            EnsureHitBuffer();

            var hitCount = Physics.OverlapSphereNonAlloc(
                origin,
                DashHitRadius,
                hitBuffer,
                hitLayers,
                QueryTriggerInteraction.Collide);

            for (var i = 0; i < hitCount; i++)
            {
                TryResolveDashHit(hitBuffer[i], origin);
            }
        }

        private void DetectDashHitsAlongSegment(Vector3 startPosition, Vector3 endPosition)
        {
            var hitLayers = DashHitLayers;
            if (hitLayers.value == 0)
            {
                if (logSetupWarnings && !loggedMissingHitLayers)
                {
                    loggedMissingHitLayers = true;
                    Debug.LogWarning($"{nameof(PlayerDashController)} on {name} has no DashHitLayers set.", this);
                }

                return;
            }

            var delta = endPosition - startPosition;
            var distance = delta.magnitude;
            if (distance <= 0.0001f)
            {
                return;
            }

            EnsureDashSweepHitBuffer();
            var direction = delta / distance;
            var hitCount = Physics.SphereCastNonAlloc(
                startPosition,
                DashHitRadius,
                direction,
                dashSweepHitBuffer,
                distance,
                hitLayers,
                QueryTriggerInteraction.Collide);

            if (hitCount > 1)
            {
                System.Array.Sort(dashSweepHitBuffer, 0, hitCount, DashHitDistanceComparer);
            }

            for (var i = 0; i < hitCount; i++)
            {
                var hit = dashSweepHitBuffer[i];
                var queryOrigin = hit.point != Vector3.zero
                    ? hit.point - direction * Mathf.Min(hit.distance, DashHitRadius)
                    : startPosition;
                TryResolveDashHit(hit.collider, queryOrigin);
            }
        }

        private void TryResolveDashHit(Collider candidateCollider, Vector3 queryOrigin)
        {
            if (candidateCollider == null ||
                !candidateCollider.gameObject.activeInHierarchy ||
                IsSelf(candidateCollider.transform))
            {
                return;
            }

            var damageable = candidateCollider.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return;
            }

            var targetable = candidateCollider.GetComponentInParent<ITargetable>();
            var targetTransform = ResolveTargetTransform(candidateCollider, damageable, targetable);
            var targetGameObject = ResolveTargetGameObject(candidateCollider, damageable, targetable, targetTransform);

            if (targetGameObject == null || !targetGameObject.activeInHierarchy)
            {
                return;
            }

            SuppressPhysicalDashCollision(candidateCollider);
            SettleTargetRigidbody(candidateCollider, targetGameObject);

            if (!hitRegistry.TryRegister(targetGameObject))
            {
                return;
            }

            var knockbackDirection = ResolveKnockbackDirection(queryOrigin, targetTransform);
            var knockback = ResolveDashKnockbackData(candidateCollider, knockbackDirection);
            var hitContext = new HitContext(gameObject, targetGameObject, DashImpactDamage, DamageType.Impact)
            {
                IsDashHit = true,
                HitDirection = dashDirection,
                HitPoint = targetTransform != null ? targetTransform.position : candidateCollider.ClosestPoint(queryOrigin),
                Knockback = knockback
            };

            CombatHitModifierUtility.ApplySourceModifiers(hitContext);
            damageable.ReceiveHit(hitContext);
            ApplyDashStatusEffectHooks(candidateCollider, targetGameObject, damageable);
            TryGrantDashShieldAfterHit();
            TryResolveDashShockwave(hitContext, targetGameObject);
            RaiseDashHitEvents(hitContext);
        }

        private void EndDash(bool completed)
        {
            movementController.SetMovementLocked(false);
            hitRegistry.Clear();
            dashShieldGrantedThisDash = false;
            DashEvents.RaiseDashEnded(new DashEndedEventArgs(gameObject, dashDirection, completed, dashState.CooldownRemaining));
        }

        public void RefundCooldown(float seconds)
        {
            dashState.ReduceCooldown(seconds);
            PublishCooldownStateIfChanged();
        }

        private void PublishCooldownStarted()
        {
            var eventArgs = CreateCooldownEventArgs();
            wasCooldownActive = eventArgs.CooldownRemaining > 0f;
            lastPublishedCooldownRemaining = eventArgs.CooldownRemaining;
            lastPublishedNormalizedCooldown = eventArgs.NormalizedCooldown;

            if (wasCooldownActive)
            {
                DashEvents.RaiseDashCooldownStarted(eventArgs);
            }

            DashEvents.RaiseDashCooldownChanged(eventArgs);
        }

        private void PublishCooldownStateIfChanged()
        {
            var eventArgs = CreateCooldownEventArgs();
            var cooldownActive = eventArgs.CooldownRemaining > 0f;
            var wasActive = wasCooldownActive;
            var changed =
                Mathf.Abs(eventArgs.CooldownRemaining - lastPublishedCooldownRemaining) > CooldownEventEpsilon ||
                Mathf.Abs(eventArgs.NormalizedCooldown - lastPublishedNormalizedCooldown) > CooldownEventEpsilon ||
                cooldownActive != wasCooldownActive;

            if (changed)
            {
                DashEvents.RaiseDashCooldownChanged(eventArgs);
            }

            if (wasActive && !cooldownActive)
            {
                DashEvents.RaiseDashCooldownReady(eventArgs);
            }

            wasCooldownActive = cooldownActive;
            lastPublishedCooldownRemaining = eventArgs.CooldownRemaining;
            lastPublishedNormalizedCooldown = eventArgs.NormalizedCooldown;
        }

        private DashCooldownEventArgs CreateCooldownEventArgs()
        {
            return new DashCooldownEventArgs(gameObject, dashState.CooldownRemaining, dashState.DashCooldown);
        }

        private void HandleEntityKilled(EntityKilledEvent entityKilledEvent)
        {
            ResolveReferences();

            if (runtimeStats == null ||
                runtimeStats.DashCooldownRefundOnKill <= 0f ||
                entityKilledEvent.Killer != gameObject ||
                entityKilledEvent.KillingHit == null ||
                !entityKilledEvent.KillingHit.IsDashHit)
            {
                return;
            }

            RefundCooldown(DashCooldown * runtimeStats.DashCooldownRefundOnKill);
        }

        private void EnsureHitBuffer()
        {
            if (hitBuffer == null || hitBuffer.Length != hitBufferSize)
            {
                hitBuffer = new Collider[hitBufferSize];
            }
        }

        private void EnsureDashSweepHitBuffer()
        {
            if (dashSweepHitBuffer == null || dashSweepHitBuffer.Length != hitBufferSize)
            {
                dashSweepHitBuffer = new RaycastHit[hitBufferSize];
            }
        }

        private void EnsureSelfColliders()
        {
            if (selfColliders == null || selfColliders.Length == 0)
            {
                selfColliders = GetComponentsInChildren<Collider>(true);
            }
        }

        private void FaceDashDirection()
        {
            if (dashDirection.sqrMagnitude <= 0f)
            {
                return;
            }

            cachedRigidbody.MoveRotation(Quaternion.LookRotation(dashDirection, Vector3.up));
        }

        private bool IsSelf(Transform candidate)
        {
            return candidate == transform || candidate.IsChildOf(transform);
        }

        private Vector3 ResolveKnockbackDirection(Vector3 queryOrigin, Transform targetTransform)
        {
            if (targetTransform != null)
            {
                var direction = targetTransform.position - queryOrigin;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    return direction.normalized;
                }
            }

            return dashDirection;
        }

        private void SuppressPhysicalDashCollision(Collider targetCollider)
        {
            if (targetCollider == null || targetCollider.isTrigger)
            {
                return;
            }

            EnsureSelfColliders();
            for (var i = 0; i < selfColliders.Length; i++)
            {
                var selfCollider = selfColliders[i];
                if (selfCollider == null ||
                    !selfCollider.enabled ||
                    selfCollider.isTrigger ||
                    selfCollider == targetCollider ||
                    targetCollider.transform.IsChildOf(transform))
                {
                    continue;
                }

                SuppressPhysicalDashCollision(selfCollider, targetCollider);
            }
        }

        private void SuppressPhysicalDashCollision(Collider selfCollider, Collider targetCollider)
        {
            for (var i = 0; i < suppressedDashCollisions.Count; i++)
            {
                var suppressed = suppressedDashCollisions[i];
                if (suppressed.SelfCollider == selfCollider && suppressed.TargetCollider == targetCollider)
                {
                    suppressed.GraceRemaining = Mathf.Max(suppressed.GraceRemaining, dashCollisionSuppressionGrace);
                    suppressedDashCollisions[i] = suppressed;
                    return;
                }
            }

            Physics.IgnoreCollision(selfCollider, targetCollider, true);
            suppressedDashCollisions.Add(new SuppressedDashCollision(
                selfCollider,
                targetCollider,
                dashCollisionSuppressionGrace));
        }

        private void TickSuppressedDashCollisions(float deltaTime)
        {
            for (var i = suppressedDashCollisions.Count - 1; i >= 0; i--)
            {
                var suppressed = suppressedDashCollisions[i];
                if (!suppressed.IsValid)
                {
                    suppressedDashCollisions.RemoveAt(i);
                    continue;
                }

                suppressed.GraceRemaining = Mathf.Max(0f, suppressed.GraceRemaining - Mathf.Max(0f, deltaTime));
                if (suppressed.GraceRemaining <= 0f &&
                    !AreCollidersPenetrating(suppressed.SelfCollider, suppressed.TargetCollider))
                {
                    Physics.IgnoreCollision(suppressed.SelfCollider, suppressed.TargetCollider, false);
                    suppressedDashCollisions.RemoveAt(i);
                    continue;
                }

                suppressedDashCollisions[i] = suppressed;
            }
        }

        private void RestoreSuppressedDashCollisions()
        {
            for (var i = suppressedDashCollisions.Count - 1; i >= 0; i--)
            {
                var suppressed = suppressedDashCollisions[i];
                if (suppressed.IsValid)
                {
                    Physics.IgnoreCollision(suppressed.SelfCollider, suppressed.TargetCollider, false);
                }
            }

            suppressedDashCollisions.Clear();
        }

        private static bool AreCollidersPenetrating(Collider first, Collider second)
        {
            return first != null &&
                second != null &&
                first.enabled &&
                second.enabled &&
                Physics.ComputePenetration(
                    first,
                    first.transform.position,
                    first.transform.rotation,
                    second,
                    second.transform.position,
                    second.transform.rotation,
                    out _,
                    out var distance) &&
                distance > 0.0001f;
        }

        private static void SettleTargetRigidbody(Collider candidateCollider, GameObject targetGameObject)
        {
            var body = targetGameObject != null
                ? targetGameObject.GetComponentInParent<Rigidbody>()
                : candidateCollider != null
                    ? candidateCollider.GetComponentInParent<Rigidbody>()
                    : null;

            if (body == null)
            {
                return;
            }

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        private KnockbackData ResolveDashKnockbackData(Collider targetCollider, Vector3 knockbackDirection)
        {
            var force = DashKnockbackForce;
            var duration = DashKnockbackDuration;
            if (force <= 0f || duration <= 0f)
            {
                return KnockbackData.None;
            }

            if (IsPenetratingAnySelfCollider(targetCollider))
            {
                force *= embeddedDashKnockbackForceMultiplier;
                duration = Mathf.Min(duration, embeddedDashKnockbackMaxDuration);
            }

            return force > 0f && duration > 0f
                ? new KnockbackData(knockbackDirection, force, duration)
                : KnockbackData.None;
        }

        private bool IsPenetratingAnySelfCollider(Collider targetCollider)
        {
            if (targetCollider == null)
            {
                return false;
            }

            EnsureSelfColliders();
            for (var i = 0; i < selfColliders.Length; i++)
            {
                var selfCollider = selfColliders[i];
                if (selfCollider == null ||
                    !selfCollider.enabled ||
                    selfCollider.isTrigger ||
                    selfCollider == targetCollider)
                {
                    continue;
                }

                if (AreCollidersPenetrating(selfCollider, targetCollider))
                {
                    return true;
                }
            }

            return false;
        }

        private float ResolveDashImpactDamage(bool includeCurrentDashSpeed)
        {
            var baseDamage = config != null ? config.DashImpactDamage : fallbackDashImpactDamage;
            var damageMultiplier = runtimeStats != null ? runtimeStats.DashDamageMultiplier : 1f;
            var referenceSpeed = ResolveDashImpactReferenceSpeed();
            var currentSpeed = includeCurrentDashSpeed && dashSpeed > 0f ? dashSpeed : referenceSpeed;
            var conditionalMultiplier = ResolveConditionalDashDamageMultiplier();

            return DashImpactDamageCalculator.CalculateDamage(
                baseDamage,
                damageMultiplier,
                currentSpeed,
                referenceSpeed,
                fallbackDashImpactSpeedDamageScale,
                fallbackDashImpactMinSpeedMultiplier,
                fallbackDashImpactMaxSpeedMultiplier,
                conditionalMultiplier);
        }

        private float ResolveDashImpactReferenceSpeed()
        {
            if (fallbackDashImpactReferenceSpeed > 0f)
            {
                return fallbackDashImpactReferenceSpeed;
            }

            return DashDistance / Mathf.Max(0.01f, DashDuration);
        }

        private float ResolveConditionalDashDamageMultiplier()
        {
            if (runtimeStats == null || playerHealth == null || playerHealth.MaxHealth <= 0f)
            {
                return 1f;
            }

            var normalizedHealth = playerHealth.CurrentHealth / playerHealth.MaxHealth;
            return normalizedHealth <= fallbackLowHealthDashDamageThreshold
                ? runtimeStats.DashLowHealthDamageMultiplier
                : 1f;
        }

        private void ApplyDashStatusEffectHooks(Collider candidateCollider, GameObject targetGameObject, IDamageable damageable)
        {
            if (runtimeStats == null || runtimeStats.DashStunDuration <= 0f || damageable == null || !damageable.IsAlive)
            {
                return;
            }

            var receiver = targetGameObject != null
                ? targetGameObject.GetComponentInChildren<IStatusEffectReceiver>()
                : null;

            if (receiver == null && candidateCollider != null)
            {
                receiver = candidateCollider.GetComponentInParent<IStatusEffectReceiver>();
            }

            receiver?.TryApplyStatusEffect(new StatusEffectRequest(
                StatusEffectType.Stun,
                gameObject,
                runtimeStats.DashStunDuration));
        }

        private void TryGrantDashShieldAfterHit()
        {
            if (runtimeStats == null || !runtimeStats.DashShieldAfterHit || dashShieldGrantedThisDash)
            {
                return;
            }

            runtimeStats.AddShieldCharge(1);
            dashShieldGrantedThisDash = true;
        }

        private void TryResolveDashShockwave(HitContext sourceHit, GameObject primaryTarget)
        {
            if (runtimeStats == null || runtimeStats.DashShockwaveRadius <= 0f || DashHitLayers.value == 0)
            {
                return;
            }

            EnsureHitBuffer();
            var origin = sourceHit.HitPoint != Vector3.zero
                ? sourceHit.HitPoint
                : primaryTarget != null
                    ? primaryTarget.transform.position
                    : hitQueryOrigin.position;
            var radius = runtimeStats.DashShockwaveRadius;
            var hitCount = Physics.OverlapSphereNonAlloc(
                origin,
                radius,
                hitBuffer,
                DashHitLayers,
                QueryTriggerInteraction.Collide);

            for (var i = 0; i < hitCount; i++)
            {
                var candidateCollider = hitBuffer[i];
                if (candidateCollider == null ||
                    !candidateCollider.gameObject.activeInHierarchy ||
                    IsSelf(candidateCollider.transform))
                {
                    continue;
                }

                var damageable = candidateCollider.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.IsAlive)
                {
                    continue;
                }

                var targetable = candidateCollider.GetComponentInParent<ITargetable>();
                var targetTransform = ResolveTargetTransform(candidateCollider, damageable, targetable);
                var targetGameObject = ResolveTargetGameObject(candidateCollider, damageable, targetable, targetTransform);
                if (targetGameObject == null ||
                    targetGameObject == primaryTarget ||
                    IsSelf(targetGameObject.transform) ||
                    !hitRegistry.TryRegister(targetGameObject))
                {
                    continue;
                }

                var hitDirection = targetTransform != null
                    ? targetTransform.position - origin
                    : candidateCollider.transform.position - origin;
                hitDirection.y = 0f;
                hitDirection = hitDirection.sqrMagnitude > 0.0001f ? hitDirection.normalized : dashDirection;

                var shockwaveHit = new HitContext(gameObject, targetGameObject, DashImpactDamage * 0.45f, DamageType.Impact)
                {
                    IsDashHit = true,
                    IsAbilityHit = true,
                    AbilityId = AbilityEffectType.DashShockwave.ToString(),
                    HitDirection = hitDirection,
                    HitPoint = targetTransform != null ? targetTransform.position : candidateCollider.ClosestPoint(origin),
                    Knockback = DashKnockbackForce > 0f && DashKnockbackDuration > 0f
                        ? new KnockbackData(hitDirection, DashKnockbackForce * 0.55f, DashKnockbackDuration)
                        : KnockbackData.None
                };

                CombatHitModifierUtility.ApplySourceModifiers(shockwaveHit);
                damageable.ReceiveHit(shockwaveHit);
                RaiseDashHitEvents(shockwaveHit);
            }
        }

        private static Transform ResolveTargetTransform(Collider candidateCollider, IDamageable damageable, ITargetable targetable)
        {
            if (targetable != null && targetable.TargetTransform != null)
            {
                return targetable.TargetTransform;
            }

            if (damageable.GameObject != null)
            {
                return damageable.GameObject.transform;
            }

            return candidateCollider.transform;
        }

        private static GameObject ResolveTargetGameObject(
            Collider candidateCollider,
            IDamageable damageable,
            ITargetable targetable,
            Transform targetTransform)
        {
            if (damageable.GameObject != null)
            {
                return damageable.GameObject;
            }

            if (targetable != null && targetable.GameObject != null)
            {
                return targetable.GameObject;
            }

            return targetTransform != null ? targetTransform.gameObject : candidateCollider.gameObject;
        }

        private void RaiseDashHitEvents(HitContext hitContext)
        {
            CombatEvents.RaiseHitResolved(hitContext);
            CombatEvents.RaiseDashHit(hitContext);

            var damageEvent = new DamageEvent(
                hitContext.Source,
                hitContext.Target,
                hitContext.DamageAmount,
                hitContext.DamageType,
                hitContext);

            CombatEvents.RaiseDamageDealt(damageEvent);
            CombatEvents.RaiseDamageReceived(damageEvent);
            DashEvents.RaiseDashHit(new DashHitEventArgs(gameObject, hitContext, dashDirection, DashDistance, DashDuration));
        }

        private bool WasDashPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                return true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.GetKeyDown(legacyKeyboardDashKey))
            {
                return true;
            }
#endif

            return false;
        }

        private struct SuppressedDashCollision
        {
            public SuppressedDashCollision(Collider selfCollider, Collider targetCollider, float graceRemaining)
            {
                SelfCollider = selfCollider;
                TargetCollider = targetCollider;
                GraceRemaining = graceRemaining;
            }

            public Collider SelfCollider { get; }
            public Collider TargetCollider { get; }
            public float GraceRemaining { get; set; }
            public bool IsValid => SelfCollider != null && TargetCollider != null;
        }
    }
}
