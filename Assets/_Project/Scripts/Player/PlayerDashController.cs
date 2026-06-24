using TapKnockout.Combat;
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
        [SerializeField] private Transform hitQueryOrigin;

        [Header("Editor / Development Input")]
        [SerializeField] private bool enableKeyboardTestDash = true;
        [SerializeField] private KeyCode legacyKeyboardDashKey = KeyCode.Space;

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

        [Header("Hit Query")]
        [SerializeField, Range(4, 64)] private int hitBufferSize = 24;

        [Header("Debug")]
        [SerializeField] private bool logSetupWarnings = true;

        private readonly DashState dashState = new DashState();
        private readonly DashHitRegistry hitRegistry = new DashHitRegistry();
        private Rigidbody cachedRigidbody;
        private Collider[] hitBuffer;
        private Vector3 dashDirection = Vector3.forward;
        private float dashSpeed;
        private bool loggedMissingConfig;
        private bool loggedMissingHitLayers;

        public bool IsDashing => dashState.IsDashing;
        public bool IsDashInvulnerable => dashState.IsIFrameActive;
        public float CooldownRemaining => dashState.CooldownRemaining;
        public float NormalizedCooldown => dashState.NormalizedCooldown;
        public float EffectiveDashCooldown => DashCooldown;
        public float EffectiveDashImpactDamage => DashImpactDamage;

        private float DashDistance => config != null ? config.DashDistance : fallbackDashDistance;
        private float DashDuration => config != null ? config.DashDuration : fallbackDashDuration;
        private float DashCooldown => Mathf.Max(0.01f, (config != null ? config.DashCooldown : fallbackDashCooldown) * (runtimeStats != null ? runtimeStats.DashCooldownMultiplier : 1f));
        private float DashImpactDamage => Mathf.Max(0f, (config != null ? config.DashImpactDamage : fallbackDashImpactDamage) * (runtimeStats != null ? runtimeStats.DashDamageMultiplier : 1f));
        private float DashKnockbackForce => config != null ? config.DashKnockbackForce : fallbackDashKnockbackForce;
        private float DashKnockbackDuration => config != null ? config.DashKnockbackDuration : fallbackDashKnockbackDuration;
        private float DashHitRadius => config != null ? config.DashHitRadius : fallbackDashHitRadius;
        private LayerMask DashHitLayers => config != null ? config.DashHitLayers : fallbackDashHitLayers;
        private bool DashHasIFrames => config != null ? config.DashHasIFrames : fallbackDashHasIFrames;
        private float DashIFrameDuration => config != null ? config.DashIFrameDuration : fallbackDashIFrameDuration;

        private void Reset()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            movementController = GetComponent<PlayerMovementController>();
            runtimeStats = GetComponent<PlayerRuntimeStats>();
            hitQueryOrigin = transform;
        }

        private void Awake()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            movementController = movementController != null ? movementController : GetComponent<PlayerMovementController>();
            runtimeStats = runtimeStats != null ? runtimeStats : GetComponent<PlayerRuntimeStats>();

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
            hitBufferSize = Mathf.Clamp(hitBufferSize, 4, 64);
        }

        private void Update()
        {
            if (enableKeyboardTestDash && WasDashPressedThisFrame())
            {
                TryDash();
            }
        }

        private void FixedUpdate()
        {
            if (dashState.IsDashing)
            {
                MoveDash(Time.fixedDeltaTime);
                DetectDashHits();
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
        }

        private void OnDisable()
        {
            if (movementController != null)
            {
                movementController.SetMovementLocked(false);
            }

            if (dashState.IsDashing)
            {
                dashState.ForceEnd(out _);
            }

            hitRegistry.Clear();
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

            dashDirection = DashDirectionResolver.Resolve(movementController, transform);
            dashSpeed = DashDistance / Mathf.Max(0.01f, DashDuration);
            hitRegistry.Clear();

            if (!dashState.TryBegin(DashDuration, DashCooldown, DashHasIFrames, DashIFrameDuration))
            {
                return false;
            }

            movementController.SetMovementLocked(true);
            FaceDashDirection();

            DashEvents.RaiseDashStarted(new DashStartedEventArgs(gameObject, dashDirection, DashDistance, DashDuration, DashCooldown));

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

            if (hitQueryOrigin == null)
            {
                hitQueryOrigin = transform;
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
            cachedRigidbody.MovePosition(targetPosition);
        }

        private void DetectDashHits()
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

            var origin = hitQueryOrigin != null ? hitQueryOrigin.position : transform.position;
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

        private void TryResolveDashHit(Collider candidateCollider, Vector3 queryOrigin)
        {
            if (candidateCollider == null || IsSelf(candidateCollider.transform))
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

            if (!hitRegistry.TryRegister(targetGameObject))
            {
                return;
            }

            var knockbackDirection = ResolveKnockbackDirection(queryOrigin, targetTransform);
            var hitContext = new HitContext(gameObject, targetGameObject, DashImpactDamage, DamageType.Impact)
            {
                IsDashHit = true,
                HitDirection = dashDirection,
                HitPoint = targetTransform != null ? targetTransform.position : candidateCollider.ClosestPoint(queryOrigin),
                Knockback = new KnockbackData(knockbackDirection, DashKnockbackForce, DashKnockbackDuration)
            };

            damageable.ReceiveHit(hitContext);
            RaiseDashHitEvents(hitContext);
        }

        private void EndDash(bool completed)
        {
            movementController.SetMovementLocked(false);
            hitRegistry.Clear();
            DashEvents.RaiseDashEnded(new DashEndedEventArgs(gameObject, dashDirection, completed, dashState.CooldownRemaining));
        }

        private void EnsureHitBuffer()
        {
            if (hitBuffer == null || hitBuffer.Length != hitBufferSize)
            {
                hitBuffer = new Collider[hitBufferSize];
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
    }
}
