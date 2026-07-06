using UnityEngine;

namespace TapKnockout.Visuals
{
    [CreateAssetMenu(
        fileName = "EnvironmentLightingProfile",
        menuName = "Tap Knockout/Visuals/Environment Lighting Profile")]
    public sealed class EnvironmentLightingProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string profileId = "forest_arena_environment_default";
        [SerializeField] private LightingQualityTier qualityTier = LightingQualityTier.Default;

        [Header("Moonlight")]
        [SerializeField] private Color moonlightColor = new Color(0.56f, 0.68f, 0.78f, 1f);
        [SerializeField, Range(0f, 2f)] private float moonlightIntensity = 0.52f;
        [SerializeField] private Vector3 moonlightEuler = new Vector3(54f, -42f, 0f);
        [SerializeField] private LightShadows moonlightShadows = LightShadows.Soft;
        [SerializeField, Range(0f, 1f)] private float moonlightShadowStrength = 0.55f;

        [Header("Ambient")]
        [SerializeField] private Color ambientSkyColor = new Color(0.052f, 0.066f, 0.072f, 1f);
        [SerializeField] private Color ambientEquatorColor = new Color(0.028f, 0.038f, 0.034f, 1f);
        [SerializeField] private Color ambientGroundColor = new Color(0.014f, 0.016f, 0.014f, 1f);
        [SerializeField, Range(0f, 1f)] private float ambientIntensity = 0.22f;

        [Header("Fog")]
        [SerializeField] private bool fogEnabled = true;
        [SerializeField] private Color fogColor = new Color(0.026f, 0.038f, 0.036f, 1f);
        [SerializeField, Range(0f, 0.03f)] private float fogDensity = 0.011f;

        [Header("Post")]
        [SerializeField, Range(-2f, 2f)] private float postExposure = -0.32f;
        [SerializeField, Range(-50f, 50f)] private float contrast = 18f;
        [SerializeField, Range(-50f, 50f)] private float saturation = 0f;
        [SerializeField, Range(0f, 2f)] private float bloomIntensity = 0.32f;
        [SerializeField, Range(0f, 3f)] private float bloomThreshold = 1.35f;
        [SerializeField, Range(0f, 0.4f)] private float vignetteIntensity = 0.22f;

        public string ProfileId => profileId;
        public LightingQualityTier QualityTier => qualityTier;
        public Color MoonlightColor => moonlightColor;
        public float MoonlightIntensity => moonlightIntensity;
        public Vector3 MoonlightEuler => moonlightEuler;
        public LightShadows MoonlightShadows => moonlightShadows;
        public float MoonlightShadowStrength => moonlightShadowStrength;
        public Color AmbientSkyColor => ambientSkyColor;
        public Color AmbientEquatorColor => ambientEquatorColor;
        public Color AmbientGroundColor => ambientGroundColor;
        public float AmbientIntensity => ambientIntensity;
        public bool FogEnabled => fogEnabled;
        public Color FogColor => fogColor;
        public float FogDensity => fogDensity;
        public float PostExposure => postExposure;
        public float Contrast => contrast;
        public float Saturation => saturation;
        public float BloomIntensity => bloomIntensity;
        public float BloomThreshold => bloomThreshold;
        public float VignetteIntensity => vignetteIntensity;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(profileId))
            {
                profileId = "forest_arena_environment_default";
            }

            moonlightIntensity = Mathf.Clamp(moonlightIntensity, 0f, 2f);
            moonlightShadowStrength = Mathf.Clamp01(moonlightShadowStrength);
            ambientIntensity = Mathf.Clamp01(ambientIntensity);
            fogDensity = Mathf.Clamp(fogDensity, 0f, 0.03f);
            postExposure = Mathf.Clamp(postExposure, -2f, 2f);
            contrast = Mathf.Clamp(contrast, -50f, 50f);
            saturation = Mathf.Clamp(saturation, -50f, 50f);
            bloomIntensity = Mathf.Clamp(bloomIntensity, 0f, 2f);
            bloomThreshold = Mathf.Clamp(bloomThreshold, 0f, 3f);
            vignetteIntensity = Mathf.Clamp(vignetteIntensity, 0f, 0.4f);
        }
    }
}
