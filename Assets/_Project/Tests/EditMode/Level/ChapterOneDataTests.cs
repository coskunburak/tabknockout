using NUnit.Framework;
using TapKnockout.Level;
using TapKnockout.Room;
using UnityEditor;

namespace TapKnockout.Level.Tests
{
    public sealed class ChapterOneDataTests
    {
        private const string ChapterPath = "Assets/_Project/ScriptableObjects/Chapters/Chapter_01.asset";

        [Test]
        public void Chapter01_HasThirtyRoomsAndBossFinale_WhenGenerated()
        {
            var chapter = AssetDatabase.LoadAssetAtPath<ChapterConfig>(ChapterPath);
            if (chapter == null)
            {
                Assert.Ignore("Run Tools > Tap Knockout > Content > Create Chapter 1 Production Data before validating Chapter_01.");
            }

            Assert.That(chapter.Rooms.Count, Is.EqualTo(30));
            var finalRoom = chapter.Rooms[chapter.Rooms.Count - 1];
            Assert.That(finalRoom, Is.Not.Null);
            Assert.That(finalRoom.RoomType, Is.EqualTo(RoomType.Boss));
            Assert.That(finalRoom.RewardType, Is.EqualTo(RoomRewardType.BossClear));
        }

        [Test]
        public void Chapter01_HasRequiredSpecialRoomCounts_WhenGenerated()
        {
            var chapter = AssetDatabase.LoadAssetAtPath<ChapterConfig>(ChapterPath);
            if (chapter == null)
            {
                Assert.Ignore("Run Tools > Tap Knockout > Content > Create Chapter 1 Production Data before validating Chapter_01.");
            }

            var rewardRooms = 0;
            var supportRooms = 0;
            var bossRooms = 0;
            var miniBossRooms = 0;

            for (var i = 0; i < chapter.Rooms.Count; i++)
            {
                var room = chapter.Rooms[i];
                Assert.That(room, Is.Not.Null, $"Room {i + 1} is missing.");

                if (room.RoomType == RoomType.Reward)
                {
                    rewardRooms++;
                }

                if (room.RoomType == RoomType.Heal || room.RoomType == RoomType.Shop)
                {
                    supportRooms++;
                }

                if (room.RoomType == RoomType.Boss || room.RewardType == RoomRewardType.BossClear)
                {
                    bossRooms++;
                }

                if (room.RoomType == RoomType.Elite && room.RoomId.Contains("boss"))
                {
                    miniBossRooms++;
                }
            }

            Assert.That(bossRooms, Is.GreaterThanOrEqualTo(1));
            Assert.That(miniBossRooms, Is.GreaterThanOrEqualTo(1));
            Assert.That(rewardRooms, Is.GreaterThanOrEqualTo(3));
            Assert.That(supportRooms, Is.GreaterThanOrEqualTo(2));
        }
    }
}
