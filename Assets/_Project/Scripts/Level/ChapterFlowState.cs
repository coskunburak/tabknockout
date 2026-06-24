namespace TapKnockout.Level
{
    public enum ChapterFlowState
    {
        Idle,
        PreparingRoom,
        RoomStarting,
        CombatRunning,
        RoomCompleted,
        RewardPending,
        AbilitySelectionPending,
        WaitingForContinue,
        TransitioningToNextRoom,
        ChapterCompleted,
        Failed
    }
}
