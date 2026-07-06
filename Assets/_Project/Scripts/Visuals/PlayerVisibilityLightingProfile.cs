using UnityEngine;

namespace TapKnockout.Visuals
{
    [CreateAssetMenu(
        fileName = "PlayerVisibilityLightingProfile",
        menuName = "Tap Knockout/Visuals/Player Visibility Lighting Profile")]
    public sealed class PlayerVisibilityLightingProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string profileId = "player_visibility_default";
        [SerializeField] private LightingQualityTier qualityTier = LightingQualityTier.Default;

        [Header("Main Aura")]
        [SerializeField] private bool enableMainAura = true;
        [SerializeField] private Color auraColor = new Color(0.78f, 0.92f, 0.9f, 1f);
        [SerializeField, Min(0f)] private float auraIntensity = 0.72f;
        [SerializeField, Min(0.1f)] private float auraRange = 4.8f;
        [SerializeField, Min(0f)] private float auraHeightOffset = 2.25f;
        [SerializeField, Min(0f)] private float auraFollowSharpness = 24f;
        [SerializeField] private LightShadows auraShadowMode = LightShadows.None;
        [SerializeField, Range(0f, 1f)] private float auraShadowStrength = 0.22f;
        [SerializeField, Range(0f, 2f)] private float auraShadowBias = 0.08f;
        [SerializeField, Range(0f, 3f)] private float auraShadowNormalBias = 0.35f;

        [Header("Outer Fill")]
        [SerializeField] private bool enableOuterFill = true;
        [SerializeField] private Color outerFillColor = new Color(0.48f, 0.62f, 0.68f, 1f);
        [SerializeField, Min(0f)] private float outerFillIntensity = 0.16f;
        [SerializeField, Min(0.1f)] private float outerFillRange = 10.5f;
        [SerializeField, Min(0f)] private float outerFillHeightOffset = 2.9f;
        [SerializeField] private LightShadows outerFillShadowMode = LightShadows.None;

        [Header("Aim Accent")]
        [SerializeField] private bool enableAimAccent;
        [SerializeField] private Color aimAccentColor = new Color(0.74f, 0.9f, 0.88f, 1f);
        [SerializeField, Min(0f)] private float aimAccentIntensity = 0.06f;
        [SerializeField, Min(0.1f)] private float aimAccentRange = 4.5f;
        [SerializeField, Range(20f, 120f)] private float aimAccentSpotAngle = 82f;
        [SerializeField, Min(0f)] private float aimAccentHeightOffset = 2f;
        [SerializeField, Min(0f)] private float aimAccentForwardOffset = 1.25f;
        [SerializeField, Min(0f)] private float aimAccentRotationSharpness = 12f;
        [SerializeField] private LightShadows aimAccentShadowMode = LightShadows.None;
        [SerializeField, Range(0f, 1f)] private float aimAccentMaxAuraIntensityFraction = 0.12f;

        [Header("Pulse")]
        [SerializeField] private bool enableSubtlePulse = true;
        [SerializeField, Range(0f, 0.2f)] private float pulseAmplitude = 0.018f;
        [SerializeField, Range(0.05f, 4f)] private float pulseSpeed = 0.9f;
        [SerializeField, Range(0f, 0.35f)] private float combatIntensityBoost = 0.04f;
        [SerializeField, Range(0f, 0.35f)] private float lowHealthPulseBoost = 0.04f;

        public string ProfileId => profileId;
        public LightingQualityTier QualityTier => qualityTier;
        public bool EnableMainAura => enableMainAura;
        public Color AuraColor => auraColor;
        public float AuraIntensity => auraIntensity;
        public float AuraRange => auraRange;
        public float AuraHeightOffset => auraHeightOffset;
        public float AuraFollowSharpness => auraFollowSharpness;
        public LightShadows AuraShadowMode => auraShadowMode;
        public float AuraShadowStrength => auraShadowStrength;
        public float AuraShadowBias => auraShadowBias;
        public float AuraShadowNormalBias => auraShadowNormalBias;
        public bool EnableOuterFill => enableOuterFill;
        public Color OuterFillColor => outerFillColor;
        public float OuterFillIntensity => outerFillIntensity;
        public float OuterFillRange => outerFillRange;
        public float OuterFillHeightOffset => outerFillHeightOffset;
        public LightShadows OuterFillShadowMode => outerFillShadowMode;
        public bool EnableAimAccent => enableAimAccent;
        public Color AimAccentColor => aimAccentColor;
        public float AimAccentIntensity => aimAccentIntensity;
        public float AimAccentRange => aimAccentRange;
        public float AimAccentSpotAngle => aimAccentSpotAngle;
        public float AimAccentHeightOffset => aimAccentHeightOffset;
        public float AimAccentForwardOffset => aimAccentForwardOffset;
        public float AimAccentRotationSharpness => aimAccentRotationSharpness;
        public LightShadows AimAccentShadowMode => aimAccentShadowMode;
        public float AimAccentMaxAuraIntensityFraction => aimAccentMaxAuraIntensityFraction;
        public bool EnableSubtlePulse => enableSubtlePulse;
        public float PulseAmplitude => pulseAmplitude;
        public float PulseSpeed => pulseSpeed;
        public float CombatIntensityBoost => combatIntensityBoost;
        public float LowHealthPulseBoost => lowHealthPulseBoost;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(profileId))
            {
                profileId = "player_visibility_default";
            }

            auraIntensity = Mathf.Clamp(auraIntensity, 0f, 3f);
            auraRange = Mathf.Clamp(auraRange, 0.1f, 14f);
            auraHeightOffset = Mathf.Clamp(auraHeightOffset, 0f, 6f);
            auraFollowSharpness = Mathf.Max(0f, auraFollowSharpness);
            auraShadowStrength = Mathf.Clamp01(auraShadowStrength);
            auraShadowBias = Mathf.Clamp(auraShadowBias, 0f, 2f);
            auraShadowNormalBias = Mathf.Clamp(auraShadowNormalBias, 0f, 3f);
            outerFillIntensity = Mathf.Clamp(outerFillIntensity, 0f, 0.6f);
            outerFillRange = Mathf.Clamp(outerFillRange, 0.1f, 18f);
            outerFillHeightOffset = Mathf.Clamp(outerFillHeightOffset, 0f, 6f);
            aimAccentMaxAuraIntensityFraction = Mathf.Clamp01(aimAccentMaxAuraIntensityFraction);
            aimAccentIntensity = Mathf.Clamp(aimAccentIntensity, 0f, Mathf.Max(0.01f, auraIntensity * aimAccentMaxAuraIntensityFraction));
            aimAccentRange = Mathf.Clamp(aimAccentRange, 0.1f, 12f);
            aimAccentSpotAngle = Mathf.Clamp(aimAccentSpotAngle, 20f, 120f);
            aimAccentHeightOffset = Mathf.Clamp(aimAccentHeightOffset, 0f, 6f);
            aimAccentForwardOffset = Mathf.Clamp(aimAccentForwardOffset, 0f, 8f);
            aimAccentRotationSharpness = Mathf.Max(0f, aimAccentRotationSharpness);
            pulseAmplitude = Mathf.Clamp(pulseAmplitude, 0f, 0.2f);
            pulseSpeed = Mathf.Clamp(pulseSpeed, 0.05f, 4f);
            combatIntensityBoost = Mathf.Clamp(combatIntensityBoost, 0f, 0.35f);
            lowHealthPulseBoost = Mathf.Clamp(lowHealthPulseBoost, 0f, 0.35f);
        }
    }
}
