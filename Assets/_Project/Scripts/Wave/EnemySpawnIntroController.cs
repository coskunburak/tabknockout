using System.Collections;
using UnityEngine;

namespace TapKnockout.Wave
{
    [DisallowMultipleComponent]
    public sealed class EnemySpawnIntroController : MonoBehaviour
    {
        [SerializeField] private EnemySpawnIntroConfig config;
        [SerializeField, Min(0f)] private float fallbackDelay;
        [SerializeField] private bool logDebug;

        private Coroutine introRoutine;
        private MonoBehaviour introOwner;
        private GameObject activeIntroVisual;
        private Renderer[] hiddenRenderers;
        private Collider[] hiddenColliders;
        private bool[] hiddenRendererStates;
        private bool[] hiddenColliderStates;

        public bool IsIntroRunning { get; private set; }
        public bool HasIntro => config != null || fallbackDelay > 0f;

        private void OnDisable()
        {
            StopIntro();
        }

        public void PlayIntro(MonoBehaviour coroutineOwner, Transform spawnPoint)
        {
            if (coroutineOwner == null || !HasIntro)
            {
                return;
            }

            StopIntro();
            introOwner = coroutineOwner;
            introRoutine = coroutineOwner.StartCoroutine(PlayIntroRoutine(spawnPoint));
        }

        public void StopIntro()
        {
            if (introRoutine != null)
            {
                if (introOwner != null)
                {
                    introOwner.StopCoroutine(introRoutine);
                }

                introRoutine = null;
            }

            if (activeIntroVisual != null)
            {
                Destroy(activeIntroVisual);
                activeIntroVisual = null;
            }

            RestoreEnemyPresentation();
            IsIntroRunning = false;
            introOwner = null;
        }

        private IEnumerator PlayIntroRoutine(Transform spawnPoint)
        {
            IsIntroRunning = true;
            var delay = ResolveDelay();

            if (ShouldDeactivateEnemyDuringIntro())
            {
                HideEnemyPresentation();
            }

            SpawnIntroVisual(spawnPoint);

            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (ShouldDeactivateEnemyDuringIntro())
            {
                RestoreEnemyPresentation();
            }

            CleanupIntroVisual();
            IsIntroRunning = false;
            introRoutine = null;
            introOwner = null;

            if (logDebug)
            {
                Debug.Log($"{nameof(EnemySpawnIntroController)} completed intro for {name}.", this);
            }
        }

        private void SpawnIntroVisual(Transform spawnPoint)
        {
            if (config == null || config.IntroVisualPrefab == null)
            {
                return;
            }

            var position = spawnPoint != null ? spawnPoint.position : transform.position;
            var rotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;
            activeIntroVisual = Instantiate(config.IntroVisualPrefab, position, rotation);
            if (config.IntroVisualLifetime > 0f)
            {
                Destroy(activeIntroVisual, config.IntroVisualLifetime);
            }
        }

        private void CleanupIntroVisual()
        {
            if (activeIntroVisual == null)
            {
                return;
            }

            Destroy(activeIntroVisual);
            activeIntroVisual = null;
        }

        private float ResolveDelay()
        {
            return config != null ? config.IntroDelay : fallbackDelay;
        }

        private bool ShouldDeactivateEnemyDuringIntro()
        {
            return config != null && config.DeactivateEnemyDuringIntro;
        }

        private void HideEnemyPresentation()
        {
            hiddenRenderers = GetComponentsInChildren<Renderer>(true);
            hiddenColliders = GetComponentsInChildren<Collider>(true);
            hiddenRendererStates = new bool[hiddenRenderers.Length];
            hiddenColliderStates = new bool[hiddenColliders.Length];

            for (var i = 0; i < hiddenRenderers.Length; i++)
            {
                if (hiddenRenderers[i] != null)
                {
                    hiddenRendererStates[i] = hiddenRenderers[i].enabled;
                    hiddenRenderers[i].enabled = false;
                }
            }

            for (var i = 0; i < hiddenColliders.Length; i++)
            {
                if (hiddenColliders[i] != null)
                {
                    hiddenColliderStates[i] = hiddenColliders[i].enabled;
                    hiddenColliders[i].enabled = false;
                }
            }
        }

        private void RestoreEnemyPresentation()
        {
            if (hiddenRenderers != null)
            {
                for (var i = 0; i < hiddenRenderers.Length; i++)
                {
                    if (hiddenRenderers[i] != null)
                    {
                        hiddenRenderers[i].enabled = hiddenRendererStates == null || i >= hiddenRendererStates.Length || hiddenRendererStates[i];
                    }
                }
            }

            if (hiddenColliders != null)
            {
                for (var i = 0; i < hiddenColliders.Length; i++)
                {
                    if (hiddenColliders[i] != null)
                    {
                        hiddenColliders[i].enabled = hiddenColliderStates == null || i >= hiddenColliderStates.Length || hiddenColliderStates[i];
                    }
                }
            }

            hiddenRenderers = null;
            hiddenColliders = null;
            hiddenRendererStates = null;
            hiddenColliderStates = null;
        }
    }
}
