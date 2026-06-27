using NUnit.Framework;
using TapKnockout.Player;

namespace TapKnockout.Player.Tests
{
    public sealed class PerfectDashDetectorTests
    {
        [Test]
        public void ShouldTriggerPerfectDash_RequiresDashInvulnerability()
        {
            Assert.That(PerfectDashDetector.ShouldTriggerPerfectDash(false, 0f, 1f, 0.05f), Is.False);
            Assert.That(PerfectDashDetector.ShouldTriggerPerfectDash(true, 0f, 1f, 0.05f), Is.True);
        }

        [Test]
        public void ShouldTriggerPerfectDash_UsesDebounceWindow()
        {
            Assert.That(PerfectDashDetector.ShouldTriggerPerfectDash(true, 1f, 1.02f, 0.05f), Is.False);
            Assert.That(PerfectDashDetector.ShouldTriggerPerfectDash(true, 1f, 1.06f, 0.05f), Is.True);
        }
    }
}
