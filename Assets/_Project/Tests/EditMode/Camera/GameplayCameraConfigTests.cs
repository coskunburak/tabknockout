using NUnit.Framework;
using TapKnockout.Camera;
using UnityEngine;

namespace TapKnockout.Camera.Tests
{
    public sealed class GameplayCameraConfigTests
    {
        [Test]
        public void DefaultConfigValues_AreSafeForPortraitGameplay()
        {
            var config = ScriptableObject.CreateInstance<GameplayCameraConfig>();

            try
            {
                Assert.That(config.PitchDegrees, Is.InRange(45f, 65f));
                Assert.That(config.CameraDistance, Is.GreaterThanOrEqualTo(20f));
                Assert.That(config.PlayerViewportAnchor.x, Is.EqualTo(0.5f).Within(0.001f));
                Assert.That(config.PlayerViewportAnchor.y, Is.LessThan(0.5f));
                Assert.That(config.ForwardLookAhead, Is.GreaterThanOrEqualTo(0f));
                Assert.That(config.PositionSmoothTime, Is.GreaterThan(0f));
                Assert.That(config.RotationSharpness, Is.GreaterThan(0f));
                Assert.That(config.UseOrthographic, Is.True);
                Assert.That(config.FieldOfView, Is.InRange(30f, 70f));
                Assert.That(config.OrthographicSize, Is.InRange(16f, 17f));
                Assert.That(config.NearClipPlane, Is.GreaterThan(0f));
                Assert.That(config.FarClipPlane, Is.GreaterThan(config.NearClipPlane));
                Assert.That(config.MinimumSupportedAspect, Is.LessThanOrEqualTo(config.MaximumSupportedAspect));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void DefaultConfig_CoversKayKitRoomWidthInNarrowPortrait()
        {
            var config = ScriptableObject.CreateInstance<GameplayCameraConfig>();

            try
            {
                var effectiveSize = CameraFramingUtility.ResolveAspectSafeOrthographicSize(
                    config.OrthographicSize,
                    config.MinimumSupportedAspect,
                    config.MinimumSupportedAspect,
                    config.MaximumSupportedAspect);
                var visibleWidth = effectiveSize * 2f * config.MinimumSupportedAspect;

                Assert.That(visibleWidth, Is.GreaterThanOrEqualTo(14.7f));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }
    }
}
