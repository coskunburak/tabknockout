using NUnit.Framework;
using TapKnockout.Player;

namespace TapKnockout.Player.Tests
{
    public sealed class DashStateTests
    {
        [Test]
        public void TryBegin_StartsDashAndPreventsSpam()
        {
            var state = new DashState();

            Assert.That(state.TryBegin(0.18f, 4f, true, 0.12f), Is.True);
            Assert.That(state.IsDashing, Is.True);
            Assert.That(state.IsIFrameActive, Is.True);
            Assert.That(state.CooldownRemaining, Is.EqualTo(4f));
            Assert.That(state.TryBegin(0.18f, 4f, true, 0.12f), Is.False);
        }

        [Test]
        public void Tick_EndsDashAndIFrameIndependently()
        {
            var state = new DashState();
            state.TryBegin(0.18f, 4f, true, 0.12f);

            state.Tick(0.13f, out var dashEnded, out var iFrameEnded);

            Assert.That(dashEnded, Is.False);
            Assert.That(iFrameEnded, Is.True);
            Assert.That(state.IsDashing, Is.True);
            Assert.That(state.IsIFrameActive, Is.False);

            state.Tick(0.06f, out dashEnded, out iFrameEnded);

            Assert.That(dashEnded, Is.True);
            Assert.That(iFrameEnded, Is.False);
            Assert.That(state.IsDashing, Is.False);
        }

        [Test]
        public void NormalizedCooldown_ReachesZeroAfterCooldownTicksDown()
        {
            var state = new DashState();
            state.TryBegin(0.18f, 4f, false, 0f);

            state.Tick(4f, out _, out _);

            Assert.That(state.CooldownRemaining, Is.EqualTo(0f));
            Assert.That(state.NormalizedCooldown, Is.EqualTo(0f));
        }
    }
}
