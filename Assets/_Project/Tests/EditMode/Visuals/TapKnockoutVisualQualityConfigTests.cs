using NUnit.Framework;
using TapKnockout.Visuals;
using UnityEngine;

namespace TapKnockout.Visuals.Tests
{
    public sealed class TapKnockoutVisualQualityConfigTests
    {
        [Test]
        public void PrototypeMediumDefaults_AreGameplaySafe()
        {
            var preset = ScriptableObject.CreateInstance<TapKnockoutVisualQualityPreset>();

            try
            {
                preset.ConfigureDefaults(TapKnockoutVisualQualityLevel.PrototypeMedium);
                var profile = preset.RenderProfile;

                Assert.That(profile.HdrEnabled, Is.True);
                Assert.That(profile.MsaaSampleCount, Is.EqualTo(2));
                Assert.That(profile.RenderScale, Is.EqualTo(1f).Within(0.001f));
                Assert.That(profile.MainLightShadowResolution, Is.EqualTo(2048));
                Assert.That(profile.AdditionalLightShadowsEnabled, Is.False);
                Assert.That(profile.AdditionalLightsPerObjectLimit, Is.GreaterThanOrEqualTo(4));
                Assert.That(profile.BloomIntensity, Is.InRange(0.24f, 0.4f));
                Assert.That(profile.BloomThreshold, Is.InRange(1.25f, 1.5f));
                Assert.That(profile.PostExposure, Is.InRange(-0.4f, -0.25f));
                Assert.That(profile.VignetteIntensity, Is.InRange(0.18f, 0.26f));
                Assert.That(profile.RadialDarknessOverlayEnabled, Is.True);
                Assert.That(profile.RadialDarknessEdgeOpacity, Is.InRange(0.56f, 0.68f));
                Assert.That(profile.RadialDarknessClearRadius, Is.InRange(0.25f, 0.32f));
                Assert.That(profile.RadialDarknessFullRadius, Is.InRange(1.15f, 1.35f));
                Assert.That(profile.MotionBlurEnabled, Is.False);
                Assert.That(profile.DepthOfFieldEnabled, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(preset);
            }
        }

        [Test]
        public void Config_ResolvesDefaultPresetByQualityLevel()
        {
            var config = ScriptableObject.CreateInstance<TapKnockoutVisualQualityConfig>();
            var low = ScriptableObject.CreateInstance<TapKnockoutVisualQualityPreset>();
            var medium = ScriptableObject.CreateInstance<TapKnockoutVisualQualityPreset>();

            try
            {
                low.ConfigureDefaults(TapKnockoutVisualQualityLevel.PrototypeLow);
                medium.ConfigureDefaults(TapKnockoutVisualQualityLevel.PrototypeMedium);
                config.SetPresets(new[] { low, medium });

                Assert.That(config.TryGetPreset(TapKnockoutVisualQualityLevel.PrototypeMedium, out var resolved), Is.True);
                Assert.That(resolved, Is.SameAs(medium));
                Assert.That(config.ResolveDefaultPreset(), Is.SameAs(medium));
            }
            finally
            {
                Object.DestroyImmediate(medium);
                Object.DestroyImmediate(low);
                Object.DestroyImmediate(config);
            }
        }
    }
}
