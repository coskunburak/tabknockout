using TapKnockout.Input;
using UnityEngine;

namespace TapKnockout.Visuals
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(74)]
    public sealed class PlayerVisibilityLightingController : MonoBehaviour
    {
        private const string MainAuraLightName = "MainAura_PointLight";
        private const string OuterFillLightName = "OuterFill_PointLight";
        private const string AimAccentLightName = "OptionalAimAccent_SpotLight";
        private const float DirectionEpsilon = 0.0001f;

        [Header("Profile")]
        [SerializeField] private PlayerVisibilityLightingProfile profile;

        [Header("References")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private MouseAimController aimController;
        [SerializeField] private Light mainAuraLight;
        [SerializeField] private Light outerFillLight;
        [SerializeField] private Light aimAccentLight;
        [SerializeField] private Transform aimAccentPivot;

        [Header("Runtime")]
        [SerializeField] private bool applyOnAwake = true;
        [SerializeField] private bool createMissingLightsOnAwake = true;
        [SerializeField] private bool useUnscaledTimeForVisualSmoothing;
        [SerializeField] private bool drawGizmos = true;

        private Vector3 velocity;
        private Vector3 lastAimDirection = Vector3.forward;
        private float currentAimAccentIntensity;
        private float combatIntensity;
        private float lowHealthPulse;
        private bool hasAimDirection;
        private bool warnedMissingProfile;

        public PlayerVisibilityLightingProfile Profile => profile;
        public Transform FollowTarget => followTarget;
        public MouseAimController AimController => aimController;
        public Light MainAuraLight => mainAuraLight;
        public Light OuterFillLight => outerFillLight;
        public Light AimAccentLight => aimAccentLight;
        public bool HasAimDirection => hasAimDirection;

        private void Reset()
        {
            followTarget = transform;
            aimController = GetComponentInParent<MouseAimController>();
            createMissingLightsOnAwake = true;
            applyOnAwake = true;
        }

        private void Awake()
        {
            ResolveReferences();
            if (createMissingLightsOnAwake)
            {
                EnsureLightReferences();
            }

            if (applyOnAwake)
            {
                ApplyProfile(profile);
            }
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (followTarget != null)
            {
                transform.position = followTarget.position;
                velocity = Vector3.zero;
            }

            ApplyProfile(profile);
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                ApplyProfile(profile);
            }
        }

        private void LateUpdate()
        {
            if (profile == null)
            {
                if (!warnedMissingProfile)
                {
                    Debug.LogWarning($"{nameof(PlayerVisibilityLightingController)} on {name} has no lighting profile.", this);
                    warnedMissingProfile = true;
                }

                SetLightEnabled(mainAuraLight, false);
                SetLightEnabled(outerFillLight, false);
                SetLightEnabled(aimAccentLight, false);
                return;
            }

            if (followTarget == null)
            {
                SetLightEnabled(mainAuraLight, false);
                SetLightEnabled(outerFillLight, false);
                SetLightEnabled(aimAccentLight, false);
                return;
            }

            var deltaTime = useUnscaledTimeForVisualSmoothing ? Time.unscaledDeltaTime : Time.deltaTime;
            FollowTargetPosition(deltaTime);
            UpdateMainAura(deltaTime);
            UpdateOuterFill();
            UpdateAimAccent(deltaTime);
        }

        public void SetTarget(Transform target)
        {
            followTarget = target;
            if (followTarget != null)
            {
                transform.position = followTarget.position;
                velocity = Vector3.zero;
                if (aimController == null)
                {
                    aimController = followTarget.GetComponent<MouseAimController>();
                }
            }
        }

        public void SetAimController(MouseAimController controller)
        {
            aimController = controller;
        }

        public void SetAimDirection(Vector3 worldDirection)
        {
            if (!TryNormalizePlanar(worldDirection, out var direction))
            {
                hasAimDirection = false;
                return;
            }

            lastAimDirection = direction;
            hasAimDirection = true;
        }

        public void SetCombatIntensity(float normalized)
        {
            combatIntensity = Mathf.Clamp01(normalized);
        }

        public void SetLowHealthPulse(float normalized)
        {
            lowHealthPulse = Mathf.Clamp01(normalized);
        }

        public void ApplyProfile(PlayerVisibilityLightingProfile value)
        {
            profile = value;
            warnedMissingProfile = false;
            EnsureLightReferences();

            if (profile == null)
            {
                return;
            }

            ConfigureMainAura();
            ConfigureOuterFill();
            ConfigureAimAccent();
        }

        private void ResolveReferences()
        {
            if (followTarget == null)
            {
                followTarget = transform;
            }

            if (aimController == null && followTarget != null)
            {
                aimController = followTarget.GetComponent<MouseAimController>();
            }
        }

        private void EnsureLightReferences()
        {
            if (!createMissingLightsOnAwake && mainAuraLight == null && outerFillLight == null && aimAccentLight == null)
            {
                return;
            }

            if (mainAuraLight == null)
            {
                mainAuraLight = ResolveOrCreateLight(MainAuraLightName, LightType.Point);
            }

            if (outerFillLight == null)
            {
                outerFillLight = ResolveOrCreateLight(OuterFillLightName, LightType.Point);
            }

            if (aimAccentPivot == null)
            {
                var pivot = transform.Find("AimAccentPivot");
                if (pivot == null)
                {
                    var pivotObject = new GameObject("AimAccentPivot");
                    pivotObject.transform.SetParent(transform, false);
                    pivot = pivotObject.transform;
                }

                aimAccentPivot = pivot;
            }

            if (aimAccentLight == null)
            {
                aimAccentLight = ResolveOrCreateLight(AimAccentLightName, LightType.Spot, aimAccentPivot);
            }
        }

        private Light ResolveOrCreateLight(string childName, LightType lightType, Transform parent = null)
        {
            var resolvedParent = parent != null ? parent : transform;
            var child = resolvedParent.Find(childName);
            if (child == null)
            {
                var lightObject = new GameObject(childName);
                lightObject.transform.SetParent(resolvedParent, false);
                child = lightObject.transform;
            }

            if (!child.TryGetComponent<Light>(out var light))
            {
                light = child.gameObject.AddComponent<Light>();
            }

            light.type = lightType;
            return light;
        }

        private void FollowTargetPosition(float deltaTime)
        {
            var desired = followTarget.position;
            var sharpness = profile != null ? profile.AuraFollowSharpness : 24f;
            var t = ResolveSharpnessT(sharpness, deltaTime);
            transform.position = t >= 1f ? desired : Vector3.SmoothDamp(transform.position, desired, ref velocity, ResolveSmoothTime(sharpness), Mathf.Infinity, Mathf.Max(0.0001f, deltaTime));
        }

        private void UpdateMainAura(float deltaTime)
        {
            if (mainAuraLight == null)
            {
                return;
            }

            if (!profile.EnableMainAura)
            {
                SetLightEnabled(mainAuraLight, false);
                return;
            }

            mainAuraLight.transform.localPosition = Vector3.up * profile.AuraHeightOffset;
            mainAuraLight.transform.localRotation = Quaternion.identity;
            mainAuraLight.type = LightType.Point;
            mainAuraLight.color = profile.AuraColor;
            mainAuraLight.range = profile.AuraRange;
            mainAuraLight.shadows = profile.AuraShadowMode;
            mainAuraLight.shadowStrength = profile.AuraShadowStrength;
            mainAuraLight.shadowBias = profile.AuraShadowBias;
            mainAuraLight.shadowNormalBias = profile.AuraShadowNormalBias;
            mainAuraLight.enabled = true;
            mainAuraLight.intensity = ResolveAuraIntensity(deltaTime);
        }

        private void UpdateOuterFill()
        {
            if (outerFillLight == null)
            {
                return;
            }

            if (!profile.EnableOuterFill)
            {
                SetLightEnabled(outerFillLight, false);
                return;
            }

            outerFillLight.transform.localPosition = Vector3.up * profile.OuterFillHeightOffset;
            outerFillLight.transform.localRotation = Quaternion.identity;
            outerFillLight.type = LightType.Point;
            outerFillLight.color = profile.OuterFillColor;
            outerFillLight.intensity = profile.OuterFillIntensity;
            outerFillLight.range = profile.OuterFillRange;
            outerFillLight.shadows = profile.OuterFillShadowMode;
            outerFillLight.enabled = profile.OuterFillIntensity > 0.005f;
        }

        private float ResolveAuraIntensity(float deltaTime)
        {
            var pulse = 1f;
            if (profile.EnableSubtlePulse)
            {
                pulse += Mathf.Sin(Time.time * Mathf.PI * 2f * profile.PulseSpeed) * profile.PulseAmplitude;
            }

            var boost = combatIntensity * profile.CombatIntensityBoost + lowHealthPulse * profile.LowHealthPulseBoost;
            return Mathf.Max(0f, profile.AuraIntensity * (pulse + boost));
        }

        private void UpdateAimAccent(float deltaTime)
        {
            if (aimAccentLight == null)
            {
                return;
            }

            var hasValidAim = TryResolveAimDirection(out var direction);
            var targetIntensity = profile.EnableAimAccent && hasValidAim
                ? Mathf.Min(profile.AimAccentIntensity, profile.AuraIntensity * profile.AimAccentMaxAuraIntensityFraction)
                : 0f;

            var t = ResolveSharpnessT(profile.AimAccentRotationSharpness, deltaTime);
            currentAimAccentIntensity = Mathf.Lerp(currentAimAccentIntensity, targetIntensity, t);
            aimAccentLight.enabled = currentAimAccentIntensity > 0.005f;
            aimAccentLight.intensity = currentAimAccentIntensity;
            if (!aimAccentLight.enabled)
            {
                return;
            }

            if (hasValidAim)
            {
                lastAimDirection = direction;
                hasAimDirection = true;
            }

            var pivot = aimAccentPivot != null ? aimAccentPivot : aimAccentLight.transform;
            pivot.localPosition = Vector3.up * profile.AimAccentHeightOffset + lastAimDirection * profile.AimAccentForwardOffset;
            var targetRotation = Quaternion.LookRotation(lastAimDirection + Vector3.down * 0.45f, Vector3.up);
            pivot.rotation = t >= 1f ? targetRotation : Quaternion.Slerp(pivot.rotation, targetRotation, t);
        }

        private void ConfigureMainAura()
        {
            if (mainAuraLight == null || profile == null)
            {
                return;
            }

            mainAuraLight.type = LightType.Point;
            mainAuraLight.color = profile.AuraColor;
            mainAuraLight.range = profile.AuraRange;
            mainAuraLight.shadows = profile.AuraShadowMode;
            mainAuraLight.shadowStrength = profile.AuraShadowStrength;
            mainAuraLight.shadowBias = profile.AuraShadowBias;
            mainAuraLight.shadowNormalBias = profile.AuraShadowNormalBias;
            mainAuraLight.enabled = profile.EnableMainAura;
        }

        private void ConfigureOuterFill()
        {
            if (outerFillLight == null || profile == null)
            {
                return;
            }

            outerFillLight.type = LightType.Point;
            outerFillLight.color = profile.OuterFillColor;
            outerFillLight.range = profile.OuterFillRange;
            outerFillLight.shadows = profile.OuterFillShadowMode;
            outerFillLight.enabled = profile.EnableOuterFill && profile.OuterFillIntensity > 0.005f;
            outerFillLight.intensity = outerFillLight.enabled ? profile.OuterFillIntensity : 0f;
        }

        private void ConfigureAimAccent()
        {
            if (aimAccentLight == null || profile == null)
            {
                return;
            }

            aimAccentLight.type = LightType.Spot;
            aimAccentLight.color = profile.AimAccentColor;
            aimAccentLight.range = profile.AimAccentRange;
            aimAccentLight.spotAngle = profile.AimAccentSpotAngle;
            aimAccentLight.shadows = profile.AimAccentShadowMode;
            aimAccentLight.enabled = profile.EnableAimAccent;
            aimAccentLight.intensity = profile.EnableAimAccent ? Mathf.Min(profile.AimAccentIntensity, profile.AuraIntensity * profile.AimAccentMaxAuraIntensityFraction) : 0f;
        }

        private bool TryResolveAimDirection(out Vector3 direction)
        {
            if (aimController != null && aimController.TryGetAimDirection(out var aimDirection))
            {
                return TryNormalizePlanar(aimDirection, out direction);
            }

            if (hasAimDirection && lastAimDirection.sqrMagnitude > DirectionEpsilon)
            {
                direction = lastAimDirection;
                return true;
            }

            direction = Vector3.forward;
            return false;
        }

        private static bool TryNormalizePlanar(Vector3 value, out Vector3 direction)
        {
            value.y = 0f;
            if (value.sqrMagnitude <= DirectionEpsilon)
            {
                direction = Vector3.forward;
                return false;
            }

            direction = value.normalized;
            return true;
        }

        private static void SetLightEnabled(Light light, bool enabled)
        {
            if (light != null)
            {
                light.enabled = enabled;
            }
        }

        private static float ResolveSharpnessT(float sharpness, float deltaTime)
        {
            if (sharpness <= 0f)
            {
                return 1f;
            }

            return 1f - Mathf.Exp(-sharpness * Mathf.Max(0f, deltaTime));
        }

        private static float ResolveSmoothTime(float sharpness)
        {
            return sharpness <= 0f ? 0f : Mathf.Clamp(1f / sharpness, 0.01f, 0.2f);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos || profile == null)
            {
                return;
            }

            Gizmos.color = new Color(profile.AuraColor.r, profile.AuraColor.g, profile.AuraColor.b, 0.35f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * profile.AuraHeightOffset, profile.AuraRange);

            if (profile.EnableOuterFill)
            {
                Gizmos.color = new Color(profile.OuterFillColor.r, profile.OuterFillColor.g, profile.OuterFillColor.b, 0.18f);
                Gizmos.DrawWireSphere(transform.position + Vector3.up * profile.OuterFillHeightOffset, profile.OuterFillRange);
            }

            if (profile.EnableAimAccent)
            {
                Gizmos.color = new Color(profile.AimAccentColor.r, profile.AimAccentColor.g, profile.AimAccentColor.b, 0.55f);
                Gizmos.DrawRay(transform.position + Vector3.up * profile.AimAccentHeightOffset, lastAimDirection * profile.AimAccentRange);
            }
        }
    }
}
