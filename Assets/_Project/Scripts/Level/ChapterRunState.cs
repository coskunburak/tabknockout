using System;

namespace TapKnockout.Level
{
    [Serializable]
    public sealed class ChapterRunState
    {
        public bool IsRunActive { get; private set; }
        public int CurrentRoomIndex { get; private set; } = -1;
        public int TotalRoomCount { get; private set; }
        public bool CurrentRoomCompleted { get; private set; }
        public bool IsRewardPending { get; private set; }
        public bool IsAbilitySelectionPending { get; private set; }
        public bool IsWaitingForContinue { get; private set; }
        public bool IsTransitioning { get; private set; }
        public bool IsChapterCompleted { get; private set; }
        public bool IsChapterFailed { get; private set; }
        public bool IsBossRoom { get; private set; }
        public bool IsLastRoom => TotalRoomCount > 0 && CurrentRoomIndex >= TotalRoomCount - 1;

        public void StartRun(int totalRooms)
        {
            ResetRun();
            TotalRoomCount = Math.Max(0, totalRooms);
            IsRunActive = true;
        }

        public void MarkRoomStarted(int roomIndex)
        {
            MarkRoomStarted(roomIndex, false);
        }

        public void MarkRoomStarted(int roomIndex, bool isBossRoom)
        {
            CurrentRoomIndex = Math.Max(0, roomIndex);
            CurrentRoomCompleted = false;
            IsRewardPending = false;
            IsAbilitySelectionPending = false;
            IsWaitingForContinue = false;
            IsTransitioning = false;
            IsBossRoom = isBossRoom;
        }

        public void MarkRoomCompleted()
        {
            CurrentRoomCompleted = true;
            IsTransitioning = false;
        }

        public void MarkRewardPending()
        {
            IsRewardPending = true;
            IsTransitioning = false;
        }

        public void MarkAbilitySelectionPending()
        {
            MarkRewardPending();
            IsAbilitySelectionPending = true;
            IsWaitingForContinue = false;
        }

        public void MarkWaitingForContinue()
        {
            MarkRewardPending();
            IsAbilitySelectionPending = false;
            IsWaitingForContinue = true;
        }

        public void MarkTransitioning()
        {
            IsRewardPending = false;
            IsAbilitySelectionPending = false;
            IsWaitingForContinue = false;
            IsTransitioning = true;
        }

        public void ClearRewardState()
        {
            IsRewardPending = false;
            IsAbilitySelectionPending = false;
            IsWaitingForContinue = false;
        }

        public void MarkChapterCompleted()
        {
            IsRunActive = false;
            IsChapterCompleted = true;
            IsChapterFailed = false;
            ClearRewardState();
            IsTransitioning = false;
        }

        public void MarkChapterFailed()
        {
            IsRunActive = false;
            IsChapterCompleted = false;
            IsChapterFailed = true;
            ClearRewardState();
            IsTransitioning = false;
        }

        public void AdvanceRoomIndex()
        {
            CurrentRoomIndex = Math.Min(CurrentRoomIndex + 1, Math.Max(0, TotalRoomCount - 1));
            CurrentRoomCompleted = false;
            IsBossRoom = false;
        }

        public void ResetRun()
        {
            IsRunActive = false;
            CurrentRoomIndex = -1;
            TotalRoomCount = 0;
            CurrentRoomCompleted = false;
            IsRewardPending = false;
            IsAbilitySelectionPending = false;
            IsWaitingForContinue = false;
            IsTransitioning = false;
            IsChapterCompleted = false;
            IsChapterFailed = false;
            IsBossRoom = false;
        }
    }
}
