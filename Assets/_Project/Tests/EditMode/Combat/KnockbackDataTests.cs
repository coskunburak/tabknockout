using NUnit.Framework;
using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Combat.Tests
{
    public sealed class KnockbackDataTests
    {
        [Test]
        public void Constructor_NormalizesDirection()
        {
            var knockback = new KnockbackData(new Vector3(0f, 0f, 5f), 8f, 0.2f);

            Assert.That(knockback.Direction.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(knockback.Direction.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(knockback.Direction.z, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Constructor_ClampsNegativeForceAndDuration()
        {
            var knockback = new KnockbackData(Vector3.forward, -1f, -2f);

            Assert.That(knockback.Force, Is.EqualTo(0f));
            Assert.That(knockback.Duration, Is.EqualTo(0f));
            Assert.That(knockback.HasKnockback, Is.False);
        }

        [Test]
        public void None_HasNoKnockback()
        {
            Assert.That(KnockbackData.None.HasKnockback, Is.False);
        }
    }
}
