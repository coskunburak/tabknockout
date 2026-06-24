using NUnit.Framework;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Player.Tests
{
    public sealed class DashDirectionResolverTests
    {
        [Test]
        public void Resolve_PrefersCurrentMovementDirection()
        {
            var result = DashDirectionResolver.Resolve(Vector3.right, Vector3.forward, Vector3.back);

            Assert.That(result, Is.EqualTo(Vector3.right));
        }

        [Test]
        public void Resolve_FallsBackToLastFacingDirection()
        {
            var result = DashDirectionResolver.Resolve(Vector3.zero, Vector3.forward, Vector3.right);

            Assert.That(result, Is.EqualTo(Vector3.forward));
        }

        [Test]
        public void Resolve_FallsBackToTransformForwardAndFlattensY()
        {
            var result = DashDirectionResolver.Resolve(Vector3.zero, Vector3.zero, new Vector3(0f, 5f, -2f));

            Assert.That(result, Is.EqualTo(Vector3.back));
        }
    }
}
