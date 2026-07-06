using System;
using UnityEngine;

namespace TapKnockout.Visuals
{
    [Serializable]
    public sealed class TapKnockoutRenderProfile
    {
        [Header("URP Baseline")]
        [SerializeField] private bool hdrEnabled = true;
        [SerializeField, Range(1, 8)] private int msaaSampleCount = 2;
        [SerializeField, Range(0.6f, 1.2f)] private float renderScale = 1f;
        [SerializeField] private bool depthTextureEnabled = true;
        [SerializeField] private bool opaqueTextureEnabled;
        [SerializeField] private bool srpBatcherEnabled = true;

        [Header("Shadows")]
        [SerializeField] private bool mainLightShadowsEnabled = true;
        [SerializeField] private bool softShadowsEnabled = true;
        [SerializeField, Range(512, 4096)] private int mainLightShadowResolution = 2048;
        [SerializeField, Range(0f, 120f)] private float shadowDistance = 45f;
        [SerializeField, Range(1, 4)] private int shadowCascadeCount = 2;
        [SerializeField] private bool additionalLightShadowsEnabled;
        [SerializeField, Range(0, 8)] private int additionalLightsPerObjectLimit = 4;

        [Header("Post Processing")]
        [SerializeField, Range(0f, 2f)] private float bloomIntensity = 0.32f;
        [SerializeField, Range(0f, 3f)] private float bloomThreshold = 1.35f;
        [SerializeField, Range(0f, 1f)] private float bloomScatter = 0.46f;
        [SerializeField, Range(0f, 1.5f)] private float ambientOcclusionIntensity = 0.38f;
        [SerializeField, Range(0.05f, 2f)] private float ambientOcclusionRadius = 0.8f;
        [SerializeField, Range(-2f, 2f)] private float postExposure = -0.32f;
        [SerializeField, Range(-50f, 50f)] private float contrast = 18f;
        [SerializeField, Range(-50f, 50f)] private float saturation = 0f;
        [SerializeField, Range(0f, 0.4f)] private float vignetteIntensity = 0.22f;
        [SerializeField, Range(0f, 1f)] private float vignetteSmoothness = 0.58f;
        [SerializeField] private bool radialDarknessOverlayEnabled = true;
        [SerializeField] private Color radialDarknessColor = new Color(0.006f, 0.012f, 0.014f, 1f);
        [SerializeField, Range(0f, 1f)] private float radialDarknessEdgeOpacity = 0.62f;
        [SerializeField, Range(0.05f, 0.6f)] private float radialDarknessClearRadius = 0.28f;
        [SerializeField, Range(0.3f, 1.6f)] private float radialDarknessFullRadius = 1.25f;
        [SerializeField, Min(0f)] private float radialDarknessFollowSharpness = 28f;
        [SerializeField, Range(0f, 0.08f)] private float filmGrainIntensity;
        [SerializeField] private bool motionBlurEnabled;
        [SerializeField] private bool depthOfFieldEnabled;

        public bool HdrEnabled => hdrEnabled;
        public int MsaaSampleCount => msaaSampleCount;
        public float RenderScale => renderScale;
        public bool DepthTextureEnabled => depthTextureEnabled;
        public bool OpaqueTextureEnabled => opaqueTextureEnabled;
        public bool SrpBatcherEnabled => srpBatcherEnabled;
        public bool MainLightShadowsEnabled => mainLightShadowsEnabled;
        public bool SoftShadowsEnabled => softShadowsEnabled;
        public int MainLightShadowResolution => mainLightShadowResolution;
        public float ShadowDistance => shadowDistance;
        public int ShadowCascadeCount => shadowCascadeCount;
        public bool AdditionalLightShadowsEnabled => additionalLightShadowsEnabled;
        public int AdditionalLightsPerObjectLimit => additionalLightsPerObjectLimit;
        public float BloomIntensity => bloomIntensity;
        public float BloomThreshold => bloomThreshold;
        public float BloomScatter => bloomScatter;
        public float AmbientOcclusionIntensity => ambientOcclusionIntensity;
        public float AmbientOcclusionRadius => ambientOcclusionRadius;
        public float PostExposure => postExposure;
        public float Contrast => contrast;
        public float Saturation => saturation;
        public float VignetteIntensity => vignetteIntensity;
        public float VignetteSmoothness => vignetteSmoothness;
        public bool RadialDarknessOverlayEnabled => radialDarknessOverlayEnabled;
        public Color RadialDarknessColor => radialDarknessColor;
        public float RadialDarknessEdgeOpacity => radialDarknessEdgeOpacity;
        public float RadialDarknessClearRadius => radialDarknessClearRadius;
        public float RadialDarknessFullRadius => radialDarknessFullRadius;
        public float RadialDarknessFollowSharpness => radialDarknessFollowSharpness;
        public float FilmGrainIntensity => filmGrainIntensity;
        public bool MotionBlurEnabled => motionBlurEnabled;
        public bool DepthOfFieldEnabled => depthOfFieldEnabled;

        public void ApplyPrototypeLowDefaults()
        {
            hdrEnabled = true;
            msaaSampleCount = 1;
            renderScale = 0.85f;
            depthTextureEnabled = true;
            opaqueTextureEnabled = false;
            srpBatcherEnabled = true;
            mainLightShadowsEnabled = true;
            softShadowsEnabled = false;
            mainLightShadowResolution = 1024;
            shadowDistance = 34f;
            shadowCascadeCount = 1;
            additionalLightShadowsEnabled = false;
            additionalLightsPerObjectLimit = 2;
            bloomIntensity = 0.2f;
            bloomThreshold = 1.45f;
            bloomScatter = 0.42f;
            ambientOcclusionIntensity = 0.18f;
            ambientOcclusionRadius = 0.45f;
            postExposure = -0.22f;
            contrast = 12f;
            saturation = -2f;
            vignetteIntensity = 0.18f;
            vignetteSmoothness = 0.52f;
            radialDarknessOverlayEnabled = true;
            radialDarknessColor = new Color(0.008f, 0.014f, 0.014f, 1f);
            radialDarknessEdgeOpacity = 0.5f;
            radialDarknessClearRadius = 0.3f;
            radialDarknessFullRadius = 1.35f;
            radialDarknessFollowSharpness = 24f;
            filmGrainIntensity = 0f;
            motionBlurEnabled = false;
            depthOfFieldEnabled = false;
            ClampValues();
        }

        public void ApplyPrototypeMediumDefaults()
        {
            hdrEnabled = true;
            msaaSampleCount = 2;
            renderScale = 1f;
            depthTextureEnabled = true;
            opaqueTextureEnabled = false;
            srpBatcherEnabled = true;
            mainLightShadowsEnabled = true;
            softShadowsEnabled = true;
            mainLightShadowResolution = 2048;
            shadowDistance = 45f;
            shadowCascadeCount = 2;
            additionalLightShadowsEnabled = false;
            additionalLightsPerObjectLimit = 4;
            bloomIntensity = 0.32f;
            bloomThreshold = 1.35f;
            bloomScatter = 0.46f;
            ambientOcclusionIntensity = 0.38f;
            ambientOcclusionRadius = 0.8f;
            postExposure = -0.32f;
            contrast = 18f;
            saturation = 0f;
            vignetteIntensity = 0.22f;
            vignetteSmoothness = 0.58f;
            radialDarknessOverlayEnabled = true;
            radialDarknessColor = new Color(0.006f, 0.012f, 0.014f, 1f);
            radialDarknessEdgeOpacity = 0.62f;
            radialDarknessClearRadius = 0.28f;
            radialDarknessFullRadius = 1.25f;
            radialDarknessFollowSharpness = 28f;
            filmGrainIntensity = 0f;
            motionBlurEnabled = false;
            depthOfFieldEnabled = false;
            ClampValues();
        }

        public void ApplyPrototypeHighDefaults()
        {
            hdrEnabled = true;
            msaaSampleCount = 4;
            renderScale = 1.05f;
            depthTextureEnabled = true;
            opaqueTextureEnabled = false;
            srpBatcherEnabled = true;
            mainLightShadowsEnabled = true;
            softShadowsEnabled = true;
            mainLightShadowResolution = 2048;
            shadowDistance = 55f;
            shadowCascadeCount = 4;
            additionalLightShadowsEnabled = false;
            additionalLightsPerObjectLimit = 4;
            bloomIntensity = 0.42f;
            bloomThreshold = 1.3f;
            bloomScatter = 0.5f;
            ambientOcclusionIntensity = 0.48f;
            ambientOcclusionRadius = 1f;
            postExposure = -0.38f;
            contrast = 22f;
            saturation = 1f;
            vignetteIntensity = 0.24f;
            vignetteSmoothness = 0.62f;
            radialDarknessOverlayEnabled = true;
            radialDarknessColor = new Color(0.004f, 0.01f, 0.012f, 1f);
            radialDarknessEdgeOpacity = 0.68f;
            radialDarknessClearRadius = 0.27f;
            radialDarknessFullRadius = 1.18f;
            radialDarknessFollowSharpness = 30f;
            filmGrainIntensity = 0.015f;
            motionBlurEnabled = false;
            depthOfFieldEnabled = false;
            ClampValues();
        }

        public void ClampValues()
        {
            msaaSampleCount = ResolveSupportedMsaa(msaaSampleCount);
            renderScale = Mathf.Clamp(renderScale, 0.6f, 1.2f);
            mainLightShadowResolution = Mathf.Clamp(ResolvePowerOfTwo(mainLightShadowResolution), 512, 4096);
            shadowDistance = Mathf.Clamp(shadowDistance, 0f, 120f);
            shadowCascadeCount = Mathf.Clamp(shadowCascadeCount, 1, 4);
            additionalLightsPerObjectLimit = Mathf.Clamp(additionalLightsPerObjectLimit, 0, 8);
            bloomIntensity = Mathf.Clamp(bloomIntensity, 0f, 2f);
            bloomThreshold = Mathf.Clamp(bloomThreshold, 0f, 3f);
            bloomScatter = Mathf.Clamp01(bloomScatter);
            ambientOcclusionIntensity = Mathf.Clamp(ambientOcclusionIntensity, 0f, 1.5f);
            ambientOcclusionRadius = Mathf.Clamp(ambientOcclusionRadius, 0.05f, 2f);
            postExposure = Mathf.Clamp(postExposure, -2f, 2f);
            contrast = Mathf.Clamp(contrast, -50f, 50f);
            saturation = Mathf.Clamp(saturation, -50f, 50f);
            vignetteIntensity = Mathf.Clamp(vignetteIntensity, 0f, 0.4f);
            vignetteSmoothness = Mathf.Clamp01(vignetteSmoothness);
            radialDarknessEdgeOpacity = Mathf.Clamp01(radialDarknessEdgeOpacity);
            radialDarknessClearRadius = Mathf.Clamp(radialDarknessClearRadius, 0.05f, 0.6f);
            radialDarknessFullRadius = Mathf.Clamp(radialDarknessFullRadius, Mathf.Max(0.3f, radialDarknessClearRadius + 0.05f), 1.6f);
            radialDarknessFollowSharpness = Mathf.Max(0f, radialDarknessFollowSharpness);
            filmGrainIntensity = Mathf.Clamp(filmGrainIntensity, 0f, 0.08f);
        }

        private static int ResolveSupportedMsaa(int value)
        {
            if (value >= 8)
            {
                return 8;
            }

            if (value >= 4)
            {
                return 4;
            }

            if (value >= 2)
            {
                return 2;
            }

            return 1;
        }

        private static int ResolvePowerOfTwo(int value)
        {
            if (value <= 512)
            {
                return 512;
            }

            if (value <= 1024)
            {
                return 1024;
            }

            if (value <= 2048)
            {
                return 2048;
            }

            return 4096;
        }
    }
}
