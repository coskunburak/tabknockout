using System;
using System.Collections;
using UnityEngine;

namespace TapKnockout.Combat
{
    [DisallowMultipleComponent]
    public sealed class ImpactFeedbackController : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [Header("Config")]
        [SerializeField] private ImpactFeedbackConfig config;

        [Header("Scene Hooks")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private bool useMainCameraFallback = true;

        [Header("Runtime")]
        [SerializeField] private bool listenToDashEvents = true;

        public event Action<HitContext> OnImpactFeedbackTriggered;

        private Coroutine hitPauseCoroutine;
        private Coroutine cameraShakeCoroutine;
        private Vector3 cameraOriginalLocalPosition;
        private bool hasCameraOriginalLocalPosition;

        private void Reset()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Awake()
        {
            ResolveCamera();
        }

        private void OnEnable()
        {
            if (listenToDashEvents)
            {
                DashEvents.OnDashHit -= HandleDashHit;
                DashEvents.OnDashHit += HandleDashHit;
            }
        }

        private void OnDisable()
        {
            DashEvents.OnDashHit -= HandleDashHit;

            if (hitPauseCoroutine != null)
            {
                StopCoroutine(hitPauseCoroutine);
                hitPauseCoroutine = null;
            }

            if (cameraShakeCoroutine != null)
            {
                StopCoroutine(cameraShakeCoroutine);
                cameraShakeCoroutine = null;
                RestoreCameraPosition();
            }
        }

        public bool TryTriggerFeedback(HitContext hitContext)
        {
            if (hitContext == null || !hitContext.IsDashHit)
            {
                return false;
            }

            TriggerHitPause();
            TriggerHitFlash(hitContext.Target);
            TriggerImpactVfx(hitContext);
            TriggerImpactSfx();
            TriggerCameraShake();
            OnImpactFeedbackTriggered?.Invoke(hitContext);
            return true;
        }

        private void HandleDashHit(DashHitEventArgs eventArgs)
        {
            TryTriggerFeedback(eventArgs.HitContext);
        }

        private void TriggerHitPause()
        {
            var duration = config != null ? config.DashHitPauseDuration : 0.05f;
            if (duration <= 0f || Time.timeScale <= 0f)
            {
                return;
            }

            if (hitPauseCoroutine != null)
            {
                StopCoroutine(hitPauseCoroutine);
            }

            hitPauseCoroutine = StartCoroutine(HitPauseRoutine(duration, Time.timeScale));
        }

        private IEnumerator HitPauseRoutine(float duration, float previousTimeScale)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);

            if (Mathf.Approximately(Time.timeScale, 0f))
            {
                Time.timeScale = previousTimeScale;
            }

            hitPauseCoroutine = null;
        }

        private void TriggerHitFlash(GameObject target)
        {
            var duration = config != null ? config.HitFlashDuration : 0.1f;
            if (target == null || duration <= 0f)
            {
                return;
            }

            var renderers = target.GetComponentsInChildren<Renderer>(true);
            var color = config != null ? config.HitFlashColor : Color.white;
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    StartCoroutine(FlashRendererRoutine(renderers[i], color, duration));
                }
            }
        }

        private IEnumerator FlashRendererRoutine(Renderer targetRenderer, Color color, float duration)
        {
            var originalBlock = new MaterialPropertyBlock();
            var flashBlock = new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(originalBlock);
            targetRenderer.GetPropertyBlock(flashBlock);
            flashBlock.SetColor(BaseColorId, color);
            flashBlock.SetColor(ColorId, color);
            targetRenderer.SetPropertyBlock(flashBlock);

            yield return new WaitForSecondsRealtime(duration);

            if (targetRenderer != null)
            {
                targetRenderer.SetPropertyBlock(originalBlock);
            }
        }

        private void TriggerImpactVfx(HitContext hitContext)
        {
            if (config == null || config.DashImpactVfxPrefab == null)
            {
                return;
            }

            var rotation = hitContext.HitDirection.sqrMagnitude > 0f
                ? Quaternion.LookRotation(hitContext.HitDirection, Vector3.up)
                : Quaternion.identity;
            var vfx = Instantiate(config.DashImpactVfxPrefab, hitContext.HitPoint, rotation);
            var main = vfx.main;
            Destroy(vfx.gameObject, Mathf.Max(0.1f, main.duration + main.startLifetime.constantMax));
        }

        private void TriggerImpactSfx()
        {
            if (config == null || config.DashImpactSfx == null || audioSource == null)
            {
                return;
            }

            audioSource.PlayOneShot(config.DashImpactSfx, config.DashImpactSfxVolume);
        }

        private void TriggerCameraShake()
        {
            var duration = config != null ? config.DashCameraShakeDuration : 0.08f;
            var magnitude = config != null ? config.DashCameraShakeMagnitude : 0.06f;
            if (duration <= 0f || magnitude <= 0f)
            {
                return;
            }

            ResolveCamera();
            if (cameraTransform == null)
            {
                return;
            }

            if (cameraShakeCoroutine != null)
            {
                StopCoroutine(cameraShakeCoroutine);
                RestoreCameraPosition();
            }

            cameraShakeCoroutine = StartCoroutine(CameraShakeRoutine(duration, magnitude));
        }

        private IEnumerator CameraShakeRoutine(float duration, float magnitude)
        {
            cameraOriginalLocalPosition = cameraTransform.localPosition;
            hasCameraOriginalLocalPosition = true;

            var elapsed = 0f;
            while (elapsed < duration && cameraTransform != null)
            {
                var offset = UnityEngine.Random.insideUnitSphere * magnitude;
                offset.z = 0f;
                cameraTransform.localPosition = cameraOriginalLocalPosition + offset;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            RestoreCameraPosition();
            cameraShakeCoroutine = null;
        }

        private void RestoreCameraPosition()
        {
            if (cameraTransform != null && hasCameraOriginalLocalPosition)
            {
                cameraTransform.localPosition = cameraOriginalLocalPosition;
            }

            hasCameraOriginalLocalPosition = false;
        }

        private void ResolveCamera()
        {
            if (cameraTransform != null || !useMainCameraFallback)
            {
                return;
            }

            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
        }
    }
}
