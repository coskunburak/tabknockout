using NUnit.Framework;
using TapKnockout.Room;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Room.Tests
{
    public sealed class RoomTemplateConfigRewardTests
    {
        [Test]
        public void AbilityRewardRoom_GrantsAbilityReward()
        {
            var config = ScriptableObject.CreateInstance<RoomTemplateConfig>();

            try
            {
                SetRoomType(config, RoomType.AbilityReward);

                Assert.That(config.GrantsAbilityReward, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void ExplicitHealReward_GrantsHealReward()
        {
            var config = ScriptableObject.CreateInstance<RoomTemplateConfig>();

            try
            {
                SetRewardType(config, RoomRewardType.Heal);

                Assert.That(config.GrantsHealReward, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        private static void SetRoomType(RoomTemplateConfig config, RoomType roomType)
        {
            var serializedObject = new SerializedObject(config);
            serializedObject.FindProperty("roomType").enumValueIndex = (int)roomType;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetRewardType(RoomTemplateConfig config, RoomRewardType rewardType)
        {
            var serializedObject = new SerializedObject(config);
            serializedObject.FindProperty("rewardType").enumValueIndex = (int)rewardType;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
