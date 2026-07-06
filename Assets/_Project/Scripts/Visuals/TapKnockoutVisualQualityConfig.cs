using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Visuals
{
    [CreateAssetMenu(fileName = "TapKnockoutVisualQualityConfig", menuName = "Tap Knockout/Visuals/Visual Quality Config")]
    public sealed class TapKnockoutVisualQualityConfig : ScriptableObject
    {
        [SerializeField] private TapKnockoutVisualQualityLevel defaultQuality = TapKnockoutVisualQualityLevel.PrototypeMedium;
        [SerializeField] private List<TapKnockoutVisualQualityPreset> presets = new List<TapKnockoutVisualQualityPreset>();

        public TapKnockoutVisualQualityLevel DefaultQuality => defaultQuality;
        public IReadOnlyList<TapKnockoutVisualQualityPreset> Presets => presets;

        public bool TryGetPreset(TapKnockoutVisualQualityLevel qualityLevel, out TapKnockoutVisualQualityPreset preset)
        {
            if (presets != null)
            {
                for (var i = 0; i < presets.Count; i++)
                {
                    var candidate = presets[i];
                    if (candidate != null && candidate.QualityLevel == qualityLevel)
                    {
                        preset = candidate;
                        return true;
                    }
                }
            }

            preset = null;
            return false;
        }

        public TapKnockoutVisualQualityPreset ResolveDefaultPreset()
        {
            if (TryGetPreset(defaultQuality, out var preset))
            {
                return preset;
            }

            return presets != null && presets.Count > 0 ? presets[0] : null;
        }

        public void SetPresets(IEnumerable<TapKnockoutVisualQualityPreset> values)
        {
            presets.Clear();
            if (values == null)
            {
                return;
            }

            foreach (var preset in values)
            {
                if (preset != null && !presets.Contains(preset))
                {
                    presets.Add(preset);
                }
            }
        }

        private void OnValidate()
        {
            presets ??= new List<TapKnockoutVisualQualityPreset>();
            for (var i = presets.Count - 1; i >= 0; i--)
            {
                if (presets[i] == null)
                {
                    presets.RemoveAt(i);
                }
            }
        }
    }
}
