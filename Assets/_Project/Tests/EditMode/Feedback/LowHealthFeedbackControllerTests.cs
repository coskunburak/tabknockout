using NUnit.Framework;
using TapKnockout.Feedback;

namespace TapKnockout.Feedback.Tests
{
    public sealed class LowHealthFeedbackControllerTests
    {
        [Test]
        public void ShouldBeLowHealth_EntersBelowThreshold()
        {
            Assert.That(LowHealthFeedbackController.ShouldBeLowHealth(29f, 100f, 0.3f, 0.08f, false), Is.True);
            Assert.That(LowHealthFeedbackController.ShouldBeLowHealth(31f, 100f, 0.3f, 0.08f, false), Is.False);
        }

        [Test]
        public void ShouldBeLowHealth_UsesHysteresisWhenAlreadyLow()
        {
            Assert.That(LowHealthFeedbackController.ShouldBeLowHealth(36f, 100f, 0.3f, 0.08f, true), Is.True);
            Assert.That(LowHealthFeedbackController.ShouldBeLowHealth(40f, 100f, 0.3f, 0.08f, true), Is.False);
        }
    }
}
