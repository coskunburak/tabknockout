using System;
using NUnit.Framework;
using TapKnockout.Combat;

namespace TapKnockout.Combat.Tests
{
    public sealed class DamageTypeTests
    {
        [Test]
        public void DamageType_IncludesVerticalSliceTypes()
        {
            Assert.That(Enum.IsDefined(typeof(DamageType), DamageType.Physical), Is.True);
            Assert.That(Enum.IsDefined(typeof(DamageType), DamageType.Impact), Is.True);
        }

        [Test]
        public void DamageType_IncludesPlannedStatusTypes()
        {
            Assert.That(Enum.IsDefined(typeof(DamageType), DamageType.Fire), Is.True);
            Assert.That(Enum.IsDefined(typeof(DamageType), DamageType.Lightning), Is.True);
            Assert.That(Enum.IsDefined(typeof(DamageType), DamageType.Poison), Is.True);
            Assert.That(Enum.IsDefined(typeof(DamageType), DamageType.Ice), Is.True);
            Assert.That(Enum.IsDefined(typeof(DamageType), DamageType.True), Is.True);
        }
    }
}
