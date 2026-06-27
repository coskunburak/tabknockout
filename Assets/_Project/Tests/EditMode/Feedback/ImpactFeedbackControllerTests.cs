using NUnit.Framework;
using TapKnockout.Combat;
using TapKnockout.Feedback;
using UnityEngine;

namespace TapKnockout.Feedback.Tests
{
    public sealed class ImpactFeedbackControllerTests
    {
        [Test]
        public void TryTriggerFeedback_WithMissingOptionalServices_DoesNotThrow()
        {
            var controllerObject = new GameObject("ImpactFeedbackController");
            var source = new GameObject("Source");
            var target = new GameObject("Target");

            try
            {
                var controller = controllerObject.AddComponent<ImpactFeedbackController>();
                var triggered = false;
                controller.OnImpactFeedbackTriggered += _ => triggered = true;

                var hitContext = new HitContext(source, target, 5f, DamageType.Impact)
                {
                    IsDashHit = true,
                    HitPoint = Vector3.one,
                    HitDirection = Vector3.forward
                };

                Assert.That(controller.TryTriggerFeedback(hitContext), Is.True);
                Assert.That(triggered, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(source);
                Object.DestroyImmediate(controllerObject);
            }
        }
    }
}
