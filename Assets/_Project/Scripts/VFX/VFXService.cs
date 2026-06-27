using UnityEngine;

namespace TapKnockout.VFX
{
    [DisallowMultipleComponent]
    public sealed class VFXService : MonoBehaviour
    {
        [Header("Catalog")]
        [SerializeField] private VFXCatalog catalog;

        [Header("Pooling")]
        [SerializeField] private Transform poolRoot;
        [SerializeField] private bool prewarmOnAwake = true;

        [Header("Debug")]
        [SerializeField] private bool debugLogging;

        private PooledVFXSpawner spawner;

        public VFXCatalog Catalog => catalog;
        public Transform PoolRoot => poolRoot;
        public int ActiveCount => spawner != null ? spawner.ActiveCount : 0;

        private void Reset()
        {
            poolRoot = transform;
        }

        private void Awake()
        {
            EnsureSpawner();

            if (prewarmOnAwake)
            {
                Prewarm();
            }
        }

        private void OnDestroy()
        {
            ClearPools();
        }

        private void Update()
        {
            spawner?.Tick(Time.deltaTime);
        }

        public void SetCatalog(VFXCatalog value, bool prewarm = false)
        {
            catalog = value;

            if (prewarm)
            {
                Prewarm();
            }
        }

        public void Spawn(VFXSpawnRequest request)
        {
            TrySpawn(request);
        }

        public void Spawn(VFXEventType eventType, Vector3 position)
        {
            TrySpawn(VFXSpawnRequest.Create(eventType, position));
        }

        public bool TrySpawn(VFXSpawnRequest request)
        {
            EnsureSpawner();
            return spawner.TrySpawn(catalog, request);
        }

        public void Prewarm()
        {
            EnsureSpawner();
            spawner.Prewarm(catalog);
        }

        public void ClearPools()
        {
            spawner?.ClearPools();
        }

        private void EnsureSpawner()
        {
            if (poolRoot == null)
            {
                poolRoot = transform;
            }

            if (spawner == null)
            {
                spawner = new PooledVFXSpawner(poolRoot, debugLogging);
                return;
            }

            spawner.DebugLogging = debugLogging;
        }
    }
}
