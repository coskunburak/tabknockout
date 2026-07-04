using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.UI
{
    [DisallowMultipleComponent]
    public sealed class DamageNumberSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private DamageNumberView numberPrefab;
        [SerializeField] private UnityEngine.Camera worldCamera;
        [SerializeField] private bool useMainCameraFallback = true;
        [SerializeField] private bool createRuntimeFallbackPrefab = true;

        [Header("Pooling")]
        [SerializeField, Min(0)] private int initialPoolSize = 8;
        [SerializeField, Min(1)] private int maxPoolSize = 32;

        [Header("Display")]
        [SerializeField] private bool numbersEnabled = true;
        [SerializeField, Min(0f)] private float minimumDamageToShow = 1f;
        [SerializeField, Min(1)] private int maxNumbersPerSecond = 24;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.25f, 0f);
        [SerializeField] private Color normalDamageColor = Color.white;
        [SerializeField] private Color dashImpactDamageColor = new Color(1f, 0.72f, 0.2f);
        [SerializeField] private Color projectileDamageColor = new Color(0.55f, 0.85f, 1f);
        [SerializeField] private Color wallSlamDamageColor = new Color(1f, 0.3f, 0.16f);
        [SerializeField] private Color chainKnockbackDamageColor = new Color(0.75f, 0.9f, 1f);
        [SerializeField] private Color criticalDamageColor = new Color(1f, 0.95f, 0.25f);
        [SerializeField] private Color ignoredDamageColor = new Color(0.7f, 0.82f, 1f);
        [SerializeField] private Color skillDamageColor = new Color(1f, 0.58f, 0.18f);
        [SerializeField] private Color heavyProjectileDamageColor = new Color(1f, 0.88f, 0.32f);
        [SerializeField] private Color bossDamageColor = new Color(1f, 0.42f, 0.28f);
        [SerializeField] private Color playerDamageColor = new Color(1f, 0.18f, 0.12f);

        private readonly Queue<DamageNumberView> inactiveViews = new Queue<DamageNumberView>();
        private readonly List<DamageNumberView> activeViews = new List<DamageNumberView>();
        private int createdCount;
        private float rateWindowStartedAt = -1f;
        private int numbersShownInRateWindow;

        public int ActiveCount => activeViews.Count;
        public int PooledCount => inactiveViews.Count;

        private void Awake()
        {
            ResolveCanvas();
            Prewarm();
        }

        private void OnValidate()
        {
            initialPoolSize = Mathf.Max(0, initialPoolSize);
            maxPoolSize = Mathf.Max(1, maxPoolSize);
            maxPoolSize = Mathf.Max(initialPoolSize, maxPoolSize);
            minimumDamageToShow = Mathf.Max(0f, minimumDamageToShow);
            maxNumbersPerSecond = Mathf.Max(1, maxNumbersPerSecond);
        }

        private void Update()
        {
            var deltaTime = Time.unscaledDeltaTime;
            for (var i = activeViews.Count - 1; i >= 0; i--)
            {
                var view = activeViews[i];
                if (view == null || view.Tick(deltaTime))
                {
                    activeViews.RemoveAt(i);
                    if (view != null)
                    {
                        inactiveViews.Enqueue(view);
                    }
                }
            }
        }

        public bool ShowDamage(float amount, Vector3 worldPosition, GameObject target = null)
        {
            return ShowDamage(amount, worldPosition, target, DamageNumberStyle.Normal);
        }

        public bool ShowDamage(float amount, Vector3 worldPosition, GameObject target, DamageNumberStyle style)
        {
            return ShowDamage(amount, worldPosition, target, style, 1f);
        }

        public bool ShowDamage(float amount, Vector3 worldPosition, GameObject target, DamageNumberStyle style, float scaleMultiplier)
        {
            if (!numbersEnabled || amount < minimumDamageToShow)
            {
                return false;
            }

            ResolveCanvas();
            EnsureRuntimeFallbackPrefab();

            if (targetCanvas == null || numberPrefab == null)
            {
                return false;
            }

            var canvasRect = targetCanvas.transform as RectTransform;
            if (canvasRect == null)
            {
                return false;
            }

            var position = target != null ? target.transform.position : worldPosition;
            if (!TryResolveAnchoredPosition(position + worldOffset, canvasRect, out var anchoredPosition))
            {
                return false;
            }

            if (!TryReserveRateLimitSlot())
            {
                return false;
            }

            var view = GetView();
            if (view == null)
            {
                return false;
            }

            view.transform.SetParent(canvasRect, false);
            view.Play(amount, anchoredPosition, ResolveStyleColor(style), scaleMultiplier);
            activeViews.Add(view);
            return true;
        }

        public void Clear()
        {
            for (var i = activeViews.Count - 1; i >= 0; i--)
            {
                if (activeViews[i] != null)
                {
                    activeViews[i].StopAndHide();
                    inactiveViews.Enqueue(activeViews[i]);
                }
            }

            activeViews.Clear();
            numbersShownInRateWindow = 0;
            rateWindowStartedAt = -1f;
        }

        private bool TryReserveRateLimitSlot()
        {
            var now = Time.unscaledTime;
            if (rateWindowStartedAt < 0f || now - rateWindowStartedAt >= 1f)
            {
                rateWindowStartedAt = now;
                numbersShownInRateWindow = 0;
            }

            if (numbersShownInRateWindow >= maxNumbersPerSecond)
            {
                return false;
            }

            numbersShownInRateWindow++;
            return true;
        }

        private void Prewarm()
        {
            EnsureRuntimeFallbackPrefab();

            if (numberPrefab == null || targetCanvas == null)
            {
                return;
            }

            while (inactiveViews.Count < initialPoolSize && createdCount < maxPoolSize)
            {
                var view = CreateView();
                view.StopAndHide();
                inactiveViews.Enqueue(view);
            }
        }

        private DamageNumberView GetView()
        {
            while (inactiveViews.Count > 0)
            {
                var view = inactiveViews.Dequeue();
                if (view != null)
                {
                    return view;
                }
            }

            return createdCount < maxPoolSize ? CreateView() : null;
        }

        private DamageNumberView CreateView()
        {
            var view = Instantiate(numberPrefab, targetCanvas.transform);
            createdCount++;
            return view;
        }

        private void EnsureRuntimeFallbackPrefab()
        {
            if (!createRuntimeFallbackPrefab || numberPrefab != null || targetCanvas == null)
            {
                return;
            }

            var root = new GameObject("RuntimeDamageNumberView");
            root.transform.SetParent(targetCanvas.transform, false);
            var rectTransform = root.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(96f, 40f);
            root.AddComponent<CanvasGroup>();

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(root.transform, false);
            var labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelObject.AddComponent<Text>();
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 24;
            label.fontStyle = FontStyle.Bold;
            label.raycastTarget = false;
            label.font = ResolveRuntimeFont();

            numberPrefab = root.AddComponent<DamageNumberView>();
            root.SetActive(false);
        }

        private static Font ResolveRuntimeFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private bool TryResolveAnchoredPosition(Vector3 worldPosition, RectTransform canvasRect, out Vector2 anchoredPosition)
        {
            var cameraForWorld = ResolveWorldCamera();
            if (cameraForWorld == null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                anchoredPosition = Vector2.zero;
                return false;
            }

            var screenPosition = RectTransformUtility.WorldToScreenPoint(cameraForWorld, worldPosition);
            var cameraForCanvas = targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cameraForWorld;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, cameraForCanvas, out anchoredPosition);
        }

        private UnityEngine.Camera ResolveWorldCamera()
        {
            if (worldCamera != null)
            {
                return worldCamera;
            }

            if (!useMainCameraFallback)
            {
                return null;
            }

            worldCamera = UnityEngine.Camera.main;
            return worldCamera;
        }

        private void ResolveCanvas()
        {
            if (targetCanvas != null)
            {
                return;
            }

            targetCanvas = GetComponentInParent<Canvas>();
        }

        private Color ResolveStyleColor(DamageNumberStyle style)
        {
            switch (style)
            {
                case DamageNumberStyle.DashImpact:
                    return dashImpactDamageColor;
                case DamageNumberStyle.Projectile:
                    return projectileDamageColor;
                case DamageNumberStyle.WallSlam:
                    return wallSlamDamageColor;
                case DamageNumberStyle.ChainKnockback:
                    return chainKnockbackDamageColor;
                case DamageNumberStyle.Critical:
                    return criticalDamageColor;
                case DamageNumberStyle.Ignored:
                    return ignoredDamageColor;
                case DamageNumberStyle.Skill:
                    return skillDamageColor;
                case DamageNumberStyle.HeavyProjectile:
                    return heavyProjectileDamageColor;
                case DamageNumberStyle.Boss:
                    return bossDamageColor;
                case DamageNumberStyle.PlayerDamage:
                    return playerDamageColor;
                default:
                    return normalDamageColor;
            }
        }
    }
}
