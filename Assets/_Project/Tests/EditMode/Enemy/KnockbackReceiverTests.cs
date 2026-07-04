using NUnit.Framework;
using TapKnockout.Enemy;

namespace TapKnockout.Enemy.Tests
{
    public sealed class KnockbackReceiverTests
    {
        [Test]
        public void CalculateEffectiveForce_AppliesResistance()
        {
            var force = KnockbackReceiver.CalculateEffectiveForce(10f, 0.25f);

            Assert.That(force, Is.EqualTo(7.5f));
        }

        [Test]
        public void CalculateEffectiveForce_ClampsInvalidValues()
        {
            Assert.That(KnockbackReceiver.CalculateEffectiveForce(-10f, 0f), Is.EqualTo(0f));
            Assert.That(KnockbackReceiver.CalculateEffectiveForce(10f, 2f), Is.EqualTo(0f));
        }

        [Test]
        public void CalculateClampedForce_UsesConfiguredMaximum()
        {
            Assert.That(KnockbackReceiver.CalculateClampedForce(12f, 6f), Is.EqualTo(6f));
            Assert.That(KnockbackReceiver.CalculateClampedForce(4f, 6f), Is.EqualTo(4f));
            Assert.That(KnockbackReceiver.CalculateClampedForce(12f, 0f), Is.EqualTo(12f));
        }

        [Test]
        public void CalculateClampedDuration_UsesConfiguredMaximum()
        {
            Assert.That(KnockbackReceiver.CalculateClampedDuration(0.3f, 0.16f), Is.EqualTo(0.16f));
            Assert.That(KnockbackReceiver.CalculateClampedDuration(0.1f, 0.16f), Is.EqualTo(0.1f));
            Assert.That(KnockbackReceiver.CalculateClampedDuration(0.3f, 0f), Is.EqualTo(0.3f));
        }
    }
}
