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

            remainingLifetime -= Time.deltaTime;
            if (remainingLifetime <= 0f)
            {
                DisposeProjectile();
                return;
            }

            var currentPosition = transform.position;
            if (TryResolveOverlaps(currentPosition))
            {
                return;
            }

            if (hasPreviousPosition && TryResolveSweep(previousPosition, currentPosition))
            {
                return;
            }

            previousPosition = currentPosition;
            hasPreviousPosition = true;

            if (cachedRigidbody == null)
            {
                transform.position += request.Direction * (request.Speed * Time.deltaTime);
            }
        }

        public void Initialize(EnemyProjectileRequest projectileRequest)
        {
            request = projectileRequest;
            remainingLifetime = request.Lifetime;
            initialized = request.CanSpawn;
            previousPosition = transform.position;
            hasPreviousPosition = initialized;

            if (cachedRigidbody != null)
            {
                cachedRigidbody.linearVelocity = request.Direction * request.Speed;
            }

            if (initialized)
            {
                TryResolveOverlaps(previousPosition);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (initialized)
            {
                TryResolveHit(other);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (initialized && collision != null)
            {
                TryResolveHit(collision.collider);
            }
        }

        private bool TryResolveHit(Collider other)
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
            var hitContext = request.CreateHitContext(targetObject);
            hitContext.HitPoint = other.ClosestPoint(transform.position);

            damageable.ReceiveHit(hitContext);
            RaiseHitEvents(hitContext);
            DisposeProjectile();
            return true;
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
                if (TryResolveHit(HitBuffer[i].collider))
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
                if (TryResolveHit(OverlapBuffer[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private float ResolveSweepRadius()
        {
            if (sweepRadiusOverride > 0f)
            {
                return sweepRadiusOverride;
            }

            var radius = Mathf.Max(0f, minimumSweepRadius);
            if (cachedCollider == null)
            {
                return radius;
            }

            var extents = cachedCollider.bounds.extents;
            var colliderRadius = Mathf.Min(Mathf.Abs(extents.x), Mathf.Abs(extents.z));
            return Mathf.Max(radius, colliderRadius);
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
