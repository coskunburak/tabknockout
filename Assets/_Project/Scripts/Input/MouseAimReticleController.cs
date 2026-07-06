using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Input
{
    [DefaultExecutionOrder(90)]
    [DisallowMultipleComponent]
    public sealed class MouseAimReticleController : MonoBehaviour, IReticlePulseTarget
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private const int DefaultReticleSegments = 64;
        private const float MinimumGroundClearance = 0.1f;

        [Header("References")]
        [SerializeField] private MouseAimController aimController;
        [SerializeField] private DesktopInputReader inputReader;
        [SerializeField] private GameObject reticlePrefab;

        [Header("Visibility")]
        [SerializeField] private bool reticleEnabled = true;
        [SerializeField] private bool allowRuntimeFallback = true;
        [SerializeField] private bool showReticleOnlyDuringGameplay = true;
        [SerializeField] private bool showReticleOnlyWhileAimingOrFiring;
        [SerializeField] private ReticleInvalidAimBehavior invalidAimBehavior = ReticleInvalidAimBehavior.ShowAtFallbackPoint;

        [Header("Presentation")]
        [SerializeField, Min(0.01f)] private float reticleScale = 1f;
        [SerializeField] private float yOffset = 0.18f;
        [SerializeField, Min(0f)] private float smoothTime;
        [SerializeField] private AimReticleRotationMode rotationMode = AimReticleRotationMode.WorldFlat;
        [SerializeField] private Color validAimColor = new Color(0.24f, 0.9f, 1f, 0.9f);
        [SerializeField] private Color invalidAimColor = new Color(1f, 0.35f, 0.2f, 0.8f);
        [SerializeField, Range(0.002f, 0.08f)] private float lineWidth = 0.018f;
        [SerializeField, Min(0.05f)] private float ringRadius = 0.45f;
        [SerializeField, Min(0.05f)] private float crosshairExtent = 0.68f;
        [SerializeField, Range(0f, 0.4f)] private float crosshairGap = 0.24f;
        [SerializeField, Range(0f, 0.75f)] private float pulseScaleAdd = 0.16f;
        [SerializeField, Range(0.01f, 0.35f)] private float pulseDuration = 0.12f;
        [SerializeField] private int reticleLayer = 2;

        [Header("Cursor")]
        [SerializeField] private bool hideSystemCursorDuringGameplay = true;
        [SerializeField] private bool restoreCursorOnDisable = true;

        private GameObject activeReticle;
        private Transform activeReticleTransform;
        private LineRenderer ringLine;
        private LineRenderer horizontalLine;
        private LineRenderer verticalLine;
        private Renderer[] reticleRenderers;
        private Material runtimeFallbackMaterial;
        private MaterialPropertyBlock materialPropertyBlock;
        private Vector3 smoothVelocity;
        private Vector3 lastValidAimPoint;
        private Color lastAppliedColor = Color.clear;
        private float pulseRemaining;
        private float activePulseScaleAdd;
        private float activePulseDuration;
        private bool ownerAlive = true;
        private bool gameplayBlocked;
        private bool primaryFireActive;
        private bool hasLastValidAimPoint;
        private bool warningLogged;
        private bool cursorHideApplied;

        public bool IsReticleVisible => activeReticle != null && activeReticle.activeSelf;
        public bool FallbackReticleEnabled => allowRuntimeFallback;
        public int ReticleLayer => reticleLayer;
        public GameObject ReticlePrefab => reticlePrefab;

        private void Reset()
        {
            aimController = GetComponent<MouseAimController>();
            inputReader = GetComponent<DesktopInputReader>();
        }

        private void Awake()
        {
            ResolveReferences();
            EnsureReticleInstance();
        }

        private void OnEnable()
        {
            ResolveReferences();
            EnsureReticleInstance();
            UpdateReticle(true);
        }

        private void OnDisable()
        {
            SetReticleVisible(false);

            if (restoreCursorOnDisable)
            {
                RestoreCursor();
            }
        }

        private void OnDestroy()
        {
            DestroyActiveReticle();

            if (runtimeFallbackMaterial != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(runtimeFallbackMaterial);
                }
                else
                {
                    DestroyImmediate(runtimeFallbackMaterial);
                }

                runtimeFallbackMaterial = null;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                RestoreCursor();
            }
        }

        private void OnValidate()
        {
            reticleScale = Mathf.Max(0.01f, reticleScale);
            yOffset = Mathf.Max(MinimumGroundClearance, yOffset);
            smoothTime = Mathf.Max(0f, smoothTime);
            lineWidth = Mathf.Clamp(lineWidth, 0.002f, 0.08f);
            ringRadius = Mathf.Max(0.05f, ringRadius);
            crosshairExtent = Mathf.Max(ringRadius, crosshairExtent);
            crosshairGap = Mathf.Clamp(crosshairGap, 0f, crosshairExtent - 0.01f);
            pulseScaleAdd = Mathf.Clamp(pulseScaleAdd, 0f, 0.75f);
            pulseDuration = Mathf.Clamp(pulseDuration, 0.01f, 0.35f);
            reticleLayer = Mathf.Clamp(reticleLayer, 0, 31);
        }

        private void LateUpdate()
        {
            UpdateReticle(false);
        }

        public void SetAimController(MouseAimController controller)
        {
            aimController = controller;
        }

        public void SetInputReader(DesktopInputReader reader)
        {
            inputReader = reader;
        }

        public void SetOwnerAlive(bool isAlive)
        {
            ownerAlive = isAlive;
        }

        public void SetGameplayBlocked(bool blocked)
        {
            gameplayBlocked = blocked;
        }

        public void SetPrimaryFireActive(bool active)
        {
            primaryFireActive = active;
        }

        public void Configure(
            bool enabled,
            float scale,
            float offset,
            float smoothing,
            bool hideCursorDuringGameplay,
            bool onlyDuringGameplay,
            bool onlyWhileAimingOrFiring,
            ReticleInvalidAimBehavior invalidBehavior)
        {
            reticleEnabled = enabled;
            reticleScale = Mathf.Max(0.01f, scale);
            yOffset = Mathf.Max(MinimumGroundClearance, offset);
            smoothTime = Mathf.Max(0f, smoothing);
            hideSystemCursorDuringGameplay = hideCursorDuringGameplay;
            showReticleOnlyDuringGameplay = onlyDuringGameplay;
            showReticleOnlyWhileAimingOrFiring = onlyWhileAimingOrFiring;
            invalidAimBehavior = invalidBehavior;
        }

        public void Pulse()
        {
            Pulse(pulseScaleAdd, pulseDuration);
        }

        public void Pulse(float scaleAdd, float duration)
        {
            var resolvedDuration = Mathf.Clamp(duration > 0f ? duration : pulseDuration, 0.01f, 0.35f);
            activePulseScaleAdd = Mathf.Max(activePulseScaleAdd, Mathf.Clamp(scaleAdd, 0f, 0.75f));
            activePulseDuration = Mathf.Max(activePulseDuration, resolvedDuration);
            pulseRemaining = Mathf.Max(pulseRemaining, activePulseDuration);
        }

        private void UpdateReticle(bool snap)
        {
            ResolveReferences();
            EnsureReticleInstance();

            var shouldShow = TryResolveReticlePoint(out var reticlePoint, out var hasValidAimPoint);
            SetReticleVisible(shouldShow);
            ApplyCursorVisibility(shouldShow);

            if (!shouldShow || activeReticleTransform == null)
            {
                TickPulse();
                return;
            }

            reticlePoint.y += Mathf.Max(MinimumGroundClearance, yOffset);
            activeReticleTransform.position = snap || smoothTime <= 0f
                ? reticlePoint
                : Vector3.SmoothDamp(
                    activeReticleTransform.position,
                    reticlePoint,
                    ref smoothVelocity,
                    smoothTime,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime);

            activeReticleTransform.rotation = ResolveReticleRotation();
            activeReticleTransform.localScale = Vector3.one * ResolveCurrentScale();
            ApplyReticleColor(hasValidAimPoint ? validAimColor : invalidAimColor);
            TickPulse();
        }

        private bool TryResolveReticlePoint(out Vector3 point, out bool hasValidAimPoint)
        {
            point = default;
            hasValidAimPoint = false;

            if (!reticleEnabled ||
                !ownerAlive ||
                activeReticle == null ||
                showReticleOnlyDuringGameplay && IsGameplayBlocked())
            {
                return false;
            }

            if (aimController == null)
            {
                return false;
            }

            hasValidAimPoint = aimController.TryGetAimPoint(out point);
            if (hasValidAimPoint)
            {
                lastValidAimPoint = point;
                hasLastValidAimPoint = true;
            }
            else
            {
                switch (invalidAimBehavior)
                {
                    case ReticleInvalidAimBehavior.Hide:
                        return false;
                    case ReticleInvalidAimBehavior.HoldLastValidPoint:
                        if (!hasLastValidAimPoint)
                        {
                            return false;
                        }

                        point = lastValidAimPoint;
                        break;
                    default:
                        break;
                }
            }

            if (showReticleOnlyWhileAimingOrFiring && !hasValidAimPoint && !IsPrimaryFireActive())
            {
                return false;
            }

            return true;
        }

        private void ResolveReferences()
        {
            if (aimController == null)
            {
                aimController = GetComponent<MouseAimController>();
            }

            if (inputReader == null)
            {
                inputReader = GetComponent<DesktopInputReader>();
            }
        }

        private void EnsureReticleInstance()
        {
            if (activeReticle != null)
            {
                return;
            }

            if (reticlePrefab != null)
            {
                activeReticle = Instantiate(reticlePrefab);
                activeReticle.name = reticlePrefab.name;
                activeReticleTransform = activeReticle.transform;
                DisableColliders(activeReticle);
                SetLayerRecursively(activeReticle, reticleLayer);
                CacheRenderers();
                return;
            }

            if (!allowRuntimeFallback)
            {
                return;
            }

            activeReticle = new GameObject("RuntimeMouseAimReticle");
            activeReticleTransform = activeReticle.transform;
            SetLayerRecursively(activeReticle, reticleLayer);
            CreateFallbackReticle(activeReticle.transform);
            CacheRenderers();

            if (!warningLogged)
            {
                warningLogged = true;
                Debug.LogWarning($"{nameof(MouseAimReticleController)} on {name} has no reticlePrefab assigned. Using runtime LineRenderer fallback.", this);
            }
        }

        private void CreateFallbackReticle(Transform parent)
        {
            runtimeFallbackMaterial = CreateFallbackMaterial();
            ringLine = CreateLine(parent, "Ring", true, DefaultReticleSegments + 1);
            horizontalLine = CreateLine(parent, "HorizontalCrosshair", false, 4);
            verticalLine = CreateLine(parent, "VerticalCrosshair", false, 4);
            BuildRing(ringLine);
            BuildCrosshair(horizontalLine, true);
            BuildCrosshair(verticalLine, false);
        }

        private LineRenderer CreateLine(Transform parent, string lineName, bool loop, int positions)
        {
            var lineObject = new GameObject(lineName);
            lineObject.transform.SetParent(parent, false);
            lineObject.layer = reticleLayer;
            var line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = loop;
            line.positionCount = positions;
            line.widthMultiplier = lineWidth;
            line.numCornerVertices = 2;
            line.numCapVertices = 2;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.sharedMaterial = runtimeFallbackMaterial;
            line.sortingOrder = 1000;
            return line;
        }

        private void BuildRing(LineRenderer line)
        {
            if (line == null)
            {
                return;
            }

            for (var i = 0; i <= DefaultReticleSegments; i++)
            {
                var normalized = i / (float)DefaultReticleSegments;
                var radians = normalized * Mathf.PI * 2f;
                line.SetPosition(i, new Vector3(Mathf.Cos(radians) * ringRadius, 0f, Mathf.Sin(radians) * ringRadius));
            }
        }

        private void BuildCrosshair(LineRenderer line, bool horizontal)
        {
            if (line == null)
            {
                return;
            }

            if (horizontal)
            {
                line.SetPosition(0, new Vector3(-crosshairExtent, 0f, 0f));
                line.SetPosition(1, new Vector3(-crosshairGap, 0f, 0f));
                line.SetPosition(2, new Vector3(crosshairGap, 0f, 0f));
                line.SetPosition(3, new Vector3(crosshairExtent, 0f, 0f));
            }
            else
            {
                line.SetPosition(0, new Vector3(0f, 0f, -crosshairExtent));
                line.SetPosition(1, new Vector3(0f, 0f, -crosshairGap));
                line.SetPosition(2, new Vector3(0f, 0f, crosshairGap));
                line.SetPosition(3, new Vector3(0f, 0f, crosshairExtent));
            }
        }

        private Material CreateFallbackMaterial()
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            if (shader == null)
            {
                return null;
            }

            var material = new Material(shader)
            {
                renderQueue = 5000
            };

            if (material.HasProperty("_ZWrite"))
            {
                material.SetInt("_ZWrite", 0);
            }

            if (material.HasProperty("_ZTest"))
            {
                material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            }

            if (material.HasProperty("_Cull"))
            {
                material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            }

            if (material.HasProperty("_SrcBlend"))
            {
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            }

            if (material.HasProperty("_DstBlend"))
            {
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }

            return material;
        }

        private Quaternion ResolveReticleRotation()
        {
            if (rotationMode != AimReticleRotationMode.CameraYaw || aimController == null || aimController.AimCamera == null)
            {
                return Quaternion.identity;
            }

            var cameraForward = aimController.AimCamera.transform.forward;
            cameraForward.y = 0f;
            return cameraForward.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(cameraForward.normalized, Vector3.up)
                : Quaternion.identity;
        }

        private float ResolveCurrentScale()
        {
            if (pulseRemaining <= 0f || pulseDuration <= 0f)
            {
                return reticleScale;
            }

            var duration = activePulseDuration > 0f ? activePulseDuration : pulseDuration;
            var scaleAdd = activePulseScaleAdd > 0f ? activePulseScaleAdd : pulseScaleAdd;
            var normalized = Mathf.Clamp01(pulseRemaining / duration);
            return reticleScale * (1f + scaleAdd * normalized);
        }

        private void TickPulse()
        {
            if (pulseRemaining > 0f)
            {
                pulseRemaining = Mathf.Max(0f, pulseRemaining - Time.unscaledDeltaTime);
                if (pulseRemaining <= 0f)
                {
                    activePulseScaleAdd = 0f;
                    activePulseDuration = 0f;
                }
            }
        }

        private bool IsGameplayBlocked()
        {
            return gameplayBlocked || Time.timeScale <= 0f;
        }

        private bool IsPrimaryFireActive()
        {
            return primaryFireActive ||
                inputReader != null &&
                (inputReader.PrimaryFireHeld || inputReader.PrimaryFirePressedThisFrame);
        }

        private void SetReticleVisible(bool visible)
        {
            if (activeReticle != null && activeReticle.activeSelf != visible)
            {
                activeReticle.SetActive(visible);
            }
        }

        private void ApplyReticleColor(Color color)
        {
            if (lastAppliedColor == color)
            {
                return;
            }

            lastAppliedColor = color;
            ApplyLineColor(ringLine, color);
            ApplyLineColor(horizontalLine, color);
            ApplyLineColor(verticalLine, color);

            if (reticleRenderers == null || reticleRenderers.Length == 0)
            {
                return;
            }

            materialPropertyBlock ??= new MaterialPropertyBlock();
            for (var i = 0; i < reticleRenderers.Length; i++)
            {
                var renderer = reticleRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(materialPropertyBlock);
                materialPropertyBlock.SetColor(BaseColorId, color);
                materialPropertyBlock.SetColor(ColorId, color);
                renderer.SetPropertyBlock(materialPropertyBlock);
            }
        }

        private static void ApplyLineColor(LineRenderer line, Color color)
        {
            if (line == null)
            {
                return;
            }

            line.startColor = color;
            line.endColor = color;
        }

        private void ApplyCursorVisibility(bool reticleVisible)
        {
            var shouldHide = hideSystemCursorDuringGameplay &&
                reticleVisible &&
                ownerAlive &&
                !IsGameplayBlocked();

            if (shouldHide)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = false;
                cursorHideApplied = true;
                return;
            }

            if (cursorHideApplied)
            {
                RestoreCursor();
            }
        }

        private void RestoreCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            cursorHideApplied = false;
        }

        private void DestroyActiveReticle()
        {
            if (activeReticle == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(activeReticle);
            }
            else
            {
                DestroyImmediate(activeReticle);
            }

            activeReticle = null;
            activeReticleTransform = null;
            ringLine = null;
            horizontalLine = null;
            verticalLine = null;
            reticleRenderers = null;
        }

        private void CacheRenderers()
        {
            reticleRenderers = activeReticle != null ? activeReticle.GetComponentsInChildren<Renderer>(true) : null;
            if (reticleRenderers != null)
            {
                for (var i = 0; i < reticleRenderers.Length; i++)
                {
                    var renderer = reticleRenderers[i];
                    if (renderer == null)
                    {
                        continue;
                    }

                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    renderer.sortingOrder = Mathf.Max(renderer.sortingOrder, 1000);
                }
            }

            lastAppliedColor = Color.clear;
        }

        private static void DisableColliders(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            var colliders = root.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = false;
                }
            }
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            if (root == null)
            {
                return;
            }

            root.layer = layer;
            var transform = root.transform;
            for (var i = 0; i < transform.childCount; i++)
            {
                SetLayerRecursively(transform.GetChild(i).gameObject, layer);
            }
        }
    }
}
