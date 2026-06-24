using NUnit.Framework;
using TapKnockout.Level;

namespace TapKnockout.Level.Tests
{
    public sealed class ChapterRunStateTests
    {
        [Test]
        public void StartRun_InitializesSafeDefaults()
        {
            var state = new ChapterRunState();

            state.StartRun(3);

            Assert.That(state.IsRunActive, Is.True);
            Assert.That(state.TotalRoomCount, Is.EqualTo(3));
            Assert.That(state.CurrentRoomIndex, Is.EqualTo(-1));
            Assert.That(state.IsRewardPending, Is.False);
            Assert.That(state.IsChapterCompleted, Is.False);
            Assert.That(state.IsChapterFailed, Is.False);
        }

        [Test]
        public void AbilitySelectionPending_BlocksRewardResolutionUntilCleared()
        {
            var state = new ChapterRunState();
            state.StartRun(2);
            state.MarkRoomStarted(0);
            state.MarkRoomCompleted();

            state.MarkAbilitySelectionPending();

            Assert.That(state.IsRewardPending, Is.True);
            Assert.That(state.IsAbilitySelectionPending, Is.True);
            Assert.That(state.IsWaitingForContinue, Is.False);

            state.MarkTransitioning();

            Assert.That(state.IsRewardPending, Is.False);
            Assert.That(state.IsAbilitySelectionPending, Is.False);
            Assert.That(state.IsTransitioning, Is.True);
        }

        [Test]
        public void MarkChapterFailed_ClearsPendingRewardFlags()
        {
            var state = new ChapterRunState();
            state.StartRun(2);
            state.MarkRoomStarted(0);
            state.MarkAbilitySelectionPending();

            state.MarkChapterFailed();

            Assert.That(state.IsRunActive, Is.False);
            Assert.That(state.IsChapterFailed, Is.True);
            Assert.That(state.IsRewardPending, Is.False);
            Assert.That(state.IsAbilitySelectionPending, Is.False);
        }
    }
}
