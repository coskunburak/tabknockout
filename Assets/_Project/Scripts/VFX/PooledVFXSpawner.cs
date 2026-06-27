using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TapKnockout.VFX
{
    public sealed class PooledVFXSpawner
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private readonly Dictionary<VFXEventType, Queue<GameObject>> inactivePools = new Dictionary<VFXEventType, Queue<GameObject>>();
        private readonly Dictionary<GameObject, Vector3> baseScales = new Dictionary<GameObject, Vector3>();
        private readonly Dictionary<GameObject, Renderer[]> renderersByInstance = new Dictionary<GameObject, Renderer[]>();
        private readonly Dictionary<GameObject, ParticleSystem[]> particleSystemsByInstance = new Dictionary<GameObject, ParticleSystem[]>();
        private readonly HashSet<VFXEventType> missingDefinitionWarnings = new HashSet<VFXEventType>();
        private readonly HashSet<VFXEventType> missingPrefabWarnings = new HashSet<VFXEventType>();
        private readonly List<ActiveVFXInstance> activeInstances = new List<ActiveVFXInstance>();
        private readonly Transform poolRoot;

        public PooledVFXSpawner(Transform poolRoot, bool debugLogging = false)
        {
            this.poolRoot = poolRoot;
            DebugLogging = debugLogging;
        }

        public bool DebugLogging { get; set; }
        public int ActiveCount => activeInstances.Count;

        public void Prewarm(VFXCatalog catalog)
        {
            if (catalog == null)
            {
                return;
            }

            var definitions = catalog.Definitions;
            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition == null || definition.InitialPoolSize <= 0 || definition.Prefab == null)
                {
                    continue;
                }

                var pool = GetPool(definition.EventType);
                while (pool.Count < definition.InitialPoolSize)
                {
                    var instance = CreateInstance(definition);
                    instance.SetActive(false);
                    pool.Enqueue(instance);
                }
            }
        }

        public bool TrySpawn(VFXCatalog catalog, VFXSpawnRequest request)
        {
            if (catalog == null)
            {
                WarnMissingDefinition(request.EventType, "No VFXCatalog is assigned.");
                return false;
            }

            if (!catalog.TryGetDefinition(request.EventType, out var definition))
            {
                WarnMissingDefinition(request.EventType, "No VFXDefinition exists.");
                return false;
            }

            if (definition.Prefab == null)
            {
                WarnMissingPrefab(request.EventType);
                return false;
            }

            var instance = GetOrCreateInstance(definition);
            PrepareInstance(instance, definition, request);
            RestartParticles(instance);

            var lifetime = ResolveLifetime(instance, definition, request);
            activeInstances.Add(new ActiveVFXInstance(instance, definition.EventType, lifetime));
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (activeInstances.Count == 0)
            {
                return;
            }

            var safeDeltaTime = Mathf.Max(0f, deltaTime);
            for (var i = activeInstances.Count - 1; i >= 0; i--)
            {
                var activeInstance = activeInstances[i];
                if (activeInstance.Instance == null)
                {
                    activeInstances.RemoveAt(i);
                    continue;
                }

                activeInstance.RemainingLifetime -= safeDeltaTime;
                if (activeInstance.RemainingLifetime <= 0f)
                {
                    Release(activeInstance.Instance, activeInstance.EventType);
                    activeInstances.RemoveAt(i);
                    continue;
                }

                activeInstances[i] = activeInstance;
            }
        }

        public void ClearPools()
        {
            for (var i = activeInstances.Count - 1; i >= 0; i--)
            {
                DestroyInstance(activeInstances[i].Instance);
            }

            activeInstances.Clear();

            foreach (var pool in inactivePools.Values)
            {
                while (pool.Count > 0)
                {
                    DestroyInstance(pool.Dequeue());
                }
            }

            inactivePools.Clear();
            baseScales.Clear();
            renderersByInstance.Clear();
            particleSystemsByInstance.Clear();
            missingDefinitionWarnings.Clear();
            missingPrefabWarnings.Clear();
        }

        private GameObject GetOrCreateInstance(VFXDefinition definition)
        {
            var pool = GetPool(definition.EventType);
            while (pool.Count > 0)
            {
                var pooledInstance = pool.Dequeue();
                if (pooledInstance != null)
                {
                    return pooledInstance;
                }
            }

            return CreateInstance(definition);
        }

        private GameObject CreateInstance(VFXDefinition definition)
        {
            var instance = Object.Instantiate(definition.Prefab, poolRoot);
            instance.name = $"{definition.Prefab.name}_{definition.EventType}_Pooled";
            baseScales[instance] = instance.transform.localScale;
            renderersByInstance[instance] = instance.GetComponentsInChildren<Renderer>(true);
            particleSystemsByInstance[instance] = instance.GetComponentsInChildren<ParticleSystem>(true);
            return instance;
        }

        private void PrepareInstance(GameObject instance, VFXDefinition definition, VFXSpawnRequest request)
        {
            var parent = definition.ParentToRequestParent && request.Parent != null ? request.Parent : poolRoot;
            instance.transform.SetParent(parent, false);

            var rotation = definition.UseRequestRotation
                ? request.EffectiveRotation * Quaternion.Euler(definition.RotationOffsetEuler)
                : Quaternion.Euler(definition.RotationOffsetEuler);
            var position = request.Position + rotation * definition.PositionOffset;

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.transform.localScale = ResolveScale(instance, definition, request);
            ApplyColorOverride(instance, definition, request);
            instance.SetActive(true);
        }

        private Vector3 ResolveScale(GameObject instance, VFXDefinition definition, VFXSpawnRequest request)
        {
            var baseScale = baseScales.TryGetValue(instance, out var cachedScale) ? cachedScale : Vector3.one;
            var resolvedScale = definition.UseRequestScale ? Vector3.Scale(baseScale, request.EffectiveScale) : baseScale;
            return resolvedScale * definition.ScaleMultiplier;
        }

        private void ApplyColorOverride(GameObject instance, VFXDefinition definition, VFXSpawnRequest request)
        {
            if (!definition.AllowColorOverride)
            {
                return;
            }

            var renderers = GetCachedRenderers(instance);
            var block = new MaterialPropertyBlock();
            for (var i = 0; i < renderers.Length; i++)
            {
                var targetRenderer = renderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                if (!request.HasColorOverride)
                {
                    targetRenderer.SetPropertyBlock(null);
                    continue;
                }

                targetRenderer.GetPropertyBlock(block);
                block.SetColor(BaseColorId, request.ColorOverride);
                block.SetColor(ColorId, request.ColorOverride);
                targetRenderer.SetPropertyBlock(block);
                block.Clear();
            }
        }

        private void RestartParticles(GameObject instance)
        {
            var particleSystems = GetCachedParticleSystems(instance);
            for (var i = 0; i < particleSystems.Length; i++)
            {
                var particleSystem = particleSystems[i];
                if (particleSystem == null)
                {
                    continue;
                }

                particleSystem.Clear(true);
                particleSystem.Play(true);
            }
        }

        private float ResolveLifetime(GameObject instance, VFXDefinition definition, VFXSpawnRequest request)
        {
            if (request.HasLifetimeOverride)
            {
                return Mathf.Max(0.05f, request.LifetimeOverride);
            }

            if (definition.DefaultLifetime > 0f)
            {
                return Mathf.Max(0.05f, definition.DefaultLifetime);
            }

            var particleLifetime = 0f;
            var particleSystems = GetCachedParticleSystems(instance);
            for (var i = 0; i < particleSystems.Length; i++)
            {
                var particleSystem = particleSystems[i];
                if (particleSystem == null)
                {
                    continue;
                }

                var main = particleSystem.main;
                particleLifetime = Mathf.Max(particleLifetime, main.duration + main.startLifetime.constantMax);
            }

            return Mathf.Max(0.05f, particleLifetime);
        }

        private Renderer[] GetCachedRenderers(GameObject instance)
        {
            if (instance == null)
            {
                return System.Array.Empty<Renderer>();
            }

            if (!renderersByInstance.TryGetValue(instance, out var renderers) || renderers == null)
            {
                renderers = instance.GetComponentsInChildren<Renderer>(true);
                renderersByInstance[instance] = renderers;
            }

            return renderers;
        }

        private ParticleSystem[] GetCachedParticleSystems(GameObject instance)
        {
            if (instance == null)
            {
                return System.Array.Empty<ParticleSystem>();
            }

            if (!particleSystemsByInstance.TryGetValue(instance, out var particleSystems) || particleSystems == null)
            {
                particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
                particleSystemsByInstance[instance] = particleSystems;
            }

            return particleSystems;
        }

        private void Release(GameObject instance, VFXEventType eventType)
        {
            if (instance == null)
            {
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(poolRoot, false);
            GetPool(eventType).Enqueue(instance);
        }

        private Queue<GameObject> GetPool(VFXEventType eventType)
        {
            if (!inactivePools.TryGetValue(eventType, out var pool))
            {
                pool = new Queue<GameObject>();
                inactivePools.Add(eventType, pool);
            }

            return pool;
        }

        private void WarnMissingDefinition(VFXEventType eventType, string reason)
        {
            if (!DebugLogging || missingDefinitionWarnings.Contains(eventType))
            {
                return;
            }

            missingDefinitionWarnings.Add(eventType);
            Debug.LogWarning($"{nameof(PooledVFXSpawner)} could not spawn {eventType}. {reason}");
        }

        private void WarnMissingPrefab(VFXEventType eventType)
        {
            if (!DebugLogging || missingPrefabWarnings.Contains(eventType))
            {
                return;
            }

            missingPrefabWarnings.Add(eventType);
            Debug.LogWarning($"{nameof(PooledVFXSpawner)} could not spawn {eventType} because its prefab is missing.");
        }

        private static void DestroyInstance(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(instance);
            }
            else
            {
                Object.DestroyImmediate(instance);
            }
        }

        private struct ActiveVFXInstance
        {
            public ActiveVFXInstance(GameObject instance, VFXEventType eventType, float remainingLifetime)
            {
                Instance = instance;
                EventType = eventType;
                RemainingLifetime = remainingLifetime;
            }

            public GameObject Instance { get; }
            public VFXEventType EventType { get; }
            public float RemainingLifetime { get; set; }
        }
    }
}
