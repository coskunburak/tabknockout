using System;
using TapKnockout.Room;

namespace TapKnockout.Level
{
    public readonly struct ChapterRoomProgressionEventArgs
    {
        public ChapterRoomProgressionEventArgs(
            ChapterRunner source,
            RoomTemplateConfig roomConfig,
            int roomIndex,
            RoomRewardType rewardType)
        {
            Source = source;
            RoomConfig = roomConfig;
            RoomIndex = roomIndex;
            RewardType = rewardType;
            TotalRoomCount = source != null ? source.TotalRoomCount : 0;
            NextRoomIndex = roomIndex + 1;
            ChapterId = source != null && source.CurrentChapter != null ? source.CurrentChapter.ChapterId : string.Empty;
            RoomId = roomConfig != null ? roomConfig.RoomId : string.Empty;
        }

        public ChapterRunner Source { get; }
        public RoomTemplateConfig RoomConfig { get; }
        public int RoomIndex { get; }
        public RoomRewardType RewardType { get; }
        public int TotalRoomCount { get; }
        public int NextRoomIndex { get; }
        public string ChapterId { get; }
        public string RoomId { get; }
    }

    public readonly struct ChapterRoomTransitionEventArgs
    {
        public ChapterRoomTransitionEventArgs(
            ChapterRunner source,
            RoomTemplateConfig fromRoomConfig,
            int fromRoomIndex,
            int toRoomIndex)
        {
            Source = source;
            FromRoomConfig = fromRoomConfig;
            FromRoomIndex = fromRoomIndex;
            ToRoomIndex = toRoomIndex;
            TotalRoomCount = source != null ? source.TotalRoomCount : 0;
            ChapterId = source != null && source.CurrentChapter != null ? source.CurrentChapter.ChapterId : string.Empty;
            FromRoomId = fromRoomConfig != null ? fromRoomConfig.RoomId : string.Empty;
        }

        public ChapterRunner Source { get; }
        public RoomTemplateConfig FromRoomConfig { get; }
        public int FromRoomIndex { get; }
        public int ToRoomIndex { get; }
        public int TotalRoomCount { get; }
        public string ChapterId { get; }
        public string FromRoomId { get; }
    }

    public static class ChapterProgressionEvents
    {
        public static event Action<ChapterRoomProgressionEventArgs> OnRoomExitUnlocked;
        public static event Action<ChapterRoomProgressionEventArgs> OnNextRoomRequested;
        public static event Action<ChapterRoomTransitionEventArgs> OnRoomTransitionStarted;
        public static event Action<ChapterRoomTransitionEventArgs> OnRoomTransitionCompleted;
        public static event Action<ChapterCompletedEventArgs> OnChapterCompleted;
        public static event Action<ChapterFailedEventArgs> OnChapterFailed;

        public static void RaiseRoomExitUnlocked(ChapterRoomProgressionEventArgs eventArgs)
        {
            OnRoomExitUnlocked?.Invoke(eventArgs);
        }

        public static void RaiseNextRoomRequested(ChapterRoomProgressionEventArgs eventArgs)
        {
            OnNextRoomRequested?.Invoke(eventArgs);
        }

        public static void RaiseRoomTransitionStarted(ChapterRoomTransitionEventArgs eventArgs)
        {
            OnRoomTransitionStarted?.Invoke(eventArgs);
        }

        public static void RaiseRoomTransitionCompleted(ChapterRoomTransitionEventArgs eventArgs)
        {
            OnRoomTransitionCompleted?.Invoke(eventArgs);
        }

        public static void RaiseChapterCompleted(ChapterCompletedEventArgs eventArgs)
        {
            OnChapterCompleted?.Invoke(eventArgs);
        }

        public static void RaiseChapterFailed(ChapterFailedEventArgs eventArgs)
        {
            OnChapterFailed?.Invoke(eventArgs);
        }
    }
}
