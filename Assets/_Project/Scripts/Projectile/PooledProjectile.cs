using UnityEngine;

namespace TapKnockout.Projectile
{
    [DisallowMultipleComponent]
    public sealed class PooledProjectile : MonoBehaviour
    {
        private ProjectilePoolService owner;
        private GameObject prefabKey;
        private Rigidbody cachedRigidbody;
        private Vector3 defaultLocalScale;
        private bool hasDefaultLocalScale;

        public bool IsConfigured => owner != null && prefabKey != null;
        public GameObject PrefabKey => prefabKey;

        public void Configure(ProjectilePoolService poolOwner, GameObject projectilePrefab)
        {
            owner = poolOwner;
            prefabKey = projectilePrefab;
            CacheDefaultsIfNeeded();
        }

        public void PrepareForSpawn(Vector3 position, Quaternion rotation, Transform runtimeParent)
        {
            CacheDefaultsIfNeeded();
            if (runtimeParent != null)
            {
                transform.SetParent(runtimeParent, true);
            }

            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = defaultLocalScale;
            ResetPhysics();
            SetCollidersEnabled(true);
            gameObject.SetActive(true);
        }

        public void ReleaseToPool()
        {
            if (owner == null)
            {
                gameObject.SetActive(false);
                return;
            }

            owner.Release(this);
        }

        internal void PrepareForPool(Transform poolRoot)
        {
            ResetPhysics();
            SetCollidersEnabled(true);
            transform.localScale = defaultLocalScale;
            if (poolRoot != null)
            {
                transform.SetParent(poolRoot, true);
            }

            gameObject.SetActive(false);
        }

        private void CacheDefaultsIfNeeded()
        {
            if (!hasDefaultLocalScale)
            {
                defaultLocalScale = transform.localScale;
                hasDefaultLocalScale = true;
            }

            if (cachedRigidbody == null)
            {
                cachedRigidbody = GetComponent<Rigidbody>();
            }
        }

        private void ResetPhysics()
        {
            if (cachedRigidbody == null)
            {
                cachedRigidbody = GetComponent<Rigidbody>();
            }

            if (cachedRigidbody == null)
            {
                return;
            }

            cachedRigidbody.linearVelocity = Vector3.zero;
            cachedRigidbody.angularVelocity = Vector3.zero;
        }

        private void SetCollidersEnabled(bool enabled)
        {
            var colliders = GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = enabled;
            }
        }
    }
}
