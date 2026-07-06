using System.Collections.Generic;
using TapKnockout.Camera;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TapKnockout.Survivor
{
    [DisallowMultipleComponent]
    public sealed class SurvivorFeedbackPlayer : MonoBehaviour
    {
        private static SurvivorFeedbackPlayer sharedInstance;

        [Header("References")]
        [SerializeField] private Transform vfxPoolRoot;
        [SerializeField] private CameraShakeReceiver cameraShakeReceiver;

        [Header("Defaults")]
        [SerializeField, Min(0.05f)] private float defaultVFXLifetime = 1.5f;
        [SerializeField, Range(0f, 1f)] private float masterVolumeScale = 1f;

        private readonly Dictionary<GameObject, Queue<GameObject>> inactiveVFXByPrefab = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly Dictionary<GameObject, GameObject> prefabByInstance = new Dictionary<GameObject, GameObject>();
        private readonly List<ActiveVFX> activeVFX = new List<ActiveVFX>(32);
        private readonly List<AudioSource> activeLoopSources = new List<AudioSource>(8);

        public int ActiveVFXCount => activeVFX.Count;
        public int InactiveVFXCount
        {
            get
            {
                var count = 0;
                foreach (var pool in inactiveVFXByPrefab.Values)
                {
                    count += pool.Count;
                }

                return count;
            }
        }

        public static SurvivorFeedbackPlayer Shared
        {
            get
            {
                if (sharedInstance != null)
                {
                    return sharedInstance;
                }

                sharedInstance = Object.FindFirstObjectByType<SurvivorFeedbackPlayer>();
                if (sharedInstance != null)
                {
                    return sharedInstance;
                }

                var feedbackObject = new GameObject("SurvivorFeedbackPlayer");
                sharedInstance = feedbackObject.AddComponent<SurvivorFeedbackPlayer>();
                return sharedInstance;
            }
        }

        private void Awake()
        {
            if (sharedInstance == null)
            {
                sharedInstance = this;
            }

            if (vfxPoolRoot == null)
            {
                vfxPoolRoot = transform;
            }

            ResolveCameraShakeReceiver();
        }

        private void Update()
        {
            TickVFX(Time.deltaTime);
            CleanupLoopSources();
        }

        private void OnDisable()
        {
            StopAllLoops();
        }

        public void Play(
            ActiveSkillFeedbackConfig feedback,
            ActiveSkillFeedbackPhase phase,
            Vector3 position,
            Quaternion rotation,
            Transform parent,
            float scale,
            float lifetime,
            GameObject source,
            TapKnockout.Ability.AbilityDefinition ability = null)
        {
            if (feedback == null)
            {
                return;
            }

            var prefab = feedback.ResolveVFXPrefab(ability, phase);
            var resolvedLifetime = lifetime > 0f ? lifetime : feedback.ResolveVFXLifetime(ability);
            SpawnVFX(prefab, position, rotation, parent, scale, resolvedLifetime);

            var clip = feedback.ResolveSFX(ability, phase);
            PlayOneShot(clip, position, feedback.ResolveVolumeScale(ability));

            if (phase == ActiveSkillFeedbackPhase.Cast)
            {
                PlayLoop(feedback.ResolveLoopSFX(ability), position, feedback.ResolveVolumeScale(ability), feedback.ResolveLoopSFXDuration(ability));
            }

            var shakeIntensity = feedback.ResolveCameraShakeIntensity(ability);
            var shakeDuration = feedback.ResolveCameraShakeDuration(ability);
            RequestCameraShake(shakeIntensity, shakeDuration);
        }

        public void SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, float scale = 1f, float lifetime = 0f)
        {
            if (prefab == null)
            {
                return;
            }

            var instance = GetOrCreateVFX(prefab);
            instance.transform.SetParent(parent != null ? parent : vfxPoolRoot, true);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.transform.localScale = prefab.transform.localScale * Mathf.Max(0.01f, scale);
            instance.SetActive(true);
            RestartParticles(instance);
            activeVFX.Add(new ActiveVFX(instance, Mathf.Max(0.05f, lifetime > 0f ? lifetime : defaultVFXLifetime)));
        }

        public void PlayOneShot(AudioClip clip, Vector3 position, float volumeScale = 1f)
        {
            if (clip == null || masterVolumeScale <= 0f)
            {
                return;
            }

            AudioSource.PlayClipAtPoint(clip, position, Mathf.Clamp01(volumeScale * masterVolumeScale));
        }

        public void PlayLoop(AudioClip clip, Vector3 position, float volumeScale, float duration)
        {
            if (clip == null || duration <= 0f || masterVolumeScale <= 0f)
            {
                return;
            }

            var loopObject = new GameObject($"{clip.name}_LoopSFX");
            loopObject.transform.position = position;
            loopObject.transform.SetParent(transform, true);
            var audioSource = loopObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.volume = Mathf.Clamp01(volumeScale * masterVolumeScale);
            audioSource.spatialBlend = 1f;
            audioSource.Play();
            activeLoopSources.Add(audioSource);
            Destroy(loopObject, duration);
        }

        public void RequestCameraShake(float intensity, float duration)
        {
            if (intensity <= 0f || duration <= 0f)
            {
                return;
            }

            ResolveCameraShakeReceiver();
            cameraShakeReceiver?.Shake(intensity, duration);
        }

        private GameObject GetOrCreateVFX(GameObject prefab)
        {
            var pool = GetPool(prefab);
            while (pool.Count > 0)
            {
                var pooled = pool.Dequeue();
                if (pooled != null)
                {
                    return pooled;
                }
            }

            var instance = Instantiate(prefab, vfxPoolRoot != null ? vfxPoolRoot : transform);
            prefabByInstance[instance] = prefab;
            return instance;
        }

        private Queue<GameObject> GetPool(GameObject prefab)
        {
            if (!inactiveVFXByPrefab.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<GameObject>();
                inactiveVFXByPrefab[prefab] = pool;
            }

            return pool;
        }

        private void TickVFX(float deltaTime)
        {
            for (var i = activeVFX.Count - 1; i >= 0; i--)
            {
                var active = activeVFX[i];
                if (active.Instance == null)
                {
                    activeVFX.RemoveAt(i);
                    continue;
                }

                active.RemainingLifetime -= Mathf.Max(0f, deltaTime);
                if (active.RemainingLifetime > 0f)
                {
                    activeVFX[i] = active;
                    continue;
                }

                ReleaseVFX(active.Instance);
                activeVFX.RemoveAt(i);
            }
        }

        private void ReleaseVFX(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            StopParticles(instance);
            instance.SetActive(false);
            instance.transform.SetParent(vfxPoolRoot != null ? vfxPoolRoot : transform, false);

            if (prefabByInstance.TryGetValue(instance, out var prefab) && prefab != null)
            {
                GetPool(prefab).Enqueue(instance);
            }
            else
            {
                Destroy(instance);
            }
        }

        private void ResolveCameraShakeReceiver()
        {
            if (cameraShakeReceiver == null)
            {
                cameraShakeReceiver = Object.FindFirstObjectByType<CameraShakeReceiver>();
            }
        }

        private void CleanupLoopSources()
        {
            for (var i = activeLoopSources.Count - 1; i >= 0; i--)
            {
                if (activeLoopSources[i] == null)
                {
                    activeLoopSources.RemoveAt(i);
                }
            }
        }

        private void StopAllLoops()
        {
            for (var i = activeLoopSources.Count - 1; i >= 0; i--)
            {
                if (activeLoopSources[i] != null)
                {
                    activeLoopSources[i].Stop();
                    Destroy(activeLoopSources[i].gameObject);
                }
            }

            activeLoopSources.Clear();
        }

        private static void RestartParticles(GameObject instance)
        {
            var particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
            for (var i = 0; i < particleSystems.Length; i++)
            {
                particleSystems[i].Clear(true);
                particleSystems[i].Play(true);
            }
        }

        private static void StopParticles(GameObject instance)
        {
            var particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
            for (var i = 0; i < particleSystems.Length; i++)
            {
                particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private struct ActiveVFX
        {
            public ActiveVFX(GameObject instance, float remainingLifetime)
            {
                Instance = instance;
                RemainingLifetime = remainingLifetime;
            }

            public GameObject Instance { get; }
            public float RemainingLifetime;
        }
    }
}
