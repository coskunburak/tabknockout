using NUnit.Framework;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Player.Tests
{
    public sealed class DashHitRegistryTests
    {
        [Test]
        public void TryRegister_ReturnsFalseForDuplicateTargetInSameDash()
        {
            var target = new GameObject("Target");
            var registry = new DashHitRegistry();

            try
            {
                Assert.That(registry.TryRegister(target), Is.True);
                Assert.That(registry.TryRegister(target), Is.False);
                Assert.That(registry.Count, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void Clear_AllowsTargetToBeRegisteredAgain()
        {
            var target = new GameObject("Target");
            var registry = new DashHitRegistry();

            try
            {
                registry.TryRegister(target);
                registry.Clear();

                Assert.That(registry.TryRegister(target), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
