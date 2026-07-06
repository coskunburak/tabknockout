using NUnit.Framework;
using TapKnockout.Enemy;

namespace TapKnockout.Enemy.Tests
{
    public sealed class SplitterEnemyTests
    {
        [Test]
        public void CalculateAllowedSpawnCount_ClampsChildrenAndDepth()
        {
            Assert.That(SplitterEnemyController.CalculateAllowedSpawnCount(4, 2, 0, 1), Is.EqualTo(2));
            Assert.That(SplitterEnemyController.CalculateAllowedSpawnCount(2, 6, 1, 1), Is.EqualTo(0));
            Assert.That(SplitterEnemyController.CalculateAllowedSpawnCount(-1, 6, 0, 1), Is.EqualTo(0));
        }
    }
}
