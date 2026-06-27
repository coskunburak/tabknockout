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
    }
}
