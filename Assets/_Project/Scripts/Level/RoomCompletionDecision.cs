using TapKnockout.Room;

namespace TapKnockout.Level
{
    public static class RoomCompletionDecision
    {
        public static RoomCompletionDecisionResult Evaluate(
            RoomTemplateConfig roomConfig,
            int roomIndex,
            int totalRoomCount,
            bool isPlayerAlive = true)
        {
            var rewardType = ResolveRewardType(roomConfig);
            if (!isPlayerAlive)
            {
                return new RoomCompletionDecisionResult(false, true, false, false, false, rewardType);
            }

            var isFinalRoom = totalRoomCount > 0 && roomIndex >= totalRoomCount - 1;
            var isBossClear = rewardType == RoomRewardType.BossClear || roomConfig != null && roomConfig.IsBossRoom;
            if (isFinalRoom || isBossClear)
            {
                return new RoomCompletionDecisionResult(true, false, false, false, false, rewardType);
            }

            if (rewardType == RoomRewardType.Ability)
            {
                return new RoomCompletionDecisionResult(false, false, true, false, false, rewardType);
            }

            var autoAdvance = roomConfig == null || roomConfig.AutoAdvanceAfterClear;
            var shouldWaitForContinue = !autoAdvance || rewardType == RoomRewardType.Shop;
            return new RoomCompletionDecisionResult(
                false,
                false,
                false,
                shouldWaitForContinue,
                autoAdvance && !shouldWaitForContinue,
                rewardType);
        }

        private static RoomRewardType ResolveRewardType(RoomTemplateConfig roomConfig)
        {
            if (roomConfig == null)
            {
                return RoomRewardType.None;
            }

            if (roomConfig.GrantsAbilityReward)
            {
                return RoomRewardType.Ability;
            }

            if (roomConfig.GrantsHealReward)
            {
                return RoomRewardType.Heal;
            }

            if (roomConfig.RewardType != RoomRewardType.None)
            {
                return roomConfig.RewardType;
            }

            switch (roomConfig.RoomType)
            {
                case RoomType.AbilityReward:
                    return RoomRewardType.Ability;
                case RoomType.Heal:
                    return RoomRewardType.Heal;
                case RoomType.Shop:
                    return RoomRewardType.Shop;
                case RoomType.Reward:
                    return RoomRewardType.Currency;
                case RoomType.Boss:
                    return RoomRewardType.BossClear;
                default:
                    return roomConfig.IsBossRoom ? RoomRewardType.BossClear : RoomRewardType.None;
            }
        }
    }
}
