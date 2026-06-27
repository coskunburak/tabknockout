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

        private Rigidbody cachedRigidbody;
        private HitContext hitContext;
        private Vector3 moveDirection = Vector3.forward;
        private float speed;
        private float remainingLifetime;
        private GameObject owner;
        private bool initialized;

        public bool IsInitialized => initialized;

        private void Reset()
        {
            var projectileCollider = GetComponent<Collider>();
            if (projectileCollider != null)
            {
                projectileCollider.isTrigger = true;
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
            cachedRigidbody = GetComponent<Rigidbody>();
        }

        private void OnDisable()
        {
            initialized = false;
            hitContext = null;
            owner = null;

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

            if (cachedRigidbody == null)
            {
                transform.position += moveDirection * (speed * Time.deltaTime);
            }
        }

        public void Initialize(
            HitContext context,
            Vector3 direction,
            float projectileSpeed,
            float lifetime,
            GameObject projectileOwner)
        {
            hitContext = context;
            owner = projectileOwner;
            moveDirection = FlattenAndNormalize(direction);
            speed = Mathf.Max(0f, projectileSpeed);
            remainingLifetime = Mathf.Max(0.01f, lifetime);
            initialized = true;

            ApplyRigidbodyVelocity();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!initialized)
            {
                return;
            }

            TryResolveHit(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!initialized)
            {
                return;
            }

            TryResolveHit(collision.collider);
        }

        private void ApplyRigidbodyVelocity()
        {
            if (cachedRigidbody != null)
            {
                cachedRigidbody.linearVelocity = moveDirection * speed;
            }
        }

        private bool TryResolveHit(Collider other)
        {
            if (other == null || IsOwnerCollider(other))
            {
                return false;
            }

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return false;
            }

            var targetObject = damageable.GameObject != null ? damageable.GameObject : other.gameObject;
            var resolvedHitContext = hitContext ?? new HitContext(owner, targetObject, 0f);
            resolvedHitContext.Target = targetObject;
            resolvedHitContext.IsProjectileHit = true;
            resolvedHitContext.HitDirection = moveDirection;
            resolvedHitContext.HitPoint = other.ClosestPoint(transform.position);

            damageable.ReceiveHit(resolvedHitContext);
            RaiseHitEvents(resolvedHitContext);
            DisposeProjectile();
            return true;
        }

        private bool IsOwnerCollider(Collider other)
        {
            if (owner == null)
            {
                return false;
            }

            return other.gameObject == owner || other.transform.IsChildOf(owner.transform);
        }

        private void DisposeProjectile()
        {
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
