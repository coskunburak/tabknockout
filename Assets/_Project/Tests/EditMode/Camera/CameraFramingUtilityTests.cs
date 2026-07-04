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
        public void ResolveAnchoredCenterPosition_ShiftsCenterAheadOfPlayerForLowerScreenFraming()
        {
            var target = Vector3.zero;
            var rotation = CameraFramingUtility.ResolveRigRotation(58f, 0f);
            var result = CameraFramingUtility.ResolveAnchoredCenterPosition(
                target,
                rotation,
                new Vector2(0.5f, 0.38f),
                14f,
                9f / 19.5f,
                new Vector3(0f, 0.85f, 0f),
                0.75f,
                Vector3.zero);

            Assert.That(result.z, Is.GreaterThan(target.z));
            Assert.That(result.y, Is.GreaterThan(target.y));
            Assert.That(Mathf.Abs(result.x), Is.LessThan(0.001f));
        }

        [Test]
        public void ResolveCameraPosition_PlacesCameraBehindAndAboveCenter()
        {
            var rotation = CameraFramingUtility.ResolveRigRotation(58f, 0f);
            var center = new Vector3(0f, 1f, 3f);
            var result = CameraFramingUtility.ResolveCameraPosition(center, rotation, 13f);

            Assert.That(result.y, Is.GreaterThan(center.y));
            Assert.That(result.z, Is.LessThan(center.z));
        }

        [Test]
        public void ResolveAspectSafeOrthographicSize_ZoomsOutForVeryNarrowPortrait()
        {
            var result = CameraFramingUtility.ResolveAspectSafeOrthographicSize(7f, 0.4f, 0.46f, 0.75f);

            Assert.That(result, Is.GreaterThan(7f));
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
