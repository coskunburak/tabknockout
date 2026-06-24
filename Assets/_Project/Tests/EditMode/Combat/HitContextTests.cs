using NUnit.Framework;
using TapKnockout.Combat;

namespace TapKnockout.Combat.Tests
{
    public sealed class HitContextTests
    {
        [Test]
        public void DefaultConstructor_UsesSafeNeutralValues()
        {
            var hitContext = new HitContext();

            Assert.That(hitContext.Source, Is.Null);
            Assert.That(hitContext.Target, Is.Null);
            Assert.That(hitContext.DamageAmount, Is.EqualTo(0f));
            Assert.That(hitContext.DamageType, Is.EqualTo(DamageType.Physical));
            Assert.That(hitContext.CriticalMultiplier, Is.EqualTo(1f));
            Assert.That(hitContext.WasIgnored, Is.False);
            Assert.That(hitContext.AbilityId, Is.Empty);
            Assert.That(hitContext.Knockback.HasKnockback, Is.False);
        }

        [Test]
        public void DamageConstructor_ClampsNegativeDamage()
        {
            var hitContext = new HitContext(null, null, -5f, DamageType.Impact);

            Assert.That(hitContext.DamageAmount, Is.EqualTo(0f));
            Assert.That(hitContext.DamageType, Is.EqualTo(DamageType.Impact));
        }
    }
}
