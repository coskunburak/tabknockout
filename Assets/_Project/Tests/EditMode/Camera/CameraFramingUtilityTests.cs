using NUnit.Framework;
using TapKnockout.Camera;
using UnityEngine;

namespace TapKnockout.Camera.Tests
{
    public sealed class CameraFramingUtilityTests
    {
        [Test]
        public void ResolveFollowPosition_AddsOffsetToTarget()
        {
            var result = CameraFramingUtility.ResolveFollowPosition(
                new Vector3(1f, 2f, 3f),
                new Vector3(0f, 10f, -8f));

            Assert.That(result, Is.EqualTo(new Vector3(1f, 12f, -5f)));
        }

        [Test]
        public void ClampPositionToBounds_ClampsOnlyHorizontalAxes()
        {
            var bounds = new Bounds(Vector3.zero, new Vector3(10f, 0f, 8f));
            var result = CameraFramingUtility.ClampPositionToBounds(new Vector3(9f, 12f, -9f), bounds);

            Assert.That(result, Is.EqualTo(new Vector3(5f, 12f, -4f)));
        }

        [Test]
        public void ResolveRotationInterpolation_ReturnsValueBetweenZeroAndOne()
        {
            var result = CameraFramingUtility.ResolveRotationInterpolation(14f, 0.016f);

            Assert.That(result, Is.GreaterThan(0f));
            Assert.That(result, Is.LessThan(1f));
        }
    }
}
