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
    }
}
