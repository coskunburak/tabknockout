using TapKnockout.Room;

namespace TapKnockout.Level
{
    public readonly struct RoomCompletionDecisionResult
    {
        public RoomCompletionDecisionResult(
            bool shouldCompleteChapter,
            bool shouldFailChapter,
            bool shouldOpenAbilitySelection,
            bool shouldWaitForContinue,
            bool shouldAutoAdvance,
            RoomRewardType rewardType)
        {
            ShouldCompleteChapter = shouldCompleteChapter;
            ShouldFailChapter = shouldFailChapter;
            ShouldOpenAbilitySelection = shouldOpenAbilitySelection;
            ShouldWaitForContinue = shouldWaitForContinue;
            ShouldAutoAdvance = shouldAutoAdvance;
            RewardType = rewardType;
        }

        public bool ShouldCompleteChapter { get; }
        public bool ShouldFailChapter { get; }
        public bool ShouldOpenAbilitySelection { get; }
        public bool ShouldWaitForContinue { get; }
        public bool ShouldAutoAdvance { get; }
        public RoomRewardType RewardType { get; }
    }
}
