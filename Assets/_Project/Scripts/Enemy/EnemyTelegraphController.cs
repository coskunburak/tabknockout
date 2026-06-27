using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemyTelegraphController : MonoBehaviour
    {
        [SerializeField] private Transform telegraphRoot;
        [SerializeField] private Renderer telegraphRenderer;
        [SerializeField] private Color windupColor = new Color(1f, 0.35f, 0.12f, 0.75f);
        [SerializeField] private Vector3 minScale = new Vector3(0.35f, 0.02f, 0.35f);
        [SerializeField] private Vector3 maxScale = new Vector3(1.15f, 0.02f, 1.15f);

        private float duration;
        private float elapsed;
        private bool isTelegraphing;

        public bool IsTelegraphing => isTelegraphing;
        public float NormalizedProgress => duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;

        private void Reset()
        {
            telegraphRoot = transform;
            telegraphRenderer = GetComponentInChildren<Renderer>(true);
        }

        private void Awake()
        {
            if (telegraphRoot == null)
            {
                telegraphRoot = transform;
            }

            SetVisible(false);
        }

        private void Update()
        {
            if (!isTelegraphing)
            {
                return;
            }

            elapsed = Mathf.Min(duration, elapsed + Time.deltaTime);
            ApplyProgress();
        }

        public void BeginTelegraph(float windupDuration)
        {
            duration = Mathf.Max(0.01f, windupDuration);
            elapsed = 0f;
            isTelegraphing = true;
            SetVisible(true);
            ApplyProgress();
        }

        public void EndTelegraph()
        {
            isTelegraphing = false;
            SetVisible(false);
        }

        private void ApplyProgress()
        {
            if (telegraphRoot != null)
            {
                telegraphRoot.localScale = Vector3.Lerp(minScale, maxScale, NormalizedProgress);
            }

            if (telegraphRenderer != null)
            {
                telegraphRenderer.material.color = windupColor;
            }
        }

        private void SetVisible(bool visible)
        {
            if (telegraphRoot != null)
            {
                telegraphRoot.gameObject.SetActive(visible);
            }
        }
    }
}
