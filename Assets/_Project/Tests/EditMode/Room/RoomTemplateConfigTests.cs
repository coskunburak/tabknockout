using NUnit.Framework;
using TapKnockout.Room;
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
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }
    }
}
