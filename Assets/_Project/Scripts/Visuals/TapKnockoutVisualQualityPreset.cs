using UnityEngine;

namespace TapKnockout.Visuals
{
    [CreateAssetMenu(fileName = "TapKnockoutVisualQualityPreset", menuName = "Tap Knockout/Visuals/Visual Quality Preset")]
    public sealed class TapKnockoutVisualQualityPreset : ScriptableObject
    {
        [SerializeField] private string presetId = "prototype_medium";
        [SerializeField] private string displayName = "Prototype Medium";
        [SerializeField] private TapKnockoutVisualQualityLevel qualityLevel = TapKnockoutVisualQualityLevel.PrototypeMedium;
        [SerializeField] private TapKnockoutRenderProfile renderProfile = new TapKnockoutRenderProfile();

        public string PresetId => presetId;
        public string DisplayName => displayName;
        public TapKnockoutVisualQualityLevel QualityLevel => qualityLevel;
        public TapKnockoutRenderProfile RenderProfile => renderProfile;

        public void ConfigureDefaults(TapKnockoutVisualQualityLevel level)
        {
            qualityLevel = level;
            presetId = level switch
            {
                TapKnockoutVisualQualityLevel.PrototypeLow => "prototype_low",
                TapKnockoutVisualQualityLevel.PrototypeHigh => "prototype_high",
                _ => "prototype_medium"
            };
            displayName = level switch
            {
                TapKnockoutVisualQualityLevel.PrototypeLow => "Prototype Low",
                TapKnockoutVisualQualityLevel.PrototypeHigh => "Prototype High",
                _ => "Prototype Medium"
            };

            renderProfile ??= new TapKnockoutRenderProfile();
            switch (level)
            {
                case TapKnockoutVisualQualityLevel.PrototypeLow:
                    renderProfile.ApplyPrototypeLowDefaults();
                    break;
                case TapKnockoutVisualQualityLevel.PrototypeHigh:
                    renderProfile.ApplyPrototypeHighDefaults();
                    break;
                default:
                    renderProfile.ApplyPrototypeMediumDefaults();
                    break;
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(presetId))
            {
                presetId = "prototype_medium";
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = presetId;
            }

            renderProfile ??= new TapKnockoutRenderProfile();
            renderProfile.ClampValues();
        }
    }
}
