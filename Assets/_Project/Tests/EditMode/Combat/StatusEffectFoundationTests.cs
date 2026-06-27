using NUnit.Framework;
using UnityEngine;

namespace TapKnockout.Combat.Tests
{
    public sealed class StatusEffectFoundationTests
    {
        [Test]
        public void StatusEffectController_AcceptsValidRequests()
        {
            var target = new GameObject("StatusTarget");

            try
            {
                var controller = target.AddComponent<StatusEffectController>();
                var request = new StatusEffectRequest(StatusEffectType.Slow, null, 2f, slowMultiplier: 0.5f);

                Assert.That(controller.TryApplyStatusEffect(request), Is.True);
                Assert.That(controller.ActiveEffectCount, Is.EqualTo(1));
                Assert.That(controller.MoveSpeedMultiplier, Is.EqualTo(0.5f));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void StatusEffectRequest_InvalidNoneEffectIsRejected()
        {
            var target = new GameObject("StatusTarget");

            try
            {
                var controller = target.AddComponent<StatusEffectController>();

                Assert.That(controller.TryApplyStatusEffect(new StatusEffectRequest(StatusEffectType.None, null, 1f)), Is.False);
                Assert.That(controller.ActiveEffectCount, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
