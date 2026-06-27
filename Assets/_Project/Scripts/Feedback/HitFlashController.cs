using UnityEngine;

namespace TapKnockout.Feedback
{
    [DisallowMultipleComponent]
    public sealed class HitFlashController : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Color fallbackFlashColor = Color.white;
        [SerializeField, Range(0f, 0.25f)] private float fallbackDuration = 0.1f;
        [SerializeField] private bool includeInactiveRenderers = true;

        private Renderer[] cachedRenderers;
        private MaterialPropertyBlock[] originalBlocks;
        private bool hasCapturedOriginalBlocks;
        private bool isFlashing;
        private float remainingDuration;

        public int RendererCount => cachedRenderers != null ? cachedRenderers.Length : 0;
        public bool IsFlashing => isFlashing;

        private void Awake()
        {
            CacheRenderers();
        }

        private void OnValidate()
        {
            fallbackDuration = Mathf.Clamp(fallbackDuration, 0f, 0.25f);
        }

        private void Update()
        {
            if (!isFlashing)
            {
                return;
            }

            remainingDuration -= Time.unscaledDeltaTime;
            if (remainingDuration <= 0f)
            {
                RestoreOriginalBlocks();
            }
        }

        private void OnDisable()
        {
            RestoreOriginalBlocks();
        }

        public bool Flash(float duration)
        {
            return Flash(fallbackFlashColor, duration);
        }

        public bool Flash(Color color, float duration)
        {
            CacheRenderersIfNeeded();

            if (cachedRenderers == null || cachedRenderers.Length == 0)
            {
                return false;
            }

            var resolvedDuration = duration > 0f ? duration : fallbackDuration;
            if (resolvedDuration <= 0f)
            {
                return false;
            }

            if (!hasCapturedOriginalBlocks)
            {
                CaptureOriginalBlocks();
            }

            ApplyFlashColor(color);
            remainingDuration = Mathf.Max(remainingDuration, resolvedDuration);
            isFlashing = true;
            return true;
        }

        public void CacheRenderers()
        {
            cachedRenderers = GetComponentsInChildren<Renderer>(includeInactiveRenderers);
            originalBlocks = new MaterialPropertyBlock[cachedRenderers.Length];
            for (var i = 0; i < originalBlocks.Length; i++)
            {
                originalBlocks[i] = new MaterialPropertyBlock();
            }

            hasCapturedOriginalBlocks = false;
        }

        private void CacheRenderersIfNeeded()
        {
            if (cachedRenderers == null)
            {
                CacheRenderers();
            }
        }

        private void CaptureOriginalBlocks()
        {
            for (var i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] != null)
                {
                    cachedRenderers[i].GetPropertyBlock(originalBlocks[i]);
                }
            }

            hasCapturedOriginalBlocks = true;
        }

        private void ApplyFlashColor(Color color)
        {
            var flashBlock = new MaterialPropertyBlock();
            for (var i = 0; i < cachedRenderers.Length; i++)
            {
                var targetRenderer = cachedRenderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                targetRenderer.GetPropertyBlock(flashBlock);
                flashBlock.SetColor(BaseColorId, color);
                flashBlock.SetColor(ColorId, color);
                targetRenderer.SetPropertyBlock(flashBlock);
                flashBlock.Clear();
            }
        }

        private void RestoreOriginalBlocks()
        {
            if (!hasCapturedOriginalBlocks || cachedRenderers == null || originalBlocks == null)
            {
                isFlashing = false;
                remainingDuration = 0f;
                return;
            }

            for (var i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] != null)
                {
                    cachedRenderers[i].SetPropertyBlock(originalBlocks[i]);
                }
            }

            hasCapturedOriginalBlocks = false;
            isFlashing = false;
            remainingDuration = 0f;
        }
    }
}
