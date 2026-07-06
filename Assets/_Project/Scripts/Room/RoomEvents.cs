using System;
using UnityEngine;

namespace TapKnockout.Room
{
    public readonly struct RoomStartedEventArgs
    {
        public RoomStartedEventArgs(RoomManager source, RoomTemplateConfig roomConfig)
        {
            Source = source;
            RoomConfig = roomConfig;
        }

        public RoomManager Source { get; }
        public RoomTemplateConfig RoomConfig { get; }
    }

    public readonly struct RoomCompletedEventArgs
    {
        public RoomCompletedEventArgs(RoomManager source, RoomTemplateConfig roomConfig, int completedWaveCount)
        {
            Source = source;
            RoomConfig = roomConfig;
            CompletedWaveCount = Mathf.Max(0, completedWaveCount);
        }

        public RoomManager Source { get; }
        public RoomTemplateConfig RoomConfig { get; }
        public int CompletedWaveCount { get; }
    }

    public readonly struct RoomExitUnlockedEventArgs
    {
        public RoomExitUnlockedEventArgs(RoomManager source, RoomTemplateConfig roomConfig, RoomRewardType rewardType)
        {
            Source = source;
            RoomConfig = roomConfig;
            RewardType = rewardType;
        }

        public RoomManager Source { get; }
        public RoomTemplateConfig RoomConfig { get; }
        public RoomRewardType RewardType { get; }
    }

    public static class RoomEvents
    {
        public static event Action<RoomStartedEventArgs> OnRoomStarted;
        public static event Action<RoomCompletedEventArgs> OnRoomCompleted;
        public static event Action<RoomExitUnlockedEventArgs> OnRoomExitUnlocked;

        public static void RaiseRoomStarted(RoomStartedEventArgs eventArgs)
        {
            OnRoomStarted?.Invoke(eventArgs);
        }

        public static void RaiseRoomCompleted(RoomCompletedEventArgs eventArgs)
        {
            OnRoomCompleted?.Invoke(eventArgs);
        }

        public static void RaiseRoomExitUnlocked(RoomExitUnlockedEventArgs eventArgs)
        {
            OnRoomExitUnlocked?.Invoke(eventArgs);
        }
    }
}
