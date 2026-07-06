using NUnit.Framework;
using TapKnockout.Camera;
using UnityEngine;

namespace TapKnockout.Camera.Tests
{
    public sealed class GameplayCameraConfigTests
    {
        [Test]
        public void DefaultConfigValues_AreSafeForDesktopSurvivorGameplay()
        {
            var config = ScriptableObject.CreateInstance<GameplayCameraConfig>();

            try
            {
                Assert.That(config.PitchDegrees, Is.InRange(48f, 56f));
                Assert.That(config.CameraDistance, Is.InRange(14f, 22f));
                Assert.That(config.PlayerViewportAnchor.x, Is.EqualTo(0.5f).Within(0.001f));
                Assert.That(config.PlayerViewportAnchor.y, Is.InRange(0.45f, 0.52f));
                Assert.That(config.ForwardLookAhead, Is.InRange(0.5f, 1.5f));
                Assert.That(config.EnableMovementLookAhead, Is.True);
                Assert.That(config.MovementLookAheadStrength, Is.InRange(0.5f, 1.5f));
                Assert.That(config.MaxMovementLookAhead, Is.InRange(1f, 2f));
                Assert.That(config.EnableDashLookAhead, Is.True);
                Assert.That(config.DashLookAheadMultiplier, Is.InRange(1f, 2f));
                Assert.That(config.DashLookAheadDuration, Is.InRange(0.08f, 0.2f));
                Assert.That(config.PositionSmoothTime, Is.GreaterThan(0f));
                Assert.That(config.RotationSharpness, Is.GreaterThan(0f));
                Assert.That(config.UseOrthographic, Is.True);
                Assert.That(config.FieldOfView, Is.InRange(30f, 70f));
                Assert.That(config.OrthographicSize, Is.InRange(10f, 12f));
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
        public void DefaultConfig_CoversArenaCombatBandAtSixteenByNine()
        {
            var config = ScriptableObject.CreateInstance<GameplayCameraConfig>();

            try
            {
                var effectiveSize = CameraFramingUtility.ResolveAspectSafeOrthographicSize(
                    config.OrthographicSize,
                    16f / 9f,
                    config.MinimumSupportedAspect,
                    config.MaximumSupportedAspect);
                var visibleWidth = effectiveSize * 2f * (16f / 9f);

                Assert.That(visibleWidth, Is.GreaterThanOrEqualTo(36f));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }
    }
}
