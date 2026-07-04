using NUnit.Framework;
using UnityEditor;

namespace TapKnockout.Room.Tests
{
    public sealed class RoomRewardRuleTests
    {
        [Test]
        public void GeneratedRooms_HaveExpectedRewardRules()
        {
            var room1 = Load("RoomTemplate_VS_01_AbilityIntro");
            var room2 = Load("RoomTemplate_VS_02_Combat");
            var room3 = Load("RoomTemplate_VS_03_AbilityReward");
            var room4 = Load("RoomTemplate_VS_04_Elite");
            var room5 = Load("RoomTemplate_VS_05_RecoveryPlaceholder");
            var room6 = Load("RoomTemplate_VS_06_CombatPressureAbility");
            var room10 = Load("RoomTemplate_VS_10_BossPlaceholder");

            Assert.That(room1.GrantsAbilityReward, Is.True);
            Assert.That(room1.AutoAdvanceAfterClear, Is.False);

            Assert.That(room2.RewardType, Is.EqualTo(RoomRewardType.None));
            Assert.That(room2.GrantsAbilityReward, Is.False);
            Assert.That(room2.AutoAdvanceAfterClear, Is.False);

            Assert.That(room3.RoomType, Is.EqualTo(RoomType.AbilityReward));
            Assert.That(room3.GrantsAbilityReward, Is.True);
            Assert.That(room3.AutoAdvanceAfterClear, Is.False);

            Assert.That(room4.RoomType, Is.EqualTo(RoomType.Elite));
            Assert.That(room4.RewardType, Is.EqualTo(RoomRewardType.None));
            Assert.That(room4.GrantsAbilityReward, Is.False);

            Assert.That(room5.GrantsHealReward, Is.True);
            Assert.That(room5.RewardType, Is.EqualTo(RoomRewardType.Heal));

            Assert.That(room6.GrantsAbilityReward, Is.True);
            Assert.That(room6.RewardType, Is.EqualTo(RoomRewardType.Ability));

            Assert.That(room10.IsBossRoom, Is.True);
            Assert.That(room10.RewardType, Is.EqualTo(RoomRewardType.BossClear));
            Assert.That(room10.GrantsAbilityReward, Is.False);
        }

        [Test]
        public void GeneratedRooms_AllReferenceOneWave()
        {
            Assert.That(Load("RoomTemplate_VS_01_AbilityIntro").Waves.Count, Is.EqualTo(1));
            Assert.That(Load("RoomTemplate_VS_02_Combat").Waves.Count, Is.EqualTo(1));
            Assert.That(Load("RoomTemplate_VS_03_AbilityReward").Waves.Count, Is.EqualTo(1));
            Assert.That(Load("RoomTemplate_VS_04_Elite").Waves.Count, Is.EqualTo(1));
            Assert.That(Load("RoomTemplate_VS_05_RecoveryPlaceholder").Waves.Count, Is.EqualTo(1));
            Assert.That(Load("RoomTemplate_VS_06_CombatPressureAbility").Waves.Count, Is.EqualTo(1));
            Assert.That(Load("RoomTemplate_VS_07_RangedPressure").Waves.Count, Is.EqualTo(1));
            Assert.That(Load("RoomTemplate_VS_08_EliteAbility").Waves.Count, Is.EqualTo(1));
            Assert.That(Load("RoomTemplate_VS_09_CombatPressure").Waves.Count, Is.EqualTo(1));
            Assert.That(Load("RoomTemplate_VS_10_BossPlaceholder").Waves.Count, Is.EqualTo(1));
        }

        private static RoomTemplateConfig Load(string assetName)
        {
            var config = AssetDatabase.LoadAssetAtPath<RoomTemplateConfig>(
                "Assets/_Project/ScriptableObjects/Rooms/" + assetName + ".asset");
            Assert.That(config, Is.Not.Null);
            return config;
        }
    }
}
