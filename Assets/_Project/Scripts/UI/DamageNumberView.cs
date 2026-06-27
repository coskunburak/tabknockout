using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class DamageNumberView : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField, Min(0.05f)] private float lifetime = 0.65f;
        [SerializeField] private Vector2 drift = new Vector2(0f, 64f);

        private RectTransform rectTransform;
        private Vector2 startPosition;
        private float remainingLifetime;
        private bool isPlaying;

        public bool IsPlaying => isPlaying;

        private void Reset()
        {
            rectTransform = GetComponent<RectTransform>();
            label = GetComponentInChildren<Text>(true);
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (label == null)
            {
                label = GetComponentInChildren<Text>(true);
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void OnValidate()
        {
            lifetime = Mathf.Max(0.05f, lifetime);
        }

        public void Play(float amount, Vector2 anchoredPosition, Color color)
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            startPosition = anchoredPosition;
            remainingLifetime = lifetime;
            isPlaying = true;
            gameObject.SetActive(true);

            rectTransform.anchoredPosition = anchoredPosition;

            if (label != null)
            {
                label.text = Mathf.CeilToInt(Mathf.Max(0f, amount)).ToString();
                label.color = color;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        public bool Tick(float deltaTime)
        {
            if (!isPlaying)
            {
                return false;
            }

            remainingLifetime -= Mathf.Max(0f, deltaTime);
            var normalized = lifetime > 0f ? Mathf.Clamp01(1f - remainingLifetime / lifetime) : 1f;

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = startPosition + drift * normalized;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - normalized;
            }

            if (remainingLifetime > 0f)
            {
                return false;
            }

            StopAndHide();
            return true;
        }

        public void StopAndHide()
        {
            isPlaying = false;
            remainingLifetime = 0f;
            gameObject.SetActive(false);
        }
    }
}
