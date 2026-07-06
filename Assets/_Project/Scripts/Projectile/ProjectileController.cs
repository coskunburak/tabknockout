using System.Collections.Generic;
using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Projectile
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class ProjectileController : MonoBehaviour
    {
        [Header("Pooling")]
        [SerializeField] private bool deactivateInsteadOfDestroy;

        [Header("Hit Detection")]
        [SerializeField] private LayerMask hitLayers = ~0;
        [SerializeField, Min(0f)] private float sweepRadiusOverride;
        [SerializeField, Min(0f)] private float minimumSweepRadius = 0.08f;
        [SerializeField, Min(0.1f)] private float ricochetSearchRadius = 7f;
        [SerializeField, Min(0.1f)] private float homingSearchRadius = 10f;

        private const float HardMinimumSweepRadius = 0.18f;
        private static readonly RaycastHit[] HitBuffer = new RaycastHit[64];
        private static readonly Collider[] OverlapBuffer = new Collider[64];
        private static readonly IComparer<RaycastHit> HitDistanceComparer =
            Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance));

        private Collider cachedCollider;
        private Rigidbody cachedRigidbody;
        private HitContext hitContext;
        private Vector3 moveDirection = Vector3.forward;
        private float speed;
        private float remainingLifetime;
        private GameObject owner;
        private bool initialized;
        private Vector3 previousPosition;
        private bool hasPreviousPosition;
        private ProjectileModifierState modifierState = ProjectileModifierState.Neutral;
        private readonly HashSet<GameObject> hitTargets = new HashSet<GameObject>();
        private int pierceRemaining;
        private int ricochetRemaining;
        private int wallBounceRemaining;
        private float homingStrength;
        private bool redirectedAfterHit;

        public bool IsInitialized => initialized;

        private void Reset()
        {
            cachedCollider = GetComponent<Collider>();
            if (cachedCollider != null)
            {
                cachedCollider.isTrigger = true;
            }

            cachedRigidbody = GetComponent<Rigidbody>();
            if (cachedRigidbody != null)
            {
                cachedRigidbody.useGravity = false;
                cachedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                cachedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
        }

        private void Awake()
        {
            cachedCollider = GetComponent<Collider>();
            cachedRigidbody = GetComponent<Rigidbody>();
            ConfigureRuntimePhysics();
        }

        private void OnDisable()
        {
            initialized = false;
            hitContext = null;
            owner = null;
            hasPreviousPosition = false;
            modifierState = ProjectileModifierState.Neutral;
            hitTargets.Clear();
            pierceRemaining = 0;
            ricochetRemaining = 0;
            wallBounceRemaining = 0;
            homingStrength = 0f;
            redirectedAfterHit = false;

            if (cachedRigidbody != null)
            {
                cachedRigidbody.linearVelocity = Vector3.zero;
            }
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            var deltaTime = Time.deltaTime;
            UpdateHoming(deltaTime);

            var currentPosition = transform.position;
            if (TryResolveOverlaps(currentPosition))
            {
                return;
            }

            if (hasPreviousPosition && TryResolveSweep(previousPosition, currentPosition))
            {
                return;
            }

            if (cachedRigidbody == null)
            {
                if (TryAdvanceTransformMotion(currentPosition, deltaTime))
                {
                    return;
                }
            }
            else
            {
                previousPosition = currentPosition;
            }

            hasPreviousPosition = true;

            remainingLifetime -= deltaTime;
            if (remainingLifetime <= 0f)
            {
                DisposeProjectile();
            }
        }

        private void FixedUpdate()
        {
            if (!initialized || cachedRigidbody == null || speed <= 0f)
            {
                return;
            }

            var currentPosition = transform.position;
            if (TryResolveOverlaps(currentPosition))
            {
                return;
            }

            var predictedPosition = currentPosition + moveDirection * (speed * Time.fixedDeltaTime);
            if (TryResolveSweep(currentPosition, predictedPosition))
            {
                return;
            }

            TryResolveOverlaps(predictedPosition);
        }

        public void Initialize(
            HitContext context,
            Vector3 direction,
            float projectileSpeed,
            float lifetime,
            GameObject projectileOwner)
        {
            Initialize(context, direction, projectileSpeed, lifetime, projectileOwner, ProjectileModifierState.Neutral);
        }

        public void Initialize(
            HitContext context,
            Vector3 direction,
            float projectileSpeed,
            float lifetime,
            GameObject projectileOwner,
            ProjectileModifierState modifiers)
        {
            hitContext = context;
            owner = projectileOwner;
            modifierState = modifiers;
            hitTargets.Clear();
            pierceRemaining = modifierState.PierceCount;
            ricochetRemaining = modifierState.RicochetCount;
            wallBounceRemaining = modifierState.WallBounceCount;
            homingStrength = modifierState.HomingStrength;
            redirectedAfterHit = false;
            moveDirection = FlattenAndNormalize(direction);
            speed = Mathf.Max(0f, projectileSpeed);
            remainingLifetime = Mathf.Max(0.01f, lifetime);
            initialized = true;
            previousPosition = transform.position;
            hasPreviousPosition = true;

            ConfigureRuntimePhysics();
            ApplyRigidbodyVelocity();
            TryResolveOverlaps(previousPosition);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!initialized)
            {
                return;
            }

            TryResolveHit(other, ResolveClosestPoint(other, transform.position));
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!initialized)
            {
                return;
            }

            if (!TryResolveHit(collision.collider, ResolveCollisionPoint(collision)))
            {
                TryResolveWallBounce(collision);
            }
        }

        private void ApplyRigidbodyVelocity()
        {
            if (cachedRigidbody != null)
            {
                cachedRigidbody.linearVelocity = moveDirection * speed;
            }
        }

        private void ConfigureRuntimePhysics()
        {
            if (cachedCollider == null)
            {
                cachedCollider = GetComponent<Collider>();
            }

            if (cachedCollider != null)
            {
                cachedCollider.isTrigger = true;
            }

            if (cachedRigidbody == null)
            {
                cachedRigidbody = GetComponent<Rigidbody>();
            }

            if (cachedRigidbody == null)
            {
                return;
            }

            cachedRigidbody.useGravity = false;
            cachedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            cachedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            cachedRigidbody.angularVelocity = Vector3.zero;
        }

        private bool TryResolveHit(Collider other)
        {
            return TryResolveHit(other, ResolveClosestPoint(other, transform.position));
        }

        private bool TryResolveHit(Collider other, Vector3 hitPoint)
        {
            if (other == null || IsProjectileCollider(other) || IsOwnerCollider(other))
            {
                return false;
            }

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return false;
            }

            var targetObject = damageable.GameObject != null ? damageable.GameObject : other.gameObject;
            if (targetObject == null || hitTargets.Contains(targetObject))
            {
                return false;
            }

            var resolvedHitContext = CreateResolvedHitContext(targetObject, other, hitPoint);
            CombatHitModifierUtility.ApplySourceModifiers(resolvedHitContext);

            damageable.ReceiveHit(resolvedHitContext);
            RaiseHitEvents(resolvedHitContext);
            hitTargets.Add(targetObject);

            if (pierceRemaining > 0)
            {
                pierceRemaining--;
                previousPosition = transform.position;
                hasPreviousPosition = true;
                return true;
            }

            if (TryResolveRicochet(targetObject))
            {
                return true;
            }

            DisposeProjectile();
            return true;
        }

        private HitContext CreateResolvedHitContext(GameObject targetObject, Collider other, Vector3 hitPoint)
        {
            var sourceContext = hitContext ?? new HitContext(owner, targetObject, 0f);
            return new HitContext(sourceContext.Source, targetObject, sourceContext.DamageAmount, sourceContext.DamageType)
            {
                CriticalChance = sourceContext.CriticalChance,
                CriticalMultiplier = sourceContext.CriticalMultiplier,
                IsProjectileHit = true,
                IsAbilityHit = sourceContext.IsAbilityHit,
                AbilityId = sourceContext.AbilityId,
                Knockback = sourceContext.Knockback,
                HitDirection = moveDirection,
                HitPoint = hitPoint != Vector3.zero ? hitPoint : ResolveClosestPoint(other, transform.position)
            };
        }

        private bool IsOwnerCollider(Collider other)
        {
            if (owner == null)
            {
                return false;
            }

            return other.gameObject == owner || other.transform.IsChildOf(owner.transform);
        }

        private bool IsProjectileCollider(Collider other)
        {
            return other == cachedCollider ||
                other.transform == transform ||
                other.transform.IsChildOf(transform);
        }

        private bool TryResolveSweep(Vector3 startPosition, Vector3 endPosition)
        {
            var delta = endPosition - startPosition;
            var distance = delta.magnitude;
            if (distance <= 0.0001f)
            {
                return false;
            }

            var direction = delta / distance;
            var radius = ResolveSweepRadius();
            var hitCount = radius > 0f
                ? Physics.SphereCastNonAlloc(
                    startPosition,
                    radius,
                    direction,
                    HitBuffer,
                    distance,
                    ResolveHitLayerMask(),
                    QueryTriggerInteraction.Collide)
                : Physics.RaycastNonAlloc(
                    startPosition,
                    direction,
                    HitBuffer,
                    distance,
                    ResolveHitLayerMask(),
                    QueryTriggerInteraction.Collide);

            if (hitCount > 1)
            {
                System.Array.Sort(HitBuffer, 0, hitCount, HitDistanceComparer);
            }

            for (var i = 0; i < hitCount; i++)
            {
                var hit = HitBuffer[i];
                if (TryResolveHit(hit.collider, hit.point != Vector3.zero ? hit.point : ResolveClosestPoint(hit.collider, startPosition)))
                {
                    if (!initialized || redirectedAfterHit)
                    {
                        redirectedAfterHit = false;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryResolveOverlaps(Vector3 position)
        {
            var radius = ResolveSweepRadius();
            if (radius <= 0f)
            {
                return false;
            }

            var hitCount = Physics.OverlapSphereNonAlloc(
                position,
                radius,
                OverlapBuffer,
                ResolveHitLayerMask(),
                QueryTriggerInteraction.Collide);

            for (var i = 0; i < hitCount; i++)
            {
                var candidate = OverlapBuffer[i];
                if (TryResolveHit(candidate, ResolveClosestPoint(candidate, position)))
                {
                    if (!initialized || redirectedAfterHit)
                    {
                        redirectedAfterHit = false;
                        return true;
                    }
                }
            }

            return false;
        }

        private void UpdateHoming(float deltaTime)
        {
            if (homingStrength <= 0f || speed <= 0f)
            {
                return;
            }

            if (!TryFindNearestTarget(transform.position, null, homingSearchRadius, out var targetPosition))
            {
                return;
            }

            var desiredDirection = targetPosition - transform.position;
            desiredDirection.y = 0f;
            if (desiredDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var turnRadians = Mathf.Deg2Rad * Mathf.Max(90f, homingStrength * 360f) * Mathf.Max(0f, deltaTime);
            moveDirection = Vector3.RotateTowards(moveDirection, desiredDirection.normalized, turnRadians, 0f).normalized;
            transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            ApplyRigidbodyVelocity();
        }

        private bool TryResolveRicochet(GameObject previousTarget)
        {
            if (ricochetRemaining <= 0)
            {
                return false;
            }

            if (!TryFindNearestTarget(transform.position, previousTarget, ricochetSearchRadius, out var targetPosition))
            {
                return false;
            }

            var nextDirection = targetPosition - transform.position;
            nextDirection.y = 0f;
            if (nextDirection.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            ricochetRemaining--;
            redirectedAfterHit = true;
            moveDirection = nextDirection.normalized;
            transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            previousPosition = transform.position;
            hasPreviousPosition = true;
            ApplyRigidbodyVelocity();
            return true;
        }

        private bool TryFindNearestTarget(Vector3 origin, GameObject excludedTarget, float radius, out Vector3 targetPosition)
        {
            targetPosition = Vector3.zero;
            var hitCount = Physics.OverlapSphereNonAlloc(
                origin,
                Mathf.Max(0.1f, radius),
                OverlapBuffer,
                ResolveHitLayerMask(),
                QueryTriggerInteraction.Collide);

            var bestSqrDistance = float.PositiveInfinity;
            var found = false;
            for (var i = 0; i < hitCount; i++)
            {
                var candidate = OverlapBuffer[i];
                if (candidate == null || IsProjectileCollider(candidate) || IsOwnerCollider(candidate))
                {
                    continue;
                }

                var damageable = candidate.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.IsAlive)
                {
                    continue;
                }

                var targetObject = damageable.GameObject != null ? damageable.GameObject : candidate.gameObject;
                if (targetObject == null || targetObject == excludedTarget || hitTargets.Contains(targetObject))
                {
                    continue;
                }

                var candidatePosition = targetObject.transform.position;
                var offset = candidatePosition - origin;
                offset.y = 0f;
                var sqrDistance = offset.sqrMagnitude;
                if (sqrDistance >= bestSqrDistance)
                {
                    continue;
                }

                bestSqrDistance = sqrDistance;
                targetPosition = candidatePosition;
                found = true;
            }

            return found;
        }

        private bool TryResolveWallBounce(Collision collision)
        {
            if (wallBounceRemaining <= 0 || collision == null || collision.contactCount == 0)
            {
                return false;
            }

            var normal = collision.GetContact(0).normal;
            normal.y = 0f;
            if (normal.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            wallBounceRemaining--;
            redirectedAfterHit = true;
            moveDirection = Vector3.Reflect(moveDirection, normal.normalized);
            moveDirection.y = 0f;
            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                moveDirection.Normalize();
            }
            else
            {
                var fallbackDirection = -collision.relativeVelocity;
                fallbackDirection.y = 0f;
                moveDirection = fallbackDirection.sqrMagnitude > 0.0001f ? fallbackDirection.normalized : Vector3.back;
            }

            transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            previousPosition = transform.position;
            hasPreviousPosition = true;
            ApplyRigidbodyVelocity();
            return true;
        }

        private Vector3 PredictTransformPosition(Vector3 currentPosition, float deltaTime)
        {
            if (speed <= 0f || deltaTime <= 0f)
            {
                return currentPosition;
            }

            return currentPosition + moveDirection * (speed * deltaTime);
        }

        private bool TryAdvanceTransformMotion(Vector3 currentPosition, float deltaTime)
        {
            var targetPosition = PredictTransformPosition(currentPosition, deltaTime);
            if (TryResolveSweep(currentPosition, targetPosition))
            {
                return true;
            }

            if (TryResolveOverlaps(targetPosition))
            {
                return true;
            }

            transform.position = targetPosition;
            previousPosition = targetPosition;
            return false;
        }

        private float ResolveSweepRadius()
        {
            if (sweepRadiusOverride > 0f)
            {
                return sweepRadiusOverride;
            }

            var radius = Mathf.Max(HardMinimumSweepRadius, minimumSweepRadius);
            if (cachedCollider == null)
            {
                return radius;
            }

            var extents = cachedCollider.bounds.extents;
            var colliderRadius = Mathf.Min(Mathf.Abs(extents.x), Mathf.Abs(extents.z));
            return Mathf.Max(radius, colliderRadius);
        }

        private static Vector3 ResolveClosestPoint(Collider targetCollider, Vector3 queryPosition)
        {
            return targetCollider != null ? targetCollider.ClosestPoint(queryPosition) : queryPosition;
        }

        private static Vector3 ResolveCollisionPoint(Collision collision)
        {
            return collision != null && collision.contactCount > 0
                ? collision.GetContact(0).point
                : Vector3.zero;
        }

        private int ResolveHitLayerMask()
        {
            return hitLayers.value != 0 ? hitLayers.value : Physics.AllLayers;
        }

        private void DisposeProjectile()
        {
            initialized = false;

            if (TryGetComponent<PooledProjectile>(out var pooledProjectile) && pooledProjectile.IsConfigured)
            {
                pooledProjectile.ReleaseToPool();
                return;
            }

            if (deactivateInsteadOfDestroy)
            {
                gameObject.SetActive(false);
                return;
            }

            Destroy(gameObject);
        }

        private static void RaiseHitEvents(HitContext resolvedHitContext)
        {
            CombatEvents.RaiseHitResolved(resolvedHitContext);

            if (resolvedHitContext.WasIgnored)
            {
                return;
            }

            var damageEvent = new DamageEvent(
                resolvedHitContext.Source,
                resolvedHitContext.Target,
                resolvedHitContext.DamageAmount,
                resolvedHitContext.DamageType,
                resolvedHitContext);

            CombatEvents.RaiseDamageDealt(damageEvent);
            CombatEvents.RaiseDamageReceived(damageEvent);
        }

        private static Vector3 FlattenAndNormalize(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
        }
    }
}
