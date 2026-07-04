using NUnit.Framework;
using TapKnockout.Level;
using TapKnockout.Room;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Level.Tests
{
    public sealed class RoomCompletionDecisionSpecialRoomTests
    {
        [Test]
        public void ShopRoom_WaitsForContinue_WhenRewardTypeIsImplicit()
        {
            var room = CreateRoom(RoomType.Shop, RoomRewardType.None, true);

            try
            {
                var decision = RoomCompletionDecision.Evaluate(room, 0, 3);

                Assert.That(decision.RewardType, Is.EqualTo(RoomRewardType.Shop));
                Assert.That(decision.ShouldWaitForContinue, Is.True);
                Assert.That(decision.ShouldAutoAdvance, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(room);
            }
        }

        [Test]
        public void HealRoom_ResolvesHealReward()
        {
            var room = CreateRoom(RoomType.Heal, RoomRewardType.None, true);

            try
            {
                var decision = RoomCompletionDecision.Evaluate(room, 0, 3);

                Assert.That(decision.RewardType, Is.EqualTo(RoomRewardType.Heal));
                Assert.That(decision.ShouldCompleteChapter, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(room);
            }
        }

        [Test]
        public void RewardRoom_ResolvesCurrencyRewardPlaceholder()
        {
            var room = CreateRoom(RoomType.Reward, RoomRewardType.None, false);

            try
            {
                var decision = RoomCompletionDecision.Evaluate(room, 0, 3);

                Assert.That(decision.RewardType, Is.EqualTo(RoomRewardType.Currency));
                Assert.That(decision.ShouldWaitForContinue, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(room);
            }
        }

        private static RoomTemplateConfig CreateRoom(RoomType roomType, RoomRewardType rewardType, bool autoAdvance)
        {
            var room = ScriptableObject.CreateInstance<RoomTemplateConfig>();
            var serializedObject = new SerializedObject(room);
            serializedObject.FindProperty("roomType").enumValueIndex = (int)roomType;
            serializedObject.FindProperty("rewardType").enumValueIndex = (int)rewardType;
            serializedObject.FindProperty("autoAdvanceAfterClear").boolValue = autoAdvance;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return room;
        }
    }
}
