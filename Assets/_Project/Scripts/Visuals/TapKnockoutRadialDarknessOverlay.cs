using UnityEngine;
using UnityEngine.UI;
using TapKnockout.Player;

namespace TapKnockout.Visuals
{
    [DisallowMultipleComponent]
    public sealed class TapKnockoutRadialDarknessOverlay : MaskableGraphic
    {
        private const int SegmentCount = 96;
        private const int RingCount = 5;
        private const float AutoResolveIntervalSeconds = 0.5f;

        [SerializeField] private UnityEngine.Camera worldCamera;
        [SerializeField] private Transform target;
        [SerializeField] private bool followTarget = true;
        [SerializeField] private bool autoResolveMissingReferences = true;
        [SerializeField] private Vector2 fallbackViewportCenter = new Vector2(0.5f, 0.49f);
        [SerializeField, Range(0.05f, 0.6f)] private float clearRadius = 0.18f;
        [SerializeField, Range(0.3f, 1.6f)] private float fullDarkRadius = 0.62f;
        [SerializeField, Range(0f, 1f)] private float edgeOpacity = 0.92f;
        [SerializeField, Min(0f)] private float centerFollowSharpness = 28f;

        private Vector2 currentViewportCenter = new Vector2(0.5f, 0.49f);
        private float nextAutoResolveTime;
        private bool hasCenter;

        public UnityEngine.Camera WorldCamera => worldCamera;
        public Transform Target => target;
        public float ClearRadius => clearRadius;
        public float FullDarkRadius => fullDarkRadius;
        public float EdgeOpacity => edgeOpacity;

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
            currentViewportCenter = fallbackViewportCenter;
            hasCenter = true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            raycastTarget = false;
            SetVerticesDirty();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            fallbackViewportCenter = new Vector2(
                Mathf.Clamp01(fallbackViewportCenter.x),
                Mathf.Clamp01(fallbackViewportCenter.y));
            clearRadius = Mathf.Clamp(clearRadius, 0.05f, 0.6f);
            fullDarkRadius = Mathf.Clamp(fullDarkRadius, Mathf.Max(0.3f, clearRadius + 0.05f), 1.6f);
            edgeOpacity = Mathf.Clamp01(edgeOpacity);
            centerFollowSharpness = Mathf.Max(0f, centerFollowSharpness);
            raycastTarget = false;
            SetVerticesDirty();
        }

        private void LateUpdate()
        {
            TryAutoResolveReferences();

            var desiredCenter = ResolveDesiredViewportCenter();
            if (!hasCenter)
            {
                currentViewportCenter = desiredCenter;
                hasCenter = true;
                SetVerticesDirty();
                return;
            }

            var t = centerFollowSharpness <= 0f
                ? 1f
                : 1f - Mathf.Exp(-centerFollowSharpness * Time.unscaledDeltaTime);
            var nextCenter = Vector2.Lerp(currentViewportCenter, desiredCenter, t);
            if ((nextCenter - currentViewportCenter).sqrMagnitude > 0.000001f)
            {
                currentViewportCenter = nextCenter;
                SetVerticesDirty();
            }
        }

        public void Configure(TapKnockoutRenderProfile profile, UnityEngine.Camera camera, Transform followTransform)
        {
            worldCamera = camera;
            target = followTransform;
            followTarget = true;

            if (profile != null)
            {
                enabled = profile.RadialDarknessOverlayEnabled;
                color = profile.RadialDarknessColor;
                edgeOpacity = profile.RadialDarknessEdgeOpacity;
                clearRadius = profile.RadialDarknessClearRadius;
                fullDarkRadius = profile.RadialDarknessFullRadius;
                centerFollowSharpness = profile.RadialDarknessFollowSharpness;
            }

            raycastTarget = false;
            OnValidate();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (edgeOpacity <= 0f)
            {
                return;
            }

            var rect = rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            var radiusScale = Mathf.Min(rect.width, rect.height);
            var innerRadius = clearRadius * radiusScale;
            var fullRadius = fullDarkRadius * radiusScale;
            var centerPoint = new Vector2(
                Mathf.Lerp(rect.xMin, rect.xMax, currentViewportCenter.x),
                Mathf.Lerp(rect.yMin, rect.yMax, currentViewportCenter.y));
            var lightRadius = Mathf.Lerp(innerRadius, fullRadius, 0.32f);
            var middleRadius = Mathf.Lerp(innerRadius, fullRadius, 0.64f);
            var outerRadius = ResolveFarthestCornerDistance(rect, centerPoint) + 4f;

            var clearColor = color;
            clearColor.a = 0f;
            var lightColor = color;
            lightColor.a *= edgeOpacity * 0.08f;
            var midColor = color;
            midColor.a *= edgeOpacity * 0.34f;
            var fullColor = color;
            fullColor.a *= edgeOpacity * 0.88f;
            var edgeColor = color;
            edgeColor.a *= edgeOpacity;

            for (var i = 0; i <= SegmentCount; i++)
            {
                var angle = i / (float)SegmentCount * Mathf.PI * 2f;
                var unit = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                AddVertex(vh, centerPoint + unit * innerRadius, clearColor);
                AddVertex(vh, centerPoint + unit * lightRadius, lightColor);
                AddVertex(vh, centerPoint + unit * middleRadius, midColor);
                AddVertex(vh, centerPoint + unit * fullRadius, fullColor);
                AddVertex(vh, centerPoint + unit * outerRadius, edgeColor);
            }

            for (var i = 0; i < SegmentCount; i++)
            {
                var index = i * RingCount;
                var next = index + RingCount;
                for (var ring = 0; ring < RingCount - 1; ring++)
                {
                    vh.AddTriangle(index + ring, next + ring, index + ring + 1);
                    vh.AddTriangle(index + ring + 1, next + ring, next + ring + 1);
                }
            }
        }

        private void TryAutoResolveReferences()
        {
            if (!autoResolveMissingReferences || Time.unscaledTime < nextAutoResolveTime)
            {
                return;
            }

            nextAutoResolveTime = Time.unscaledTime + AutoResolveIntervalSeconds;
            if (worldCamera == null)
            {
                worldCamera = UnityEngine.Camera.main;
            }

            if (target != null)
            {
                return;
            }

            var playerTagged = GameObject.FindGameObjectWithTag("Player");
            if (playerTagged != null)
            {
                target = playerTagged.transform;
                return;
            }

            var movement = UnityEngine.Object.FindAnyObjectByType<PlayerMovementController>();
            if (movement != null)
            {
                target = movement.transform;
            }
        }

        private Vector2 ResolveDesiredViewportCenter()
        {
            if (followTarget && target != null && worldCamera != null)
            {
                var viewport = worldCamera.WorldToViewportPoint(target.position);
                if (viewport.z > 0f)
                {
                    return new Vector2(
                        Mathf.Clamp(viewport.x, 0.1f, 0.9f),
                        Mathf.Clamp(viewport.y, 0.1f, 0.9f));
                }
            }

            return fallbackViewportCenter;
        }

        private static float ResolveFarthestCornerDistance(Rect rect, Vector2 centerPoint)
        {
            var topLeft = new Vector2(rect.xMin, rect.yMax);
            var topRight = new Vector2(rect.xMax, rect.yMax);
            var bottomLeft = new Vector2(rect.xMin, rect.yMin);
            var bottomRight = new Vector2(rect.xMax, rect.yMin);
            return Mathf.Max(
                Mathf.Max(Vector2.Distance(centerPoint, topLeft), Vector2.Distance(centerPoint, topRight)),
                Mathf.Max(Vector2.Distance(centerPoint, bottomLeft), Vector2.Distance(centerPoint, bottomRight)));
        }

        private static void AddVertex(VertexHelper vh, Vector2 position, Color vertexColor)
        {
            var vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = vertexColor;
            vh.AddVert(vertex);
        }
    }
}
