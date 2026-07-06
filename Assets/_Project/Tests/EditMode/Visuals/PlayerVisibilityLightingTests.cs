using System.Reflection;
using NUnit.Framework;
using TapKnockout.Visuals;
using UnityEngine;

namespace TapKnockout.Visuals.Tests
{
    public sealed class PlayerVisibilityLightingTests
    {
        private static readonly MethodInfo LateUpdateMethod = typeof(PlayerVisibilityLightingController)
            .GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);

        [Test]
        public void DefaultProfile_UsesDirectionIndependentAuraWithSecondaryAimAccent()
        {
            var profile = ScriptableObject.CreateInstance<PlayerVisibilityLightingProfile>();

            try
            {
                Assert.That(profile.EnableMainAura, Is.True);
                Assert.That(profile.AuraIntensity, Is.InRange(0.55f, 0.9f));
                Assert.That(profile.AuraRange, Is.InRange(4.2f, 5.4f));
                Assert.That(ComputeSaturation(profile.AuraColor), Is.LessThanOrEqualTo(0.2f));
                Assert.That(profile.AuraShadowMode, Is.EqualTo(LightShadows.None));
                Assert.That(profile.EnableOuterFill, Is.True);
                Assert.That(profile.OuterFillIntensity, Is.InRange(0.08f, 0.22f));
                Assert.That(profile.OuterFillRange, Is.InRange(9f, 12f));
                Assert.That(profile.OuterFillShadowMode, Is.EqualTo(LightShadows.None));
                Assert.That(profile.EnableAimAccent, Is.False);
                Assert.That(profile.AimAccentIntensity, Is.LessThanOrEqualTo(profile.AuraIntensity * profile.AimAccentMaxAuraIntensityFraction));
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void Controller_KeepsMainAuraPointLightRotationIndependentFromTarget()
        {
            var profile = ScriptableObject.CreateInstance<PlayerVisibilityLightingProfile>();
            var target = new GameObject("PlayerTarget_Test");
            var rig = new GameObject("PlayerVisibilityLighting_Test");

            try
            {
                var controller = rig.AddComponent<PlayerVisibilityLightingController>();
                controller.ApplyProfile(profile);
                controller.SetTarget(target.transform);

                target.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                LateUpdateMethod.Invoke(controller, null);

                Assert.That(controller.MainAuraLight, Is.Not.Null);
                Assert.That(controller.MainAuraLight.type, Is.EqualTo(LightType.Point));
                Assert.That(controller.MainAuraLight.transform.localRotation, Is.EqualTo(Quaternion.identity));
                Assert.That(controller.MainAuraLight.enabled, Is.True);
                Assert.That(controller.OuterFillLight, Is.Not.Null);
                Assert.That(controller.OuterFillLight.type, Is.EqualTo(LightType.Point));
                Assert.That(controller.OuterFillLight.transform.localRotation, Is.EqualTo(Quaternion.identity));
                Assert.That(controller.OuterFillLight.enabled, Is.True);
                Assert.That(controller.AimAccentLight.enabled, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(rig);
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(profile);
            }
        }

        private static float ComputeSaturation(Color color)
        {
            var max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            var min = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
            return Mathf.Approximately(max, 0f) ? 0f : (max - min) / max;
        }
    }
}
