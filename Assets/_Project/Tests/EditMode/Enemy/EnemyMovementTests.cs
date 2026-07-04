using NUnit.Framework;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Enemy.Tests
{
    public sealed class EnemyMovementTests
    {
        [Test]
        public void IsWithinStoppingDistance_UsesHorizontalDistanceOnly()
        {
            var current = new Vector3(0f, 0f, 0f);
            var target = new Vector3(0f, 10f, 1f);

            Assert.That(EnemyMovement.IsWithinStoppingDistance(current, target, 1.1f), Is.True);
            Assert.That(EnemyMovement.IsWithinStoppingDistance(current, target, 0.9f), Is.False);
        }

        [Test]
        public void CalculateSeparationOffset_PushesAwayFromNearbyNeighbor()
        {
            var current = new Vector3(0f, 0f, 0f);
            var neighbor = new Vector3(0.25f, 4f, 0f);

            var offset = EnemyMovement.CalculateSeparationOffset(current, neighbor, 1f);

            Assert.That(offset.x, Is.LessThan(0f));
            Assert.That(offset.y, Is.EqualTo(0f));
            Assert.That(offset.magnitude, Is.EqualTo(0.75f).Within(0.0001f));
        }

        [Test]
        public void CalculateSeparationOffset_IgnoresNeighborOutsideRadius()
        {
            var offset = EnemyMovement.CalculateSeparationOffset(Vector3.zero, new Vector3(2f, 0f, 0f), 1f);

            Assert.That(offset, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void CalculateNormalizedMoveSpeed_ClampsAgainstMaximumSpeed()
        {
            Assert.That(EnemyMovement.CalculateNormalizedMoveSpeed(1f, 2f), Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(EnemyMovement.CalculateNormalizedMoveSpeed(3f, 2f), Is.EqualTo(1f));
            Assert.That(EnemyMovement.CalculateNormalizedMoveSpeed(-1f, 2f), Is.EqualTo(0f));
            Assert.That(EnemyMovement.CalculateNormalizedMoveSpeed(1f, 0f), Is.EqualTo(0f));
        }

        [Test]
        public void CalculateDesiredDirection_DoesNotLetSeparationPushEnemyBackward()
        {
            var desired = EnemyMovement.CalculateDesiredDirection(Vector3.forward, Vector3.back, 4f);

            Assert.That(Vector3.Dot(desired, Vector3.forward), Is.GreaterThan(0.99f));
        }

        [Test]
        public void CalculateDesiredDirection_PreservesLateralSeparation()
        {
            var desired = EnemyMovement.CalculateDesiredDirection(Vector3.forward, new Vector3(1f, 0f, -1f), 1f);

            Assert.That(desired.x, Is.GreaterThan(0f));
            Assert.That(Vector3.Dot(desired, Vector3.forward), Is.GreaterThan(0f));
        }
    }
}
