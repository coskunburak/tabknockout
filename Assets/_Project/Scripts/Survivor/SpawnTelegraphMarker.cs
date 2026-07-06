using UnityEngine;

namespace TapKnockout.Survivor
{
    [DisallowMultipleComponent]
    public sealed class SpawnTelegraphMarker : MonoBehaviour
    {
        [Header("Shape")]
        [SerializeField, Min(8)] private int segments = 48;
        [SerializeField, Min(0.05f)] private float radius = 0.85f;
        [SerializeField, Min(0.005f)] private float lineWidth = 0.045f;
        [SerializeField, Min(0f)] private float heightOffset = 0.05f;
        [SerializeField] private Color color = new Color(1f, 0.42f, 0.08f, 0.85f);

        private LineRenderer lineRenderer;
        private Material runtimeMaterial;
        private float duration = 0.45f;
        private float remaining;

        public bool IsPlaying => gameObject.activeSelf && remaining > 0f;

        private void Awake()
        {
            EnsureRenderer();
            RedrawRing();
        }

        private void OnValidate()
        {
            segments = Mathf.Max(8, segments);
            radius = Mathf.Max(0.05f, radius);
            lineWidth = Mathf.Max(0.005f, lineWidth);
            heightOffset = Mathf.Max(0f, heightOffset);

            if (lineRenderer != null)
            {
                RedrawRing();
                ApplyVisualProgress(1f);
            }
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }

        public void Play(Vector3 worldPosition, float markerRadius, float markerDuration, Color markerColor)
        {
            radius = Mathf.Max(0.05f, markerRadius);
            duration = Mathf.Max(0.01f, markerDuration);
            remaining = duration;
            color = markerColor;
            transform.position = worldPosition + Vector3.up * heightOffset;
            transform.rotation = Quaternion.identity;
            EnsureRenderer();
            RedrawRing();
            ApplyVisualProgress(1f);
            gameObject.SetActive(true);
        }

        public void Tick(float deltaTime)
        {
            if (remaining <= 0f)
            {
                return;
            }

            remaining = Mathf.Max(0f, remaining - Mathf.Max(0f, deltaTime));
            ApplyVisualProgress(duration <= 0f ? 0f : remaining / duration);
        }

        public void StopAndHide()
        {
            remaining = 0f;
            gameObject.SetActive(false);
        }

        private void EnsureRenderer()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
                if (lineRenderer == null)
                {
                    lineRenderer = gameObject.AddComponent<LineRenderer>();
                }
            }

            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = false;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.widthMultiplier = lineWidth;

            if (lineRenderer.sharedMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    runtimeMaterial = new Material(shader)
                    {
                        name = "Runtime Spawn Telegraph Marker"
                    };
                    lineRenderer.sharedMaterial = runtimeMaterial;
                }
            }
        }

        private void RedrawRing()
        {
            EnsureRenderer();
            lineRenderer.positionCount = segments;
            for (var i = 0; i < segments; i++)
            {
                var angle = (i / (float)segments) * Mathf.PI * 2f;
                lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }

        private void ApplyVisualProgress(float normalizedRemaining)
        {
            EnsureRenderer();
            var pulse = 0.65f + Mathf.PingPong((1f - normalizedRemaining) * 4f, 0.35f);
            var visualColor = color;
            visualColor.a *= Mathf.Clamp01(normalizedRemaining) * pulse;
            lineRenderer.startColor = visualColor;
            lineRenderer.endColor = visualColor;
            lineRenderer.widthMultiplier = lineWidth * Mathf.Lerp(1.35f, 0.85f, 1f - normalizedRemaining);
        }
    }
}
