using NUnit.Framework;
using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Combat.Tests
{
    public sealed class TargetingResultTests
    {
        [Test]
        public void None_HasNoTarget()
        {
            var result = TargetingResult.None;

            Assert.That(result.HasTarget, Is.False);
            Assert.That(result.TargetTransform, Is.Null);
            Assert.That(result.TargetGameObject, Is.Null);
            Assert.That(result.Direction, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Constructor_NormalizesDirectionAndClampsDistance()
        {
            var target = new GameObject("Target");

            try
            {
                var result = new TargetingResult(
                    target.transform,
                    target,
                    null,
                    null,
                    -5f,
                    new Vector3(3f, 0f, 4f));

                Assert.That(result.HasTarget, Is.True);
                Assert.That(result.TargetTransform, Is.EqualTo(target.transform));
                Assert.That(result.TargetGameObject, Is.EqualTo(target));
                Assert.That(result.Distance, Is.EqualTo(0f));
                Assert.That(Vector3.Distance(result.Direction, new Vector3(0.6f, 0f, 0.8f)), Is.LessThan(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
