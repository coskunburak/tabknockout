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
                Assert.That(config.FollowOffset.y, Is.GreaterThan(0f));
                Assert.That(config.FollowOffset.z, Is.LessThan(0f));
                Assert.That(config.PositionSmoothTime, Is.GreaterThan(0f));
                Assert.That(config.RotationSharpness, Is.GreaterThan(0f));
                Assert.That(config.UseOrthographic, Is.True);
                Assert.That(config.FieldOfView, Is.InRange(30f, 70f));
                Assert.That(config.OrthographicSize, Is.InRange(6.5f, 8f));
                Assert.That(config.NearClipPlane, Is.GreaterThan(0f));
                Assert.That(config.FarClipPlane, Is.GreaterThan(config.NearClipPlane));
                Assert.That(config.MinimumSupportedAspect, Is.LessThanOrEqualTo(config.MaximumSupportedAspect));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }
    }
}
