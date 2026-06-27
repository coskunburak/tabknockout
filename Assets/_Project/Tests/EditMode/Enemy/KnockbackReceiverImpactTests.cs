using NUnit.Framework;
using TapKnockout.Enemy;

namespace TapKnockout.Enemy.Tests
{
    public sealed class KnockbackReceiverImpactTests
    {
        [Test]
        public void CalculateWallSlamDamage_UsesBaseDamageAndEffectiveForce()
        {
            var damage = KnockbackReceiver.CalculateWallSlamDamage(8f, 6f, 0.5f);

            Assert.That(damage, Is.EqualTo(10f).Within(0.0001f));
        }

        [Test]
        public void CalculateChainKnockbackDamage_ClampsNegativeInputs()
        {
            var damage = KnockbackReceiver.CalculateChainKnockbackDamage(-8f, -6f, 0.5f);

            Assert.That(damage, Is.EqualTo(0f).Within(0.0001f));
        }
    }
}
