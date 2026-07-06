using UnityEngine;

namespace TapKnockout.Visuals
{
    [DisallowMultipleComponent]
    public sealed class TapKnockoutVisualQualityApplier : MonoBehaviour
    {
        [SerializeField] private TapKnockoutVisualQualityConfig config;
        [SerializeField] private TapKnockoutVisualQualityLevel selectedQuality = TapKnockoutVisualQualityLevel.PrototypeMedium;
        [SerializeField] private UnityEngine.Camera targetCamera;
        [SerializeField] private bool applyOnAwake = true;

        public TapKnockoutVisualQualityConfig Config => config;
        public TapKnockoutVisualQualityLevel SelectedQuality => selectedQuality;

        private void Reset()
        {
            targetCamera = GetComponent<UnityEngine.Camera>();
        }

        private void Awake()
        {
            if (applyOnAwake)
            {
                ApplySelectedQuality();
            }
        }

        public bool ApplySelectedQuality()
        {
            if (config == null || !config.TryGetPreset(selectedQuality, out var preset))
            {
                preset = config != null ? config.ResolveDefaultPreset() : null;
            }

            return ApplyPreset(preset);
        }

        public bool ApplyPreset(TapKnockoutVisualQualityPreset preset)
        {
            if (preset == null || preset.RenderProfile == null)
            {
                return false;
            }

            selectedQuality = preset.QualityLevel;
            ApplyRenderProfile(preset.RenderProfile);
            return true;
        }

        public void SetConfig(TapKnockoutVisualQualityConfig value)
        {
            config = value;
        }

        private void ApplyRenderProfile(TapKnockoutRenderProfile profile)
        {
            profile.ClampValues();

            QualitySettings.antiAliasing = profile.MsaaSampleCount;

            if (targetCamera == null)
            {
                targetCamera = GetComponent<UnityEngine.Camera>();
            }

            if (targetCamera != null)
            {
                targetCamera.allowHDR = profile.HdrEnabled;
                targetCamera.allowMSAA = profile.MsaaSampleCount > 1;
                targetCamera.depthTextureMode = profile.DepthTextureEnabled
                    ? DepthTextureMode.Depth
                    : DepthTextureMode.None;
            }
        }
    }
}
