using NUnit.Framework;
using TapKnockout.Level;
using UnityEngine;

namespace TapKnockout.Level.Tests
{
    public sealed class ChapterConfigTests
    {
        [Test]
        public void DefaultChapterConfigValues_AreSafe()
        {
            var config = ScriptableObject.CreateInstance<ChapterConfig>();

            try
            {
                Assert.That(config.ChapterId, Is.Not.Empty);
                Assert.That(config.DisplayName, Is.Not.Empty);
                Assert.That(config.ChapterIndex, Is.GreaterThan(0));
                Assert.That(config.Rooms, Is.Not.Null);
                Assert.That(config.RecommendedPower, Is.GreaterThanOrEqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }
    }
}
