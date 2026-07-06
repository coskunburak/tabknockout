using NUnit.Framework;
using TapKnockout.Visuals;
using UnityEngine;

namespace TapKnockout.Visuals.Tests
{
    public sealed class EnvironmentLightingProfileTests
    {
        [Test]
        public void DefaultEnvironmentProfile_KeepsDarkForestReadable()
        {
            var profile = ScriptableObject.CreateInstance<EnvironmentLightingProfile>();

            try
            {
                Assert.That(profile.MoonlightIntensity, Is.InRange(0.45f, 0.6f));
                Assert.That(profile.MoonlightShadowStrength, Is.InRange(0.45f, 0.62f));
                Assert.That(profile.MoonlightShadows, Is.EqualTo(LightShadows.Soft));
                Assert.That(profile.AmbientIntensity, Is.InRange(0.18f, 0.26f));
                Assert.That(profile.FogEnabled, Is.True);
                Assert.That(profile.FogDensity, Is.InRange(0.008f, 0.014f));
                Assert.That(profile.PostExposure, Is.InRange(-0.4f, -0.25f));
                Assert.That(profile.BloomIntensity, Is.LessThanOrEqualTo(0.4f));
                Assert.That(profile.VignetteIntensity, Is.InRange(0.18f, 0.26f));
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }
    }
}
