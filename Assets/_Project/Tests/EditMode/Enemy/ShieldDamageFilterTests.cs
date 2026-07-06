using NUnit.Framework;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Enemy.Tests
{
    public sealed class ShieldDamageFilterTests
    {
        [Test]
        public void IsFrontalHit_BlocksFrontButNotRear()
        {
            Assert.That(ShieldDamageFilter.IsFrontalHit(Vector3.forward, Vector3.back, 120f), Is.True);
            Assert.That(ShieldDamageFilter.IsFrontalHit(Vector3.forward, Vector3.forward, 120f), Is.False);
            Assert.That(ShieldDamageFilter.IsFrontalHit(Vector3.forward, Vector3.right, 60f), Is.False);
        }

        [Test]
        public void CalculateDamageAfterShield_ReducesByClampedAmount()
        {
            Assert.That(ShieldDamageFilter.CalculateDamageAfterShield(100f, 0.65f), Is.EqualTo(35f).Within(0.001f));
            Assert.That(ShieldDamageFilter.CalculateDamageAfterShield(100f, 2f), Is.EqualTo(0f).Within(0.001f));
            Assert.That(ShieldDamageFilter.CalculateDamageAfterShield(-10f, 0.5f), Is.EqualTo(0f).Within(0.001f));
        }
    }
}
