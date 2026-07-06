using NUnit.Framework;
using TapKnockout.Visuals;
using UnityEngine;

namespace TapKnockout.Visuals.Tests
{
    public sealed class TapKnockoutLightingConfigTests
    {
        [Test]
        public void DefaultLightingConfig_UsesCappedPerformanceFriendlyAccentLights()
        {
            var config = ScriptableObject.CreateInstance<TapKnockoutLightingConfig>();

            try
            {
                Assert.That(config.MainLightIntensity, Is.InRange(0.45f, 0.6f));
                Assert.That(config.MainLightShadowStrength, Is.InRange(0.45f, 0.62f));
                Assert.That(config.AmbientIntensity, Is.InRange(0.18f, 0.26f));
                Assert.That(config.FogDensity, Is.InRange(0.008f, 0.014f));
                Assert.That(config.MaxRuntimeAccentLights, Is.LessThanOrEqualTo(4));
                Assert.That(config.AccentLights.Count, Is.GreaterThanOrEqualTo(4));

                for (var i = 0; i < config.AccentLights.Count; i++)
                {
                    Assert.That(config.AccentLights[i].Range, Is.LessThanOrEqualTo(16f));
                    Assert.That(config.AccentLights[i].Intensity, Is.LessThanOrEqualTo(8f));
                    Assert.That(config.AccentLights[i].CastsShadows, Is.False);
                }
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }
    }
}
