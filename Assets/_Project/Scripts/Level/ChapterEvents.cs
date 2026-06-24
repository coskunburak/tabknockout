using System;
using TapKnockout.Room;
using UnityEngine;

namespace TapKnockout.Level
{
    public readonly struct ChapterStartedEventArgs
    {
        public ChapterStartedEventArgs(ChapterRunner source, ChapterConfig chapterConfig)
        {
            Source = source;
            ChapterConfig = chapterConfig;
        }

        public ChapterRunner Source { get; }
        public ChapterConfig ChapterConfig { get; }
    }

    public readonly struct ChapterRoomChangedEventArgs
    {
        public ChapterRoomChangedEventArgs(ChapterRunner source, ChapterConfig chapterConfig, RoomTemplateConfig roomConfig, int roomIndex)
        {
            Source = source;
            ChapterConfig = chapterConfig;
            RoomConfig = roomConfig;
            RoomIndex = Mathf.Max(0, roomIndex);
        }

        public ChapterRunner Source { get; }
        public ChapterConfig ChapterConfig { get; }
        public RoomTemplateConfig RoomConfig { get; }
        public int RoomIndex { get; }
    }

    public readonly struct ChapterCompletedEventArgs
    {
        public ChapterCompletedEventArgs(ChapterRunner source, ChapterConfig chapterConfig, int completedRoomCount)
        {
            Source = source;
            ChapterConfig = chapterConfig;
            CompletedRoomCount = Mathf.Max(0, completedRoomCount);
        }

        public ChapterRunner Source { get; }
        public ChapterConfig ChapterConfig { get; }
        public int CompletedRoomCount { get; }
    }

    public readonly struct ChapterFailedEventArgs
    {
        public ChapterFailedEventArgs(
            ChapterRunner source,
            ChapterConfig chapterConfig,
            int completedRoomCount,
            string reason,
            GameObject playerObject)
        {
            Source = source;
            ChapterConfig = chapterConfig;
            CompletedRoomCount = Mathf.Max(0, completedRoomCount);
            Reason = string.IsNullOrWhiteSpace(reason) ? "unknown" : reason;
            PlayerObject = playerObject;
        }

        public ChapterRunner Source { get; }
        public ChapterConfig ChapterConfig { get; }
        public int CompletedRoomCount { get; }
        public string Reason { get; }
        public GameObject PlayerObject { get; }
    }

    public static class ChapterEvents
    {
        public static event Action<ChapterStartedEventArgs> OnChapterStarted;
        public static event Action<ChapterRoomChangedEventArgs> OnChapterRoomChanged;
        public static event Action<ChapterCompletedEventArgs> OnChapterCompleted;
        public static event Action<ChapterFailedEventArgs> OnChapterFailed;

        public static void RaiseChapterStarted(ChapterStartedEventArgs eventArgs)
        {
            OnChapterStarted?.Invoke(eventArgs);
        }

        public static void RaiseChapterRoomChanged(ChapterRoomChangedEventArgs eventArgs)
        {
            OnChapterRoomChanged?.Invoke(eventArgs);
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
