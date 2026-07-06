using TapKnockout.Combat;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Visuals
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(72)]
    public sealed class TapKnockoutPlayerGlow : MonoBehaviour
    {
        private enum GlowMeshKind
        {
            LanternField,
            ForwardLantern,
            GroundDisc,
            CoreDisc
        }

        private const int DiscSegments = 48;
        private const int ForwardLanternSegments = 28;
        private const string LanternFieldName = "PlayerLanternField";
        private const string ForwardLanternName = "PlayerForwardLantern";
        private const string GroundGlowName = "PlayerGroundGlow";
        private const string CoreGlowName = "PlayerCoreGlow";
        private const float DirectionEpsilon = 0.0001f;
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

        [SerializeField] private TapKnockoutPlayerLightRigConfig config;
        [SerializeField] private Transform target;
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private UnityEngine.Camera billboardCamera;
        [SerializeField] private MeshRenderer lanternFieldRenderer;
        [SerializeField] private MeshRenderer forwardLanternRenderer;
        [SerializeField] private MeshRenderer groundGlowRenderer;
        [SerializeField] private MeshRenderer coreGlowRenderer;
        [SerializeField] private bool listenToDashEvents = true;
        [SerializeField] private bool createMissingRenderersOnAwake = true;

        private Mesh lanternFieldMesh;
        private Mesh forwardLanternMesh;
        private Mesh groundMesh;
        private Mesh coreMesh;
        private Material lanternFieldMaterial;
        private Material forwardLanternMaterial;
        private Material groundMaterial;
        private Material coreMaterial;
        private MaterialPropertyBlock lanternFieldBlock;
        private MaterialPropertyBlock forwardLanternBlock;
        private MaterialPropertyBlock groundBlock;
        private MaterialPropertyBlock coreBlock;
        private Vector3 lastTargetPosition;
        private Vector3 smoothedForwardDirection = Vector3.forward;
        private Vector3 dashDirection = Vector3.forward;
        private float dashPulseRemaining;
        private float dashPulseDuration;
        private bool hasLastTargetPosition;
        private bool hasActiveDirection;

        public TapKnockoutPlayerLightRigConfig Config => config;
        public Transform Target => target;
        public PlayerMovementController MovementController => movementController;
        public MeshRenderer LanternFieldRenderer => lanternFieldRenderer;
        public MeshRenderer ForwardLanternRenderer => forwardLanternRenderer;
        public MeshRenderer GroundGlowRenderer => groundGlowRenderer;
        public MeshRenderer CoreGlowRenderer => coreGlowRenderer;

        private void Reset()
        {
            listenToDashEvents = true;
            createMissingRenderersOnAwake = true;
        }

        private void Awake()
        {
            ResolveMovementController();

            if (createMissingRenderersOnAwake)
            {
                EnsureRenderers();
            }

            ApplyStaticSettings();
        }

        private void OnEnable()
        {
            if (target != null)
            {
                lastTargetPosition = target.position;
                hasLastTargetPosition = true;
                transform.position = target.position;
            }

            if (listenToDashEvents)
            {
                DashEvents.OnDashStarted -= HandleDashStarted;
                DashEvents.OnDashStarted += HandleDashStarted;
            }
        }

        private void OnDisable()
        {
            DashEvents.OnDashStarted -= HandleDashStarted;
        }

        private void OnDestroy()
        {
            DestroyRuntimeObject(lanternFieldMesh);
            DestroyRuntimeObject(forwardLanternMesh);
            DestroyRuntimeObject(groundMesh);
            DestroyRuntimeObject(coreMesh);
            DestroyRuntimeObject(lanternFieldMaterial);
            DestroyRuntimeObject(forwardLanternMaterial);
            DestroyRuntimeObject(groundMaterial);
            DestroyRuntimeObject(coreMaterial);
        }

        private void LateUpdate()
        {
            if (target != null)
            {
                transform.position = target.position;
            }

            var deltaTime = Time.deltaTime;
            UpdateDirection(deltaTime);
            ApplyStaticSettings();
            UpdatePulse();
            FaceCoreToCamera();
        }

        public void Configure(TapKnockoutPlayerLightRigConfig rigConfig, Transform followTarget, UnityEngine.Camera camera)
        {
            Configure(rigConfig, followTarget, camera, followTarget != null ? followTarget.GetComponent<PlayerMovementController>() : null);
        }

        public void Configure(
            TapKnockoutPlayerLightRigConfig rigConfig,
            Transform followTarget,
            UnityEngine.Camera camera,
            PlayerMovementController movement)
        {
            config = rigConfig;
            target = followTarget;
            movementController = movement != null ? movement : followTarget != null ? followTarget.GetComponent<PlayerMovementController>() : null;
            billboardCamera = camera;
            hasLastTargetPosition = false;
            if (target != null)
            {
                lastTargetPosition = target.position;
                hasLastTargetPosition = true;
                transform.position = target.position;
            }

            if (Application.isPlaying)
            {
                EnsureRenderers();
                ApplyStaticSettings();
            }
        }

        private void EnsureRenderers()
        {
            if (lanternFieldRenderer == null)
            {
                lanternFieldRenderer = ResolveRenderer(LanternFieldName, GlowMeshKind.LanternField);
            }

            if (forwardLanternRenderer == null)
            {
                forwardLanternRenderer = ResolveRenderer(ForwardLanternName, GlowMeshKind.ForwardLantern);
            }

            if (groundGlowRenderer == null)
            {
                groundGlowRenderer = ResolveRenderer(GroundGlowName, GlowMeshKind.GroundDisc);
            }

            if (coreGlowRenderer == null)
            {
                coreGlowRenderer = ResolveRenderer(CoreGlowName, GlowMeshKind.CoreDisc);
            }

            lanternFieldMaterial ??= CreateGlowMaterial("TapKnockout_PlayerLanternField_Runtime", 3075, false);
            forwardLanternMaterial ??= CreateGlowMaterial("TapKnockout_PlayerForwardLantern_Runtime", 3080, false);
            groundMaterial ??= CreateGlowMaterial("TapKnockout_PlayerGroundGlow_Runtime", 3100, false);
            coreMaterial ??= CreateGlowMaterial("TapKnockout_PlayerCoreGlow_Runtime", 3150, true);

            AssignMaterial(lanternFieldRenderer, lanternFieldMaterial);
            AssignMaterial(forwardLanternRenderer, forwardLanternMaterial);
            AssignMaterial(groundGlowRenderer, groundMaterial);
            AssignMaterial(coreGlowRenderer, coreMaterial);

            lanternFieldBlock ??= new MaterialPropertyBlock();
            forwardLanternBlock ??= new MaterialPropertyBlock();
            groundBlock ??= new MaterialPropertyBlock();
            coreBlock ??= new MaterialPropertyBlock();
        }

        private MeshRenderer ResolveRenderer(string childName, GlowMeshKind meshKind)
        {
            var child = transform.Find(childName);
            if (child == null)
            {
                var childObject = new GameObject(childName);
                childObject.transform.SetParent(transform, false);
                child = childObject.transform;
            }

            var meshFilter = child.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = child.gameObject.AddComponent<MeshFilter>();
            }

            var renderer = child.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = child.gameObject.AddComponent<MeshRenderer>();
            }

            meshFilter.sharedMesh = ResolveMesh(meshKind);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            return renderer;
        }

        private Mesh ResolveMesh(GlowMeshKind meshKind)
        {
            switch (meshKind)
            {
                case GlowMeshKind.LanternField:
                    lanternFieldMesh ??= BuildGroundDiscMesh("TapKnockout_PlayerLanternFieldDisc_Runtime");
                    return lanternFieldMesh;
                case GlowMeshKind.ForwardLantern:
                    forwardLanternMesh ??= BuildForwardLanternMesh();
                    return forwardLanternMesh;
                case GlowMeshKind.GroundDisc:
                    groundMesh ??= BuildGroundDiscMesh("TapKnockout_PlayerGroundGlowDisc_Runtime");
                    return groundMesh;
                case GlowMeshKind.CoreDisc:
                    coreMesh ??= BuildCoreDiscMesh();
                    return coreMesh;
                default:
                    groundMesh ??= BuildGroundDiscMesh("TapKnockout_PlayerGroundGlowDisc_Runtime");
                    return groundMesh;
            }
        }

        private void ApplyStaticSettings()
        {
            var glowEnabled = config == null || config.VisibleGlowEnabled;
            var lanternEnabled = glowEnabled && (config == null || config.LanternFieldEnabled);
            var forwardEnabled = glowEnabled && (config == null || config.ForwardLanternEnabled);

            SetRendererEnabled(lanternFieldRenderer, lanternEnabled);
            SetRendererEnabled(forwardLanternRenderer, forwardEnabled);
            SetRendererEnabled(groundGlowRenderer, glowEnabled);
            SetRendererEnabled(coreGlowRenderer, glowEnabled);

            if (lanternFieldRenderer != null)
            {
                lanternFieldRenderer.transform.localPosition = config != null
                    ? config.LanternFieldOffset
                    : new Vector3(0f, 0.045f, 0f);
                var radius = config != null ? config.LanternFieldRadius : 4.6f;
                lanternFieldRenderer.transform.localScale = new Vector3(radius, 1f, radius);
            }

            if (forwardLanternRenderer != null)
            {
                var direction = ResolveForwardDirection();
                forwardLanternRenderer.transform.localPosition = ResolveDirectionalOffset(
                    config != null ? config.ForwardLanternOffset : new Vector3(0f, 0.055f, 0.25f),
                    direction);
                forwardLanternRenderer.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                var width = config != null ? config.ForwardLanternWidth : 3.4f;
                var range = config != null ? config.ForwardLanternRange : 5.2f;
                forwardLanternRenderer.transform.localScale = new Vector3(width, 1f, range);
            }

            if (groundGlowRenderer != null)
            {
                groundGlowRenderer.transform.localPosition = Vector3.up * 0.06f;
                var groundRadius = config != null ? config.GroundGlowRadius : 1.45f;
                groundGlowRenderer.transform.localScale = new Vector3(groundRadius, 1f, groundRadius);
            }

            if (coreGlowRenderer != null)
            {
                coreGlowRenderer.transform.localPosition = config != null ? config.CoreGlowOffset : new Vector3(0f, 0.92f, 0f);
                var coreSize = config != null ? config.CoreGlowSize : new Vector2(0.92f, 1.45f);
                coreGlowRenderer.transform.localScale = new Vector3(coreSize.x, coreSize.y, 1f);
            }
        }

        private void UpdatePulse()
        {
            var pulseAmplitude = config != null ? config.GlowPulseAmplitude : 0.045f;
            var pulseFrequency = config != null ? config.GlowPulseFrequency : 0.85f;
            var pulse = 1f + Mathf.Sin(Time.time * pulseFrequency * Mathf.PI * 2f) * pulseAmplitude;
            var dashBoost = dashPulseDuration > 0f ? 1f + Mathf.Clamp01(dashPulseRemaining / dashPulseDuration) * 0.12f : 1f;
            var forwardAlphaMultiplier = hasActiveDirection || dashPulseRemaining > 0f
                ? 1f
                : config != null ? config.ForwardLanternIdleAlphaMultiplier : 0.35f;

            ApplyColor(
                lanternFieldRenderer,
                lanternFieldBlock,
                ScaleAlpha(config != null ? config.LanternFieldColor : new Color(0.66f, 0.82f, 0.84f, 0.08f), pulse * dashBoost));
            ApplyColor(
                forwardLanternRenderer,
                forwardLanternBlock,
                ScaleAlpha(config != null ? config.ForwardLanternColor : new Color(0.78f, 1f, 0.66f, 0.12f), pulse * dashBoost * forwardAlphaMultiplier));
            ApplyColor(
                groundGlowRenderer,
                groundBlock,
                ScaleAlpha(config != null ? config.GroundGlowColor : new Color(0.74f, 0.92f, 0.9f, 0.18f), pulse * dashBoost));
            ApplyColor(
                coreGlowRenderer,
                coreBlock,
                ScaleAlpha(config != null ? config.CoreGlowColor : new Color(0.86f, 0.96f, 0.92f, 0.48f), pulse * dashBoost));
        }

        private void UpdateDirection(float deltaTime)
        {
            if (target == null)
            {
                hasActiveDirection = false;
                UpdateDashPulse(deltaTime);
                return;
            }

            ResolveMovementController();
            var targetDelta = ResolveTargetDelta();
            hasActiveDirection = TryResolvePlanarDirection(targetDelta, out var desiredDirection);
            if (hasActiveDirection)
            {
                var t = ResolveSharpnessT(config != null ? config.ForwardDirectionSharpness : 18f, deltaTime);
                smoothedForwardDirection = Vector3.Slerp(smoothedForwardDirection, desiredDirection, t);
                if (smoothedForwardDirection.sqrMagnitude <= DirectionEpsilon)
                {
                    smoothedForwardDirection = desiredDirection;
                }
            }

            UpdateDashPulse(deltaTime);
        }

        private Vector3 ResolveTargetDelta()
        {
            var currentPosition = target.position;
            if (!hasLastTargetPosition)
            {
                lastTargetPosition = currentPosition;
                hasLastTargetPosition = true;
                return Vector3.zero;
            }

            var delta = currentPosition - lastTargetPosition;
            lastTargetPosition = currentPosition;
            delta.y = 0f;
            return delta;
        }

        private bool TryResolvePlanarDirection(Vector3 targetDelta, out Vector3 direction)
        {
            if (dashPulseRemaining > 0f && dashDirection.sqrMagnitude > DirectionEpsilon)
            {
                direction = dashDirection;
                return true;
            }

            var deadZoneSqr = ResolveDirectionDeadZoneSqr();
            if (movementController != null && movementController.CurrentMoveDirection.sqrMagnitude > deadZoneSqr)
            {
                direction = PlanarNormalized(movementController.CurrentMoveDirection);
                return true;
            }

            if (targetDelta.sqrMagnitude > deadZoneSqr)
            {
                direction = PlanarNormalized(targetDelta);
                return true;
            }

            direction = ResolveForwardDirection();
            return false;
        }

        private void UpdateDashPulse(float deltaTime)
        {
            if (dashPulseRemaining <= 0f)
            {
                dashPulseRemaining = 0f;
                return;
            }

            dashPulseRemaining = Mathf.Max(0f, dashPulseRemaining - Mathf.Max(0f, deltaTime));
        }

        private void RequestDashPulse(Vector3 direction, float duration)
        {
            if (config != null && !config.DashPulseEnabled)
            {
                return;
            }

            dashDirection = direction.sqrMagnitude > DirectionEpsilon ? PlanarNormalized(direction) : ResolveForwardDirection();
            dashPulseDuration = Mathf.Max(0.03f, Mathf.Min(Mathf.Max(config != null ? config.DashPulseDuration : 0.18f, duration), 0.4f));
            dashPulseRemaining = dashPulseDuration;
        }

        private void FaceCoreToCamera()
        {
            if (coreGlowRenderer == null)
            {
                return;
            }

            var cameraTransform = billboardCamera != null ? billboardCamera.transform : UnityEngine.Camera.main != null ? UnityEngine.Camera.main.transform : null;
            if (cameraTransform == null)
            {
                return;
            }

            coreGlowRenderer.transform.rotation = Quaternion.LookRotation(cameraTransform.forward, cameraTransform.up);
        }

        private void HandleDashStarted(DashStartedEventArgs eventArgs)
        {
            if (!MatchesTarget(eventArgs.Source))
            {
                return;
            }

            RequestDashPulse(eventArgs.Direction, eventArgs.Duration);
        }

        private bool MatchesTarget(GameObject source)
        {
            if (target == null || source == null)
            {
                return false;
            }

            var sourceTransform = source.transform;
            return sourceTransform == target ||
                sourceTransform.IsChildOf(target) ||
                target.IsChildOf(sourceTransform);
        }

        private void ResolveMovementController()
        {
            if (movementController == null && target != null)
            {
                movementController = target.GetComponent<PlayerMovementController>();
            }
        }

        private float ResolveDirectionDeadZoneSqr()
        {
            var deadZone = config != null ? config.MovementDirectionDeadZone : 0.05f;
            return deadZone * deadZone;
        }

        private Vector3 ResolveForwardDirection()
        {
            return smoothedForwardDirection.sqrMagnitude > DirectionEpsilon ? PlanarNormalized(smoothedForwardDirection) : Vector3.forward;
        }

        private static Mesh BuildGroundDiscMesh(string meshName)
        {
            var mesh = new Mesh { name = meshName };
            var vertices = new Vector3[DiscSegments * 2 + 3];
            var colors = new Color[vertices.Length];
            var triangles = new int[DiscSegments * 9];

            vertices[0] = Vector3.zero;
            colors[0] = new Color(1f, 1f, 1f, 0.42f);
            for (var i = 0; i <= DiscSegments; i++)
            {
                var angle = i / (float)DiscSegments * Mathf.PI * 2f;
                var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                var irregularRadius = 1f +
                    Mathf.Sin(angle * 3.1f + 0.73f) * 0.075f +
                    Mathf.Sin(angle * 7.7f + 1.91f) * 0.045f;
                var innerIndex = i + 1;
                var outerIndex = i + DiscSegments + 2;
                vertices[innerIndex] = direction * 0.44f;
                vertices[outerIndex] = direction * irregularRadius;
                colors[innerIndex] = new Color(1f, 1f, 1f, 0.2f);
                colors[outerIndex] = new Color(1f, 1f, 1f, 0f);
            }

            for (var i = 0; i < DiscSegments; i++)
            {
                var triangle = i * 9;
                var innerCurrent = i + 1;
                var innerNext = i + 2;
                var outerCurrent = i + DiscSegments + 2;
                var outerNext = i + DiscSegments + 3;

                triangles[triangle] = 0;
                triangles[triangle + 1] = innerCurrent;
                triangles[triangle + 2] = innerNext;
                triangles[triangle + 3] = innerCurrent;
                triangles[triangle + 4] = outerCurrent;
                triangles[triangle + 5] = outerNext;
                triangles[triangle + 6] = innerCurrent;
                triangles[triangle + 7] = outerNext;
                triangles[triangle + 8] = innerNext;
            }

            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildForwardLanternMesh()
        {
            var mesh = new Mesh { name = "TapKnockout_PlayerForwardLanternFan_Runtime" };
            var vertices = new Vector3[ForwardLanternSegments + 2];
            var colors = new Color[vertices.Length];
            var triangles = new int[ForwardLanternSegments * 3];
            var halfAngle = 38f * Mathf.Deg2Rad;

            vertices[0] = new Vector3(0f, 0f, -0.04f);
            colors[0] = Color.white;
            for (var i = 0; i <= ForwardLanternSegments; i++)
            {
                var t = i / (float)ForwardLanternSegments;
                var angle = Mathf.Lerp(-halfAngle, halfAngle, t);
                vertices[i + 1] = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                colors[i + 1] = new Color(1f, 1f, 1f, 0f);
            }

            for (var i = 0; i < ForwardLanternSegments; i++)
            {
                var triangle = i * 3;
                triangles[triangle] = 0;
                triangles[triangle + 1] = i + 1;
                triangles[triangle + 2] = i + 2;
            }

            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildCoreDiscMesh()
        {
            var mesh = new Mesh { name = "TapKnockout_PlayerCoreGlowDisc_Runtime" };
            var vertices = new Vector3[DiscSegments + 2];
            var colors = new Color[vertices.Length];
            var triangles = new int[DiscSegments * 3];

            vertices[0] = Vector3.zero;
            colors[0] = Color.white;
            for (var i = 0; i <= DiscSegments; i++)
            {
                var angle = i / (float)DiscSegments * Mathf.PI * 2f;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                colors[i + 1] = new Color(1f, 1f, 1f, 0f);
            }

            for (var i = 0; i < DiscSegments; i++)
            {
                var triangle = i * 3;
                triangles[triangle] = 0;
                triangles[triangle + 1] = i + 1;
                triangles[triangle + 2] = i + 2;
            }

            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Material CreateGlowMaterial(string materialName, int renderQueue, bool renderOnTop)
        {
            var shader = Shader.Find("Tap Knockout/Visuals/Vertex Color Additive")
                ?? Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Unlit/Transparent");
            var material = new Material(shader)
            {
                name = materialName,
                renderQueue = renderQueue
            };
            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 0f);
            }

            if (material.HasProperty("_ZTest"))
            {
                material.SetFloat("_ZTest", (float)(renderOnTop
                    ? UnityEngine.Rendering.CompareFunction.Always
                    : UnityEngine.Rendering.CompareFunction.LessEqual));
            }

            return material;
        }

        private static void AssignMaterial(MeshRenderer renderer, Material material)
        {
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static void ApplyColor(Renderer renderer, MaterialPropertyBlock block, Color value)
        {
            if (renderer == null || block == null)
            {
                return;
            }

            renderer.GetPropertyBlock(block);
            block.SetColor(ColorProperty, value);
            block.SetColor(BaseColorProperty, value);
            renderer.SetPropertyBlock(block);
        }

        private static Color ScaleAlpha(Color value, float multiplier)
        {
            value.a = Mathf.Clamp01(value.a * multiplier);
            return value;
        }

        private static Vector3 PlanarNormalized(Vector3 value)
        {
            value.y = 0f;
            return value.sqrMagnitude > DirectionEpsilon ? value.normalized : Vector3.forward;
        }

        private static Vector3 ResolveDirectionalOffset(Vector3 offset, Vector3 planarDirection)
        {
            var forward = PlanarNormalized(planarDirection);
            var right = new Vector3(forward.z, 0f, -forward.x);
            return right * offset.x + Vector3.up * offset.y + forward * offset.z;
        }

        private static float ResolveSharpnessT(float sharpness, float deltaTime)
        {
            if (sharpness <= 0f)
            {
                return 1f;
            }

            return 1f - Mathf.Exp(-sharpness * Mathf.Max(0f, deltaTime));
        }

        private static void SetRendererEnabled(Renderer renderer, bool enabled)
        {
            if (renderer != null)
            {
                renderer.enabled = enabled;
            }
        }

        private static void DestroyRuntimeObject(Object value)
        {
            if (value == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(value);
            }
            else
            {
                DestroyImmediate(value);
            }
        }
    }
}
