using System.IO;
using TapKnockout.Level;
using TapKnockout.Room;
using TapKnockout.Wave;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class EnemyBossPlaytestContentBuilder
    {
        private const string MenuPath = "Tools/Tap Knockout/Content/Create Enemy/Boss Playtest Chapter";
        private const string RoomFolder = "Assets/_Project/ScriptableObjects/Rooms/Playtest";
        private const string WaveFolder = "Assets/_Project/ScriptableObjects/Waves";
        private const string PlaytestWaveFolder = "Assets/_Project/ScriptableObjects/Waves/Playtest";
        private const string ChapterFolder = "Assets/_Project/ScriptableObjects/Chapters";
        private const string ChapterPath = "Assets/_Project/ScriptableObjects/Chapters/Chapter_Playtest_EnemiesBosses.asset";
        private const string GeneratedMeleePrefabPath = "Assets/_Project/Prefabs/Enemies/Generated/PF_Enemy_BasicMelee_GreenDemon_Generated.prefab";

        private static readonly PlaytestRoomSpec[] RoomSpecs =
        {
            new PlaytestRoomSpec("RoomTemplate_Playtest_01_MeleeChaser", "playtest_01_melee_chaser", RoomType.Combat, RoomRewardType.None, "Wave_Test_MeleeChaser", "PF_Room_KayKit_Combat_01"),
            new PlaytestRoomSpec("RoomTemplate_Playtest_02_FastCharger", "playtest_02_fast_charger", RoomType.Combat, RoomRewardType.None, "Wave_Test_FastCharger", "PF_Room_Medium_Combat"),
            new PlaytestRoomSpec("RoomTemplate_Playtest_03_RangedShooter", "playtest_03_ranged_shooter", RoomType.Combat, RoomRewardType.None, "Wave_Test_RangedShooter", "PF_Room_Wide_Combat"),
            new PlaytestRoomSpec("RoomTemplate_Playtest_04_AreaBomber", "playtest_04_area_bomber", RoomType.Combat, RoomRewardType.None, "Wave_Test_AreaBomber", "PF_Room_Hazard_Placeholder"),
            new PlaytestRoomSpec("RoomTemplate_Playtest_05_ShieldEnemy", "playtest_05_shield_enemy", RoomType.Combat, RoomRewardType.None, "Wave_Test_ShieldEnemy", "PF_Room_Medium_Combat"),
            new PlaytestRoomSpec("RoomTemplate_Playtest_06_SplitterEnemy", "playtest_06_splitter_enemy", RoomType.Combat, RoomRewardType.None, "Wave_Test_SplitterEnemy", "PF_Room_Small_Combat"),
            new PlaytestRoomSpec("RoomTemplate_Playtest_07_EliteChaser", "playtest_07_elite_chaser", RoomType.Elite, RoomRewardType.Ability, "Wave_Test_EliteChaser", "PF_Room_Elite"),
            new PlaytestRoomSpec("RoomTemplate_Playtest_08_EliteRanged", "playtest_08_elite_ranged", RoomType.Elite, RoomRewardType.Ability, "Wave_Test_EliteRanged", "PF_Room_Elite"),
            new PlaytestRoomSpec("RoomTemplate_Playtest_09_MixedEnemies", "playtest_09_mixed_enemies", RoomType.Combat, RoomRewardType.None, "Wave_Playtest_09_MixedEnemies", "PF_Room_Wide_Combat", true),
            new PlaytestRoomSpec("RoomTemplate_Playtest_10_BossDashCounterBrute", "playtest_10_boss_dash_counter_brute", RoomType.Boss, RoomRewardType.BossClear, "Wave_Test_Boss1_DashCounterBrute", "PF_Room_Boss")
        };

        [MenuItem(MenuPath)]
        public static void CreateEnemyBossPlaytestChapter()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Enemy/Boss Playtest Chapter", "Exit Play Mode before creating content assets.", "OK");
                return;
            }

            CreateOrUpdatePlaytestChapter();
            EditorUtility.DisplayDialog("Enemy/Boss Playtest Chapter", $"Playtest chapter ready:\n\n{ChapterPath}", "OK");
        }

        public static ChapterConfig CreateOrUpdatePlaytestChapter()
        {
            EnsureFolder(RoomFolder);
            EnsureFolder(PlaytestWaveFolder);
            EnsureFolder(ChapterFolder);
            EnemyBossPrefabReferenceRepairTool.RepairAllReferences();
            CreateOrUpdateMixedWave();

            var rooms = new RoomTemplateConfig[RoomSpecs.Length];
            for (var i = 0; i < RoomSpecs.Length; i++)
            {
                rooms[i] = CreateOrUpdateRoom(RoomSpecs[i], i);
            }

            var chapter = AssetDatabase.LoadAssetAtPath<ChapterConfig>(ChapterPath);
            if (chapter == null)
            {
                chapter = ScriptableObject.CreateInstance<ChapterConfig>();
                AssetDatabase.CreateAsset(chapter, ChapterPath);
            }

            var serializedObject = new SerializedObject(chapter);
            SetString(serializedObject, "chapterId", "chapter_playtest_enemies_bosses");
            SetString(serializedObject, "displayName", "Enemy/Boss Playtest");
            SetInt(serializedObject, "chapterIndex", 9001);
            SetInt(serializedObject, "recommendedPower", 1);
            var roomList = serializedObject.FindProperty("rooms");
            roomList.arraySize = rooms.Length;
            for (var i = 0; i < rooms.Length; i++)
            {
                roomList.GetArrayElementAtIndex(i).objectReferenceValue = rooms[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(chapter);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{nameof(EnemyBossPlaytestContentBuilder)} Done: created/updated {ChapterPath} with {rooms.Length} rooms.", chapter);
            return chapter;
        }

        private static RoomTemplateConfig CreateOrUpdateRoom(PlaytestRoomSpec spec, int index)
        {
            var path = $"{RoomFolder}/{spec.AssetName}.asset";
            var room = AssetDatabase.LoadAssetAtPath<RoomTemplateConfig>(path);
            if (room == null)
            {
                room = ScriptableObject.CreateInstance<RoomTemplateConfig>();
                AssetDatabase.CreateAsset(room, path);
            }

            var wave = ResolveWave(spec);
            var roomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Prefabs/Rooms/{spec.RoomPrefabName}.prefab");
            var serializedObject = new SerializedObject(room);
            SetString(serializedObject, "roomId", spec.RoomId);
            SetEnum(serializedObject, "roomType", (int)spec.RoomType);
            SetFloat(serializedObject, "startDelay", 0.35f);
            SetBool(serializedObject, "lockExitsUntilCleared", spec.RoomType != RoomType.Reward && spec.RoomType != RoomType.Heal && spec.RoomType != RoomType.Shop);
            SetObject(serializedObject, "roomPrefab", roomPrefab);
            SetEnum(serializedObject, "rewardType", (int)spec.RewardType);
            SetBool(serializedObject, "autoAdvanceAfterClear", true);
            SetBool(serializedObject, "grantsAbilityReward", spec.RewardType == RoomRewardType.Ability);
            SetBool(serializedObject, "grantsHealReward", spec.RewardType == RoomRewardType.Heal);
            SetBool(serializedObject, "isBossRoom", spec.RoomType == RoomType.Boss);
            SetString(serializedObject, "environmentThemeId", "theme_playtest_01");
            var waves = serializedObject.FindProperty("waves");
            waves.arraySize = 1;
            waves.GetArrayElementAtIndex(0).objectReferenceValue = wave;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(room);
            Debug.Log($"{nameof(EnemyBossPlaytestContentBuilder)} Done: room {index + 1:00} {path}.", room);
            return room;
        }

        private static WaveConfig ResolveWave(PlaytestRoomSpec spec)
        {
            var folder = spec.IsPlaytestWave ? PlaytestWaveFolder : WaveFolder;
            return AssetDatabase.LoadAssetAtPath<WaveConfig>($"{folder}/{spec.WaveAssetName}.asset");
        }

        private static void CreateOrUpdateMixedWave()
        {
            var path = $"{PlaytestWaveFolder}/Wave_Playtest_09_MixedEnemies.asset";
            var wave = AssetDatabase.LoadAssetAtPath<WaveConfig>(path);
            if (wave == null)
            {
                wave = ScriptableObject.CreateInstance<WaveConfig>();
                AssetDatabase.CreateAsset(wave, path);
            }

            var serializedObject = new SerializedObject(wave);
            SetString(serializedObject, "waveId", "wave_playtest_09_mixed_enemies");
            SetFloat(serializedObject, "startDelay", 0.35f);
            SetBool(serializedObject, "completeWhenAllSpawnedEnemiesDead", true);
            var enemies = serializedObject.FindProperty("enemies");
            enemies.arraySize = 4;
            SetWaveEntry(enemies.GetArrayElementAtIndex(0), "EnemyConfig_MeleeChaser", "PF_Enemy_MeleeChaser", 2, 0.25f, -1);
            SetWaveEntry(enemies.GetArrayElementAtIndex(1), "EnemyConfig_FastCharger", "PF_Enemy_FastCharger", 1, 0.5f, -1);
            SetWaveEntry(enemies.GetArrayElementAtIndex(2), "EnemyConfig_RangedShooter", "PF_Enemy_RangedShooter", 2, 0.4f, -1);
            SetWaveEntry(enemies.GetArrayElementAtIndex(3), "EnemyConfig_ShieldEnemy", "PF_Enemy_ShieldEnemy", 1, 0.4f, -1);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(wave);
        }

        private static void SetWaveEntry(SerializedProperty entry, string configName, string prefabName, int count, float delay, int spawnIndex)
        {
            entry.FindPropertyRelative("enemyConfig").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>($"Assets/_Project/ScriptableObjects/Enemies/{configName}.asset");
            entry.FindPropertyRelative("enemyPrefab").objectReferenceValue =
                ResolveEnemyPrefab(prefabName);
            entry.FindPropertyRelative("count").intValue = count;
            entry.FindPropertyRelative("spawnDelay").floatValue = delay;
            entry.FindPropertyRelative("spawnPointIndex").intValue = spawnIndex;
        }

        private static Object ResolveEnemyPrefab(string prefabName)
        {
            if (prefabName == "PF_Enemy_MeleeChaser")
            {
                var generatedMeleePrefab = AssetDatabase.LoadAssetAtPath<Object>(GeneratedMeleePrefabPath);
                if (generatedMeleePrefab != null)
                {
                    return generatedMeleePrefab;
                }
            }

            return AssetDatabase.LoadAssetAtPath<Object>($"Assets/_Project/Prefabs/Enemies/{prefabName}.prefab");
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent ?? "Assets", Path.GetFileName(folderPath));
        }

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.stringValue = value;
            }
        }

        private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        private static void SetEnum(SerializedObject serializedObject, string propertyName, int value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.enumValueIndex = value;
            }
        }

        private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private readonly struct PlaytestRoomSpec
        {
            public PlaytestRoomSpec(string assetName, string roomId, RoomType roomType, RoomRewardType rewardType, string waveAssetName, string roomPrefabName, bool isPlaytestWave = false)
            {
                AssetName = assetName;
                RoomId = roomId;
                RoomType = roomType;
                RewardType = rewardType;
                WaveAssetName = waveAssetName;
                RoomPrefabName = roomPrefabName;
                IsPlaytestWave = isPlaytestWave;
            }

            public string AssetName { get; }
            public string RoomId { get; }
            public RoomType RoomType { get; }
            public RoomRewardType RewardType { get; }
            public string WaveAssetName { get; }
            public string RoomPrefabName { get; }
            public bool IsPlaytestWave { get; }
        }
    }
}
