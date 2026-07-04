using NUnit.Framework;
using TapKnockout.Room;
using UnityEditor;

namespace TapKnockout.Level.Tests
{
    public sealed class EnemyBossPlaytestChapterTests
    {
        private const string ChapterPath = "Assets/_Project/ScriptableObjects/Chapters/Chapter_Playtest_EnemiesBosses.asset";

        [Test]
        public void PlaytestChapter_HasTenRoomsAndBossFinalRoom_WhenGenerated()
        {
            var chapter = AssetDatabase.LoadAssetAtPath<ChapterConfig>(ChapterPath);

            Assert.That(chapter, Is.Not.Null, $"Missing generated playtest chapter at {ChapterPath}.");
            Assert.That(chapter.Rooms, Has.Count.EqualTo(10));
            Assert.That(chapter.Rooms[chapter.Rooms.Count - 1].RoomType, Is.EqualTo(RoomType.Boss));
            Assert.That(chapter.Rooms[chapter.Rooms.Count - 1].RewardType, Is.EqualTo(RoomRewardType.BossClear));
        }

        [Test]
        public void PlaytestChapter_AllRoomsHavePrefabAndWave_WhenGenerated()
        {
            var chapter = AssetDatabase.LoadAssetAtPath<ChapterConfig>(ChapterPath);

            Assert.That(chapter, Is.Not.Null, $"Missing generated playtest chapter at {ChapterPath}.");
            for (var i = 0; i < chapter.Rooms.Count; i++)
            {
                Assert.That(chapter.Rooms[i], Is.Not.Null, $"Room {i + 1:00} is null.");
                Assert.That(chapter.Rooms[i].RoomPrefab, Is.Not.Null, $"Room {i + 1:00} has no room prefab.");
                Assert.That(chapter.Rooms[i].Waves, Is.Not.Empty, $"Room {i + 1:00} has no wave.");
                Assert.That(chapter.Rooms[i].Waves[0], Is.Not.Null, $"Room {i + 1:00} wave reference is null.");
            }
        }
    }
}
