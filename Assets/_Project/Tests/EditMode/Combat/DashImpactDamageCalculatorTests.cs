using NUnit.Framework;
using TapKnockout.Combat;

namespace TapKnockout.Combat.Tests
{
    public sealed class DashImpactDamageCalculatorTests
    {
        [Test]
        public void CalculateDamage_AppliesRuntimeAndConditionalMultipliers()
        {
            var damage = DashImpactDamageCalculator.CalculateDamage(
                10f,
                1.5f,
                20f,
                20f,
                0.2f,
                0.8f,
                1.3f,
                1.25f);

            Assert.That(damage, Is.EqualTo(18.75f).Within(0.0001f));
        }

        [Test]
        public void CalculateSpeedMultiplier_ClampsFastAndSlowDashes()
        {
            var fast = DashImpactDamageCalculator.CalculateSpeedMultiplier(40f, 20f, 0.5f, 0.8f, 1.25f);
            var slow = DashImpactDamageCalculator.CalculateSpeedMultiplier(2f, 20f, 0.5f, 0.8f, 1.25f);

            Assert.That(fast, Is.EqualTo(1.25f).Within(0.0001f));
            Assert.That(slow, Is.EqualTo(0.8f).Within(0.0001f));
        }
    }
}
