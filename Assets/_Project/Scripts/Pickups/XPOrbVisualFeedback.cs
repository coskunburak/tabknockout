using UnityEngine;

namespace TapKnockout.Pickups
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(XPOrb))]
    public sealed class XPOrbVisualFeedback : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color glowColor = new Color(0.28f, 0.95f, 1f, 1f);
        [SerializeField] private Color emissionColor = new Color(0.18f, 0.95f, 0.55f, 1f);
        [SerializeField, Range(0f, 4f)] private float emissionIntensity = 1.4f;
        [SerializeField, Range(0f, 0.4f)] private float bobAmplitude = 0.08f;
        [SerializeField, Range(0f, 8f)] private float bobFrequency = 3.2f;
        [SerializeField, Range(0f, 0.3f)] private float pulseScale = 0.08f;
        [SerializeField, Range(0f, 360f)] private float rotateDegreesPerSecond = 90f;

        private MaterialPropertyBlock propertyBlock;
        private MaterialPropertyBlock originalBlock;
        private Vector3 baseLocalPosition;
        private Vector3 baseLocalScale;
        private float phase;
        private bool hasBaseTransform;
        private bool hasOriginalBlock;

        public bool HasRenderer => targetRenderer != null;

        private void Reset()
        {
            targetRenderer = GetComponentInChildren<Renderer>(true);
        }

        private void Awake()
        {
            CacheReferences();
            CacheBaseTransform();
        }

        private void OnEnable()
        {
            CacheReferences();
            CacheBaseTransform();
            CaptureOriginalBlock();
            ApplyGlow();
        }

        private void Update()
        {
            TickVisual(Time.deltaTime);
        }

        private void OnDisable()
        {
            RestoreOriginalBlock();
            RestoreBaseTransform();
        }

        private void OnValidate()
        {
            emissionIntensity = Mathf.Clamp(emissionIntensity, 0f, 4f);
            bobAmplitude = Mathf.Clamp(bobAmplitude, 0f, 0.4f);
            bobFrequency = Mathf.Clamp(bobFrequency, 0f, 8f);
            pulseScale = Mathf.Clamp(pulseScale, 0f, 0.3f);
            rotateDegreesPerSecond = Mathf.Clamp(rotateDegreesPerSecond, 0f, 360f);
        }

        public void TickVisual(float deltaTime)
        {
            if (!hasBaseTransform)
            {
                CacheBaseTransform();
            }

            phase += Mathf.Max(0f, deltaTime) * bobFrequency;
            var bob = bobAmplitude > 0f ? Mathf.Sin(phase) * bobAmplitude : 0f;
            var pulse = pulseScale > 0f ? 1f + Mathf.Sin(phase * 1.7f) * pulseScale : 1f;
            transform.localPosition = baseLocalPosition + Vector3.up * bob;
            transform.localScale = baseLocalScale * Mathf.Max(0.01f, pulse);

            if (rotateDegreesPerSecond > 0f && deltaTime > 0f)
            {
                transform.Rotate(Vector3.up, rotateDegreesPerSecond * deltaTime, Space.Self);
            }
        }

        public void ApplyGlow()
        {
            if (targetRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, glowColor);
            propertyBlock.SetColor(ColorId, glowColor);
            propertyBlock.SetColor(EmissionColorId, emissionColor * Mathf.Max(0f, emissionIntensity));
            targetRenderer.SetPropertyBlock(propertyBlock);
            propertyBlock.Clear();
        }

        private void CacheReferences()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<Renderer>(true);
            }
        }

        private void CacheBaseTransform()
        {
            if (hasBaseTransform)
            {
                return;
            }

            baseLocalPosition = transform.localPosition;
            baseLocalScale = transform.localScale;
            hasBaseTransform = true;
        }

        private void CaptureOriginalBlock()
        {
            if (targetRenderer == null || hasOriginalBlock)
            {
                return;
            }

            originalBlock ??= new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(originalBlock);
            hasOriginalBlock = true;
        }

        private void RestoreOriginalBlock()
        {
            if (targetRenderer == null || !hasOriginalBlock || originalBlock == null)
            {
                hasOriginalBlock = false;
                return;
            }

            targetRenderer.SetPropertyBlock(originalBlock);
            hasOriginalBlock = false;
        }

        private void RestoreBaseTransform()
        {
            if (!hasBaseTransform)
            {
                return;
            }

            transform.localPosition = baseLocalPosition;
            transform.localScale = baseLocalScale;
            phase = 0f;
        }
    }
}
