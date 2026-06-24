using NUnit.Framework;
using TapKnockout.Level;
using TapKnockout.Room;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Level.Tests
{
    public sealed class RoomCompletionDecisionTests
    {
        [Test]
        public void CombatRoomWithNoReward_AutoAdvances()
        {
            var room = ScriptableObject.CreateInstance<RoomTemplateConfig>();

            try
            {
                var decision = RoomCompletionDecision.Evaluate(room, 0, 2);

                Assert.That(decision.ShouldAutoAdvance, Is.True);
                Assert.That(decision.ShouldOpenAbilitySelection, Is.False);
                Assert.That(decision.ShouldCompleteChapter, Is.False);
                Assert.That(decision.RewardType, Is.EqualTo(RoomRewardType.None));
            }
            finally
            {
                Object.DestroyImmediate(room);
            }
        }

        [Test]
        public void AbilityRewardRoom_OpensAbilitySelectionAndDoesNotAutoAdvance()
        {
            var room = ScriptableObject.CreateInstance<RoomTemplateConfig>();

            try
            {
                SetRewardType(room, RoomRewardType.Ability);

                var decision = RoomCompletionDecision.Evaluate(room, 0, 2);

                Assert.That(decision.ShouldOpenAbilitySelection, Is.True);
                Assert.That(decision.ShouldAutoAdvance, Is.False);
                Assert.That(decision.ShouldCompleteChapter, Is.False);
                Assert.That(decision.RewardType, Is.EqualTo(RoomRewardType.Ability));
            }
            finally
            {
                Object.DestroyImmediate(room);
            }
        }

        [Test]
        public void LastRoom_CompletesChapter()
        {
            var room = ScriptableObject.CreateInstance<RoomTemplateConfig>();

            try
            {
                var decision = RoomCompletionDecision.Evaluate(room, 1, 2);

                Assert.That(decision.ShouldCompleteChapter, Is.True);
                Assert.That(decision.ShouldAutoAdvance, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(room);
            }
        }

        [Test]
        public void BossClearReward_CompletesChapter()
        {
            var room = ScriptableObject.CreateInstance<RoomTemplateConfig>();

            try
            {
                SetRewardType(room, RoomRewardType.BossClear);

                var decision = RoomCompletionDecision.Evaluate(room, 0, 3);

                Assert.That(decision.ShouldCompleteChapter, Is.True);
                Assert.That(decision.RewardType, Is.EqualTo(RoomRewardType.BossClear));
            }
            finally
            {
                Object.DestroyImmediate(room);
            }
        }

        [Test]
        public void DeadPlayer_FailsChapter()
        {
            var room = ScriptableObject.CreateInstance<RoomTemplateConfig>();

            try
            {
                var decision = RoomCompletionDecision.Evaluate(room, 0, 2, false);

                Assert.That(decision.ShouldFailChapter, Is.True);
                Assert.That(decision.ShouldAutoAdvance, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(room);
            }
        }

        private static void SetRewardType(RoomTemplateConfig room, RoomRewardType rewardType)
        {
            var serializedObject = new SerializedObject(room);
            serializedObject.FindProperty("rewardType").enumValueIndex = (int)rewardType;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
