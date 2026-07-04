using System.Collections.Generic;
using TapKnockout.Level;
using TapKnockout.Room;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class ChapterDataValidator
    {
        private const string MenuPath = "Tools/Tap Knockout/Content/Validate Chapter Room Data";

        [MenuItem(MenuPath)]
        public static void ValidateChapterRoomData()
        {
            var chapter = AssetDatabase.LoadAssetAtPath<ChapterConfig>(ChapterOneContentBuilder.ChapterPath);
            if (chapter == null)
            {
                EditorUtility.DisplayDialog(
                    "Chapter Data Validator",
                    $"Missing {ChapterOneContentBuilder.ChapterPath}. Run Tools > Tap Knockout > Content > Create Chapter 1 Production Data.",
                    "OK");
                return;
            }

            var issues = ValidateChapter(chapter);
            for (var i = 0; i < issues.Count; i++)
            {
                Debug.LogWarning(issues[i], chapter);
            }

            EditorUtility.DisplayDialog(
                "Chapter Data Validator",
                issues.Count == 0
                    ? "Chapter room data passed validation."
                    : $"Chapter room data has {issues.Count} issue(s). Check Window > General > Console.",
                "OK");
        }

        public static List<string> ValidateChapter(ChapterConfig chapter)
        {
            var issues = new List<string>();
            if (chapter == null)
            {
                issues.Add("ChapterConfig is missing.");
                return issues;
            }

            if (chapter.Rooms == null || chapter.Rooms.Count != 30)
            {
                issues.Add($"Chapter_01 should contain 30 rooms; found {chapter.Rooms?.Count ?? 0}.");
            }

            var rewardRooms = 0;
            var supportRooms = 0;
            var bossRooms = 0;
            var eliteBossRooms = 0;

            for (var i = 0; chapter.Rooms != null && i < chapter.Rooms.Count; i++)
            {
                var room = chapter.Rooms[i];
                if (room == null)
                {
                    issues.Add($"Room {i + 1:00} is missing.");
                    continue;
                }

                if (room.RoomPrefab == null)
                {
                    issues.Add($"Room {i + 1:00} ({room.RoomId}) has no room prefab reference.");
                }
                else if (!room.TryGetRoomPrefabContract(out _))
                {
                    issues.Add($"Room {i + 1:00} ({room.RoomId}) prefab has no {nameof(RoomPrefabContract)}.");
                }

                if (room.RoomType == RoomType.Reward)
                {
                    rewardRooms++;
                }

                if (room.RoomType == RoomType.Heal || room.RoomType == RoomType.Shop)
                {
                    supportRooms++;
                }

                if (room.RoomType == RoomType.Boss || room.RewardType == RoomRewardType.BossClear || room.IsBossRoom)
                {
                    bossRooms++;
                }

                if (room.RoomType == RoomType.Elite && room.RoomId.Contains("boss"))
                {
                    eliteBossRooms++;
                }

                if ((room.RoomType == RoomType.Combat || room.RoomType == RoomType.Elite || room.RoomType == RoomType.Boss) && !room.HasWaves)
                {
                    issues.Add($"Combat-like room {i + 1:00} ({room.RoomId}) has no wave data.");
                }
            }

            if (chapter.Rooms != null && chapter.Rooms.Count > 0)
            {
                var finalRoom = chapter.Rooms[chapter.Rooms.Count - 1];
                if (finalRoom == null || finalRoom.RoomType != RoomType.Boss || finalRoom.RewardType != RoomRewardType.BossClear)
                {
                    issues.Add("Final room must be Boss with BossClear reward.");
                }
            }

            if (bossRooms < 1)
            {
                issues.Add("Chapter_01 should include at least one boss room.");
            }

            if (eliteBossRooms < 1)
            {
                issues.Add("Chapter_01 should include at least one mini-boss/elite-boss room.");
            }

            if (rewardRooms < 3)
            {
                issues.Add($"Chapter_01 should include at least 3 reward rooms; found {rewardRooms}.");
            }

            if (supportRooms < 2)
            {
                issues.Add($"Chapter_01 should include at least 2 heal/shop support rooms; found {supportRooms}.");
            }

            return issues;
        }
    }
}
