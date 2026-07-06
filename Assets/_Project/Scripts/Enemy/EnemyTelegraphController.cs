using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemyTelegraphController : MonoBehaviour, IPoolLifecycle
    {
        [SerializeField] private EnemyTelegraphConfig config;
        [SerializeField] private Transform telegraphRoot;
        [SerializeField] private Renderer telegraphRenderer;
        [SerializeField] private LineRenderer telegraphLineRenderer;
        [SerializeField] private Color windupColor = new Color(1f, 0.35f, 0.12f, 0.75f);
        [SerializeField] private Vector3 minScale = new Vector3(0.35f, 0.02f, 0.35f);
        [SerializeField] private Vector3 maxScale = new Vector3(1.15f, 0.02f, 1.15f);

        private EnemyTelegraphConfig runtimeConfig;
        private EnemyTelegraphType runtimeTelegraphType = EnemyTelegraphType.Circle;
        private Transform followTarget;
        private Vector3 lockedWorldPosition;
        private float duration;
        private float elapsed;
        private bool isTelegraphing;

        public bool IsTelegraphing => isTelegraphing;
        public float NormalizedProgress => duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
        public EnemyTelegraphType RuntimeTelegraphType => runtimeTelegraphType;

        private void Reset()
        {
            telegraphRoot = transform;
            telegraphRenderer = GetComponentInChildren<Renderer>(true);
            telegraphLineRenderer = GetComponentInChildren<LineRenderer>(true);
        }

        private void Awake()
        {
            if (telegraphRoot == null)
            {
                telegraphRoot = transform;
            }

            if (telegraphLineRenderer == null)
            {
                telegraphLineRenderer = telegraphRoot.GetComponentInChildren<LineRenderer>(true);
            }

            if (telegraphRenderer == null)
            {
                telegraphRenderer = telegraphLineRenderer != null
                    ? telegraphLineRenderer
                    : telegraphRoot.GetComponentInChildren<Renderer>(true);
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
            UpdateFollowPosition();
            ApplyProgress();
        }

        public void BeginTelegraph(float windupDuration)
        {
            BeginTelegraph(config, config != null ? config.TelegraphType : EnemyTelegraphType.Circle, windupDuration, transform, null);
        }

        public void BeginTelegraph(
            EnemyTelegraphConfig telegraphConfig,
            EnemyTelegraphType telegraphType,
            float windupDuration,
            Transform origin,
            Transform target)
        {
            runtimeConfig = telegraphConfig != null ? telegraphConfig : config;
            runtimeTelegraphType = telegraphType;
            followTarget = target;
            lockedWorldPosition = ResolveStartPosition(origin, target);
            duration = Mathf.Max(0.01f, windupDuration > 0f ? windupDuration : runtimeConfig != null ? runtimeConfig.Duration : 0.01f);
            elapsed = 0f;
            isTelegraphing = true;
            SetVisible(true);
            ApplyPlacement(origin);
            ApplyProgress();
        }

        public void BeginTelegraphAtPosition(
            EnemyTelegraphConfig telegraphConfig,
            EnemyTelegraphType telegraphType,
            float windupDuration,
            Vector3 worldPosition,
            Quaternion worldRotation)
        {
            runtimeConfig = telegraphConfig != null ? telegraphConfig : config;
            runtimeTelegraphType = telegraphType;
            followTarget = null;
            lockedWorldPosition = worldPosition;
            duration = Mathf.Max(0.01f, windupDuration > 0f ? windupDuration : runtimeConfig != null ? runtimeConfig.Duration : 0.01f);
            elapsed = 0f;
            isTelegraphing = true;
            SetVisible(true);

            if (telegraphRoot != null)
            {
                telegraphRoot.position = lockedWorldPosition;
                telegraphRoot.rotation = worldRotation;
            }

            ApplyProgress();
        }

        public void EndTelegraph()
        {
            isTelegraphing = false;
            followTarget = null;
            SetVisible(false);
        }

        public void ResetRuntimeState()
        {
            runtimeConfig = config;
            runtimeTelegraphType = EnemyTelegraphType.Circle;
            followTarget = null;
            lockedWorldPosition = Vector3.zero;
            duration = 0f;
            elapsed = 0f;
            isTelegraphing = false;
            SetVisible(false);
        }

        public void OnBeforeSpawnFromPool()
        {
            ResetRuntimeState();
        }

        public void OnSpawnedFromPool()
        {
        }

        public void OnBeforeDespawnToPool()
        {
            ResetRuntimeState();
        }

        public void ResetForPool()
        {
            ResetRuntimeState();
        }

        private void ApplyProgress()
        {
            if (telegraphRoot != null)
            {
                var targetScale = ResolveTargetScale(runtimeConfig, runtimeTelegraphType);
                var startScale = runtimeConfig != null && runtimeConfig.ShowFillProgress ? minScale : targetScale;
                telegraphRoot.localScale = Vector3.Lerp(startScale, targetScale, NormalizedProgress);
            }

            if (telegraphRenderer != null)
            {
                telegraphRenderer.material.color = runtimeConfig != null ? runtimeConfig.Color : windupColor;
            }

            if (telegraphLineRenderer != null)
            {
                var color = runtimeConfig != null ? runtimeConfig.Color : windupColor;
                telegraphLineRenderer.startColor = color;
                telegraphLineRenderer.endColor = color;
                ApplyLineRendererShape(runtimeTelegraphType);
            }
        }

        private void SetVisible(bool visible)
        {
            if (telegraphRoot != null)
            {
                telegraphRoot.gameObject.SetActive(visible);
            }
        }

        private void ApplyPlacement(Transform origin)
        {
            if (telegraphRoot == null)
            {
                return;
            }

            telegraphRoot.position = lockedWorldPosition;

            if (origin != null && runtimeTelegraphType != EnemyTelegraphType.Circle && runtimeTelegraphType != EnemyTelegraphType.Area)
            {
                telegraphRoot.rotation = origin.rotation;
            }
        }

        private void UpdateFollowPosition()
        {
            if (telegraphRoot == null || runtimeConfig == null || !runtimeConfig.FollowTarget || runtimeConfig.LockAtCastStart || followTarget == null)
            {
                return;
            }

            telegraphRoot.position = followTarget.position;
        }

        private Vector3 ResolveStartPosition(Transform origin, Transform target)
        {
            if (runtimeConfig != null && runtimeConfig.FollowTarget && target != null && !runtimeConfig.LockAtCastStart)
            {
                return target.position;
            }

            if (runtimeConfig != null && runtimeConfig.FollowTarget && target != null)
            {
                return target.position;
            }

            return origin != null ? origin.position : transform.position;
        }

        private static Vector3 ResolveTargetScale(EnemyTelegraphConfig telegraphConfig, EnemyTelegraphType telegraphType)
        {
            if (telegraphConfig == null)
            {
                return Vector3.one;
            }

            switch (telegraphType)
            {
                case EnemyTelegraphType.Line:
                case EnemyTelegraphType.ChargePath:
                    return new Vector3(
                        Mathf.Max(0.01f, telegraphConfig.Width),
                        0.02f,
                        Mathf.Max(0.01f, telegraphConfig.Length));
                case EnemyTelegraphType.Cone:
                    return new Vector3(
                        Mathf.Max(0.01f, telegraphConfig.Width),
                        0.02f,
                        Mathf.Max(0.01f, telegraphConfig.Length));
                default:
                    var diameter = Mathf.Max(0.01f, telegraphConfig.Radius * 2f);
                    return new Vector3(diameter, 0.02f, diameter);
            }
        }

        public static bool IsValidTelegraphShape(float radius, float width, float length, float duration)
        {
            return radius >= 0f && width >= 0f && length >= 0f && duration >= 0f;
        }

        private void ApplyLineRendererShape(EnemyTelegraphType telegraphType)
        {
            switch (telegraphType)
            {
                case EnemyTelegraphType.Line:
                case EnemyTelegraphType.ChargePath:
                    ApplyLineShape();
                    break;
                case EnemyTelegraphType.Cone:
                    ApplyArcShape();
                    break;
                default:
                    ApplyCircleShape();
                    break;
            }
        }

        private void ApplyLineShape()
        {
            telegraphLineRenderer.loop = false;
            telegraphLineRenderer.positionCount = 2;
            telegraphLineRenderer.useWorldSpace = false;
            telegraphLineRenderer.widthMultiplier = 0.12f;
            telegraphLineRenderer.SetPosition(0, new Vector3(0f, 0f, 0f));
            telegraphLineRenderer.SetPosition(1, new Vector3(0f, 0f, 1f));
        }

        private void ApplyArcShape()
        {
            const int segmentCount = 24;
            const float arcDegrees = 130f;
            telegraphLineRenderer.loop = false;
            telegraphLineRenderer.positionCount = segmentCount + 1;
            telegraphLineRenderer.useWorldSpace = false;
            telegraphLineRenderer.widthMultiplier = 0.09f;

            var start = -arcDegrees * 0.5f;
            for (var i = 0; i <= segmentCount; i++)
            {
                var angle = (start + arcDegrees * (i / (float)segmentCount)) * Mathf.Deg2Rad;
                telegraphLineRenderer.SetPosition(i, new Vector3(Mathf.Sin(angle) * 0.5f, 0f, Mathf.Cos(angle) * 0.5f));
            }
        }

        private void ApplyCircleShape()
        {
            const int segmentCount = 48;
            telegraphLineRenderer.loop = true;
            telegraphLineRenderer.positionCount = segmentCount;
            telegraphLineRenderer.useWorldSpace = false;
            telegraphLineRenderer.widthMultiplier = 0.08f;

            for (var i = 0; i < segmentCount; i++)
            {
                var angle = Mathf.PI * 2f * (i / (float)segmentCount);
                telegraphLineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.5f, 0f, Mathf.Sin(angle) * 0.5f));
            }
        }
    }
}
