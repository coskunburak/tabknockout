using System.Collections.Generic;
using NUnit.Framework;
using TapKnockout.Room;
using UnityEditor;

namespace TapKnockout.Level.Tests
{
    public sealed class ChapterConfigSequenceTests
    {
        private const string ChapterPath = "Assets/_Project/ScriptableObjects/Chapters/Chapter_VerticalSlice_01.asset";

        [Test]
        public void GeneratedChapter_ContainsTenDistinctRooms()
        {
            var chapter = LoadChapter();

            Assert.That(chapter.Rooms.Count, Is.EqualTo(10));

            var distinctRooms = new HashSet<RoomTemplateConfig>();
            for (var i = 0; i < chapter.Rooms.Count; i++)
            {
                Assert.That(chapter.Rooms[i], Is.Not.Null, $"Room {i + 1} is missing.");
                distinctRooms.Add(chapter.Rooms[i]);
            }

            Assert.That(distinctRooms.Count, Is.EqualTo(10));
        }

        [Test]
        public void GeneratedChapter_RoomRewardSequence_IsExpected()
        {
            var chapter = LoadChapter();

            Assert.That(chapter.Rooms[0].RewardType, Is.EqualTo(RoomRewardType.Ability));
            Assert.That(chapter.Rooms[1].RewardType, Is.EqualTo(RoomRewardType.None));
            Assert.That(chapter.Rooms[2].RewardType, Is.EqualTo(RoomRewardType.Ability));
            Assert.That(chapter.Rooms[3].RoomType, Is.EqualTo(RoomType.Elite));
            Assert.That(chapter.Rooms[4].RewardType, Is.EqualTo(RoomRewardType.Heal));
            Assert.That(chapter.Rooms[5].RewardType, Is.EqualTo(RoomRewardType.Ability));
            Assert.That(chapter.Rooms[7].RewardType, Is.EqualTo(RoomRewardType.Ability));
            Assert.That(chapter.Rooms[9].RoomType, Is.EqualTo(RoomType.Boss));
            Assert.That(chapter.Rooms[9].RewardType, Is.EqualTo(RoomRewardType.BossClear));
        }

        [Test]
        public void GeneratedChapter_RoomCompletionDecisions_AreExpected()
        {
            var chapter = LoadChapter();

            var room1Decision = RoomCompletionDecision.Evaluate(chapter.Rooms[0], 0, chapter.Rooms.Count);
            Assert.That(room1Decision.ShouldOpenAbilitySelection, Is.True);
            Assert.That(room1Decision.ShouldWaitForContinue, Is.False);
            Assert.That(room1Decision.ShouldCompleteChapter, Is.False);

            var room2Decision = RoomCompletionDecision.Evaluate(chapter.Rooms[1], 1, chapter.Rooms.Count);
            Assert.That(room2Decision.ShouldOpenAbilitySelection, Is.False);
            Assert.That(room2Decision.ShouldWaitForContinue, Is.True);
            Assert.That(room2Decision.ShouldCompleteChapter, Is.False);

            var room3Decision = RoomCompletionDecision.Evaluate(chapter.Rooms[2], 2, chapter.Rooms.Count);
            Assert.That(room3Decision.ShouldOpenAbilitySelection, Is.True);
            Assert.That(room3Decision.ShouldCompleteChapter, Is.False);

            var room4Decision = RoomCompletionDecision.Evaluate(chapter.Rooms[3], 3, chapter.Rooms.Count);
            Assert.That(room4Decision.ShouldOpenAbilitySelection, Is.False);
            Assert.That(room4Decision.ShouldWaitForContinue, Is.True);
            Assert.That(room4Decision.ShouldCompleteChapter, Is.False);

            var room6Decision = RoomCompletionDecision.Evaluate(chapter.Rooms[5], 5, chapter.Rooms.Count);
            Assert.That(room6Decision.ShouldOpenAbilitySelection, Is.True);
            Assert.That(room6Decision.ShouldCompleteChapter, Is.False);

            var room10Decision = RoomCompletionDecision.Evaluate(chapter.Rooms[9], 9, chapter.Rooms.Count);
            Assert.That(room10Decision.ShouldCompleteChapter, Is.True);
            Assert.That(room10Decision.ShouldOpenAbilitySelection, Is.False);
            Assert.That(room10Decision.ShouldWaitForContinue, Is.False);
            Assert.That(room10Decision.ShouldAutoAdvance, Is.False);
        }

        private static ChapterConfig LoadChapter()
        {
            var chapter = AssetDatabase.LoadAssetAtPath<ChapterConfig>(ChapterPath);
            Assert.That(chapter, Is.Not.Null);
            return chapter;
        }
    }
}
