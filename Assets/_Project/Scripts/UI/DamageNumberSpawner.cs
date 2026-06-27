using System.Collections.Generic;
using UnityEngine;

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

        [Header("Pooling")]
        [SerializeField, Min(0)] private int initialPoolSize = 8;
        [SerializeField, Min(1)] private int maxPoolSize = 32;

        [Header("Display")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.25f, 0f);
        [SerializeField] private Color normalDamageColor = Color.white;
        [SerializeField] private Color dashImpactDamageColor = new Color(1f, 0.72f, 0.2f);
        [SerializeField] private Color projectileDamageColor = new Color(0.55f, 0.85f, 1f);
        [SerializeField] private Color wallSlamDamageColor = new Color(1f, 0.3f, 0.16f);
        [SerializeField] private Color chainKnockbackDamageColor = new Color(0.75f, 0.9f, 1f);
        [SerializeField] private Color criticalDamageColor = new Color(1f, 0.95f, 0.25f);
        [SerializeField] private Color ignoredDamageColor = new Color(0.7f, 0.82f, 1f);

        private readonly Queue<DamageNumberView> inactiveViews = new Queue<DamageNumberView>();
        private readonly List<DamageNumberView> activeViews = new List<DamageNumberView>();
        private int createdCount;

        public int ActiveCount => activeViews.Count;
        public int PooledCount => inactiveViews.Count;

        private void Awake()
        {
            ResolveCanvas();
            Prewarm();
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
            ResolveCanvas();

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

            var view = GetView();
            if (view == null)
            {
                return false;
            }

            view.transform.SetParent(canvasRect, false);
            view.Play(amount, anchoredPosition, ResolveStyleColor(style));
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
        }

        private void Prewarm()
        {
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
                default:
                    return normalDamageColor;
            }
        }
    }
}
