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
    }
}
