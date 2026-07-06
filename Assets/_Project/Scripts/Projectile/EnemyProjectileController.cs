using System.Collections.Generic;
using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Projectile
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class EnemyProjectileController : MonoBehaviour
    {
        [SerializeField] private bool deactivateInsteadOfDestroy;
        [SerializeField] private LayerMask hitLayers = ~0;
        [SerializeField, Min(0f)] private float sweepRadiusOverride;
        [SerializeField, Min(0f)] private float minimumSweepRadius = 0.05f;
        [SerializeField] private GameObject impactVfxPrefab;
        [SerializeField, Min(0.05f)] private float impactVfxLifetime = 1.25f;

        private const float HardMinimumSweepRadius = 0.12f;
        private static readonly RaycastHit[] HitBuffer = new RaycastHit[64];
        private static readonly Collider[] OverlapBuffer = new Collider[64];
        private static readonly IComparer<RaycastHit> HitDistanceComparer =
            Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance));

        private Collider cachedCollider;
        private Rigidbody cachedRigidbody;
        private EnemyProjectileRequest request;
        private float remainingLifetime;
        private bool initialized;
        private Vector3 previousPosition;
        private bool hasPreviousPosition;

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

        private void OnValidate()
        {
            sweepRadiusOverride = Mathf.Max(0f, sweepRadiusOverride);
            minimumSweepRadius = Mathf.Max(0f, minimumSweepRadius);
            impactVfxLifetime = Mathf.Max(0.05f, impactVfxLifetime);
        }

        private void OnDisable()
        {
            initialized = false;
            request = default;
            hasPreviousPosition = false;
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

        public void Initialize(EnemyProjectileRequest projectileRequest)
        {
            request = projectileRequest;
            remainingLifetime = request.Lifetime;
            initialized = request.CanSpawn;
            previousPosition = transform.position;
            hasPreviousPosition = initialized;

            ConfigureRuntimePhysics();
            if (cachedRigidbody != null)
            {
                cachedRigidbody.linearVelocity = request.Direction * request.Speed;
            }

            if (initialized)
            {
                TryResolveOverlaps(previousPosition);
            }
        }

        public void SetImpactVfx(GameObject prefab, float lifetime)
        {
            impactVfxPrefab = prefab;
            impactVfxLifetime = Mathf.Max(0.05f, lifetime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (initialized)
            {
                TryResolveHit(other, ResolveClosestPoint(other, transform.position));
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (initialized && collision != null)
            {
                TryResolveHit(collision.collider, ResolveCollisionPoint(collision));
            }
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
            if (!IsRequestedTarget(targetObject))
            {
                return false;
            }

            var hitContext = request.CreateHitContext(targetObject);
            hitContext.HitPoint = hitPoint != Vector3.zero ? hitPoint : ResolveClosestPoint(other, transform.position);

            damageable.ReceiveHit(hitContext);
            RaiseHitEvents(hitContext);
            SpawnImpactVfx(hitContext.HitPoint, hitContext.HitDirection);
            DisposeProjectile();
            return true;
        }

        private bool IsRequestedTarget(GameObject targetObject)
        {
            if (request.Target == null || targetObject == null)
            {
                return true;
            }

            return targetObject == request.Target ||
                targetObject.transform.IsChildOf(request.Target.transform) ||
                request.Target.transform.IsChildOf(targetObject.transform);
        }

        private bool IsOwnerCollider(Collider other)
        {
            return request.Source != null &&
                (other.gameObject == request.Source || other.transform.IsChildOf(request.Source.transform));
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
                    return true;
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
                    return true;
                }
            }

            return false;
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

        private Vector3 PredictTransformPosition(Vector3 currentPosition, float deltaTime)
        {
            if (request.Speed <= 0f || deltaTime <= 0f)
            {
                return currentPosition;
            }

            return currentPosition + request.Direction * (request.Speed * deltaTime);
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
            request = default;

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

        private void SpawnImpactVfx(Vector3 position, Vector3 direction)
        {
            if (impactVfxPrefab == null)
            {
                return;
            }

            var rotationDirection = direction.sqrMagnitude > 0.0001f ? direction : transform.forward;
            var instance = Instantiate(impactVfxPrefab, position, Quaternion.LookRotation(rotationDirection, Vector3.up));
            instance.SetActive(true);
            Destroy(instance, impactVfxLifetime);
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
