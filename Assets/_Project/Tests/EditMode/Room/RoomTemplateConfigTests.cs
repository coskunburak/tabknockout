using NUnit.Framework;
using TapKnockout.Room;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Room.Tests
{
    public sealed class RoomTemplateConfigTests
    {
        [Test]
        public void DefaultRoomTemplateConfigValues_AreSafe()
        {
            var config = ScriptableObject.CreateInstance<RoomTemplateConfig>();

            try
            {
                Assert.That(config.RoomId, Is.Not.Empty);
                Assert.That(config.RoomType, Is.EqualTo(RoomType.Combat));
                Assert.That(config.Waves, Is.Not.Null);
                Assert.That(config.StartDelay, Is.GreaterThanOrEqualTo(0f));
                Assert.That(config.LockExitsUntilCleared, Is.True);
                Assert.That(config.RewardType, Is.EqualTo(RoomRewardType.None));
                Assert.That(config.AutoAdvanceAfterClear, Is.True);
                Assert.That(config.GrantsAbilityReward, Is.False);
                Assert.That(config.GrantsHealReward, Is.False);
                Assert.That(config.EnvironmentThemeId, Is.Not.Empty);
                Assert.That(config.RoomPrefab, Is.Null);
                Assert.That(config.HasRoomPrefab, Is.False);
                Assert.That(config.HasValidRoomPrefabReference(), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void RoomTemplateConfig_CanReferenceRoomPrefabContract()
        {
            var config = ScriptableObject.CreateInstance<RoomTemplateConfig>();
            var prefab = new GameObject("RoomPrefab");

            try
            {
                prefab.AddComponent<RoomPrefabContract>();
                var serializedObject = new SerializedObject(config);
                serializedObject.FindProperty("roomPrefab").objectReferenceValue = prefab;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();

                Assert.That(config.RoomPrefab, Is.EqualTo(prefab));
                Assert.That(config.HasRoomPrefab, Is.True);
                Assert.That(config.TryGetRoomPrefabContract(out var contract), Is.True);
                Assert.That(contract, Is.Not.Null);
                Assert.That(config.HasValidRoomPrefabReference(), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(prefab);
            }
        }
    }
}
