using NUnit.Framework;
using TapKnockout.Visuals;
using UnityEngine;

namespace TapKnockout.Visuals.Tests
{
    public sealed class TapKnockoutPlayerLightRigConfigTests
    {
        [Test]
        public void DefaultPlayerLightRigConfig_UsesPerformanceSafeNonShadowLights()
        {
            var config = ScriptableObject.CreateInstance<TapKnockoutPlayerLightRigConfig>();

            try
            {
                Assert.That(config.LocalHeroLightEnabled, Is.False);
                Assert.That(config.LocalHeroLightIntensity, Is.LessThanOrEqualTo(0.5f));
                Assert.That(config.LocalHeroLightRange, Is.InRange(4f, 5f));
                Assert.That(config.ForwardLightEnabled, Is.False);
                Assert.That(config.ForwardLightIntensity, Is.LessThanOrEqualTo(0.2f));
                Assert.That(config.ForwardLightRange, Is.InRange(5f, 6f));
                Assert.That(config.ForwardLightSpotAngle, Is.InRange(78f, 84f));
                Assert.That(config.ForwardLightIdleIntensityMultiplier, Is.EqualTo(0f));
                Assert.That(config.DashPulseEnabled, Is.True);
                Assert.That(config.DashPulseIntensity, Is.InRange(2f, 2.8f));
                Assert.That(config.DashPulseRange, Is.InRange(8f, 9f));
                Assert.That(config.DashPulseDuration, Is.InRange(0.08f, 0.2f));
                Assert.That(config.VisibleGlowEnabled, Is.True);
                Assert.That(config.GroundGlowRadius, Is.InRange(1.2f, 1.7f));
                Assert.That(config.GroundGlowColor.a, Is.InRange(0.12f, 0.24f));
                Assert.That(config.CoreGlowColor.a, Is.InRange(0.38f, 0.58f));
                Assert.That(config.CoreGlowSize.x, Is.InRange(0.75f, 1.05f));
                Assert.That(config.CoreGlowSize.y, Is.InRange(1.25f, 1.65f));
                Assert.That(config.LanternFieldEnabled, Is.True);
                Assert.That(config.LanternFieldRadius, Is.InRange(4f, 5.2f));
                Assert.That(config.LanternFieldColor.a, Is.InRange(0.04f, 0.12f));
                Assert.That(config.ForwardLanternEnabled, Is.False);
                Assert.That(config.ForwardLanternRange, Is.InRange(5f, 5.5f));
                Assert.That(config.ForwardLanternWidth, Is.InRange(3f, 3.8f));
                Assert.That(config.ForwardLanternIdleAlphaMultiplier, Is.InRange(0.2f, 0.5f));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }
    }
}
