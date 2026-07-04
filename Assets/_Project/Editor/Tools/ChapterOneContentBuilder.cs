using System;
using System.Collections.Generic;
using System.IO;
using TapKnockout.Enemy;
using TapKnockout.Level;
using TapKnockout.Room;
using TapKnockout.Wave;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class ChapterOneContentBuilder
    {
        public const string ChapterPath = "Assets/_Project/ScriptableObjects/Chapters/Chapter_01.asset";

        private const string MenuPath = "Tools/Tap Knockout/Content/Create Chapter 1 Production Data";
        private const string RoomFolder = "Assets/_Project/ScriptableObjects/Rooms/Chapter01";
        private const string WaveFolder = "Assets/_Project/ScriptableObjects/Waves/Chapter01";
        private const string EnemyFolder = "Assets/_Project/ScriptableObjects/Enemies/Chapter01";
        private const string RoomPrefabFolder = "Assets/_Project/Prefabs/Rooms";

        private static readonly ChapterRoomSpec[] Rooms =
        {
            new ChapterRoomSpec(1, "combat_small_ability", RoomType.Combat, RoomRewardType.Ability, "PF_Room_Small_Combat", true, false, 3),
            new ChapterRoomSpec(2, "combat_medium", RoomType.Combat, RoomRewardType.None, "PF_Room_Medium_Combat", true, false, 5),
            new ChapterRoomSpec(3, "combat_wide", RoomType.Combat, RoomRewardType.None, "PF_Room_Wide_Combat", true, false, 5),
            new ChapterRoomSpec(4, "reward_ability", RoomType.Reward, RoomRewardType.Ability, "PF_Room_Reward", false, false, 0),
            new ChapterRoomSpec(5, "combat_hazard", RoomType.Combat, RoomRewardType.None, "PF_Room_Hazard_Placeholder", true, false, 6),
            new ChapterRoomSpec(6, "elite_ability", RoomType.Elite, RoomRewardType.Ability, "PF_Room_Elite", true, false, 6),
            new ChapterRoomSpec(7, "combat_medium_pressure", RoomType.Combat, RoomRewardType.None, "PF_Room_Medium_Combat", true, false, 6),
            new ChapterRoomSpec(8, "heal_placeholder", RoomType.Heal, RoomRewardType.Heal, "PF_Room_Heal", false, false, 0),
            new ChapterRoomSpec(9, "combat_wide_pressure", RoomType.Combat, RoomRewardType.None, "PF_Room_Wide_Combat", true, false, 7),
            new ChapterRoomSpec(10, "mini_boss_elite_reward", RoomType.Elite, RoomRewardType.Currency, "PF_Room_Elite", true, false, 1),
            new ChapterRoomSpec(11, "combat_small", RoomType.Combat, RoomRewardType.None, "PF_Room_Small_Combat", true, false, 6),
            new ChapterRoomSpec(12, "combat_medium_ability", RoomType.Combat, RoomRewardType.Ability, "PF_Room_Medium_Combat", true, false, 7),
            new ChapterRoomSpec(13, "reward_placeholder", RoomType.Reward, RoomRewardType.Currency, "PF_Room_Reward", false, false, 0),
            new ChapterRoomSpec(14, "combat_hazard_mid", RoomType.Combat, RoomRewardType.None, "PF_Room_Hazard_Placeholder", true, false, 7),
            new ChapterRoomSpec(15, "elite_mid", RoomType.Elite, RoomRewardType.None, "PF_Room_Elite", true, false, 7),
            new ChapterRoomSpec(16, "combat_wide_ability", RoomType.Combat, RoomRewardType.Ability, "PF_Room_Wide_Combat", true, false, 8),
            new ChapterRoomSpec(17, "shop_placeholder", RoomType.Shop, RoomRewardType.Shop, "PF_Room_ShopPlaceholder", false, false, 0),
            new ChapterRoomSpec(18, "combat_medium_late", RoomType.Combat, RoomRewardType.None, "PF_Room_Medium_Combat", true, false, 8),
            new ChapterRoomSpec(19, "combat_pressure", RoomType.Combat, RoomRewardType.None, "PF_Room_Medium_Combat", true, false, 8),
            new ChapterRoomSpec(20, "elite_ability_late", RoomType.Elite, RoomRewardType.Ability, "PF_Room_Elite", true, false, 8),
            new ChapterRoomSpec(21, "reward_late", RoomType.Reward, RoomRewardType.Currency, "PF_Room_Reward", false, false, 0),
            new ChapterRoomSpec(22, "combat_hazard_late", RoomType.Combat, RoomRewardType.None, "PF_Room_Hazard_Placeholder", true, false, 9),
            new ChapterRoomSpec(23, "combat_wide_late", RoomType.Combat, RoomRewardType.None, "PF_Room_Wide_Combat", true, false, 9),
            new ChapterRoomSpec(24, "heal_late", RoomType.Heal, RoomRewardType.Heal, "PF_Room_Heal", false, false, 0),
            new ChapterRoomSpec(25, "combat_pressure_ability", RoomType.Combat, RoomRewardType.Ability, "PF_Room_Medium_Combat", true, false, 10),
            new ChapterRoomSpec(26, "elite_final_arc", RoomType.Elite, RoomRewardType.None, "PF_Room_Elite", true, false, 9),
            new ChapterRoomSpec(27, "combat_wide_final", RoomType.Combat, RoomRewardType.None, "PF_Room_Wide_Combat", true, false, 10),
            new ChapterRoomSpec(28, "combat_pressure_final", RoomType.Combat, RoomRewardType.None, "PF_Room_Medium_Combat", true, false, 10),
            new ChapterRoomSpec(29, "preboss_elite", RoomType.Elite, RoomRewardType.None, "PF_Room_Elite", true, false, 2),
            new ChapterRoomSpec(30, "boss_clear", RoomType.Boss, RoomRewardType.BossClear, "PF_Room_Boss", true, true, 1)
        };

        [MenuItem(MenuPath)]
        public static void CreateChapterOneProductionData()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Chapter 1 Production Data", "Exit Play Mode before creating Chapter 1 data.", "OK");
                return;
            }

            EnsureFolder(RoomFolder);
            EnsureFolder(WaveFolder);
            EnsureFolder(EnemyFolder);
            EnsureFolder(Path.GetDirectoryName(ChapterPath)?.Replace('\\', '/') ?? "Assets/_Project/ScriptableObjects/Chapters");

            var warnings = new List<string>();
            var enemyConfig = ResolveOrCreateEnemyConfig();
            var enemyPrefab = ResolveEnemyPrefab(warnings);
            var roomConfigs = new List<RoomTemplateConfig>(Rooms.Length);

            for (var i = 0; i < Rooms.Length; i++)
            {
                var spec = Rooms[i];
                var wave = spec.HasWave
                    ? CreateOrUpdateWave($"{WaveFolder}/Wave_C01_{spec.Index:00}_{spec.Slug}.asset", spec, enemyConfig, enemyPrefab)
                    : null;
                var room = CreateOrUpdateRoom($"{RoomFolder}/RoomTemplate_C01_{spec.Index:00}_{spec.Slug}.asset", spec, wave, warnings);
                roomConfigs.Add(room);
            }

            var chapter = CreateOrUpdateChapter(roomConfigs);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = chapter;
            EditorGUIUtility.PingObject(chapter);

            for (var i = 0; i < warnings.Count; i++)
            {
                Debug.LogWarning(warnings[i]);
            }

            Debug.Log($"{nameof(ChapterOneContentBuilder)} created/updated {ChapterPath} with {roomConfigs.Count} rooms.", chapter);
            EditorUtility.DisplayDialog(
                "Chapter 1 Production Data",
                warnings.Count == 0
                    ? "Chapter_01 production data was created or updated."
                    : $"Chapter_01 production data was created or updated with {warnings.Count} warning(s). Check Window > General > Console.",
                "OK");
        }

        private static WaveConfig CreateOrUpdateWave(string path, ChapterRoomSpec spec, EnemyConfig enemyConfig, GameObject enemyPrefab)
        {
            var wave = LoadOrCreateAsset<WaveConfig>(path, "Update Chapter 1 Wave");
            var serializedObject = new SerializedObject(wave);
            SetString(serializedObject, "waveId", $"wave_c01_{spec.Index:00}_{spec.Slug}");
            SetFloat(serializedObject, "startDelay", spec.IsBoss ? 0.8f : 0.35f);
            SetBool(serializedObject, "completeWhenAllSpawnedEnemiesDead", true);

            var enemies = serializedObject.FindProperty("enemies");
            if (enemies != null)
            {
                enemies.arraySize = 1;
                var entry = enemies.GetArrayElementAtIndex(0);
                SetRelativeObject(entry, "enemyConfig", enemyConfig);
                SetRelativeObject(entry, "enemyPrefab", enemyPrefab);
                SetRelativeInt(entry, "count", Mathf.Max(1, spec.EnemyCount));
                SetRelativeFloat(entry, "spawnDelay", spec.IsBoss ? 0f : 0.22f);
                SetRelativeInt(entry, "spawnPointIndex", spec.IsBoss ? 0 : -1);
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(wave);
            return wave;
        }

        private static RoomTemplateConfig CreateOrUpdateRoom(string path, ChapterRoomSpec spec, WaveConfig wave, List<string> warnings)
        {
            var room = LoadOrCreateAsset<RoomTemplateConfig>(path, "Update Chapter 1 Room");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{RoomPrefabFolder}/{spec.PrefabName}.prefab");
            if (prefab == null)
            {
                warnings.Add($"Room {spec.Index:00} references missing prefab {spec.PrefabName}. Run Tools > Tap Knockout > Rooms > Create Placeholder Room Prefabs first.");
            }

            var serializedObject = new SerializedObject(room);
            SetString(serializedObject, "roomId", $"room_c01_{spec.Index:00}_{spec.Slug}");
            SetEnum(serializedObject, "roomType", (int)spec.RoomType);
            SetFloat(serializedObject, "startDelay", spec.IsBoss ? 0.35f : 0.25f);
            SetBool(serializedObject, "lockExitsUntilCleared", true);
            SetObject(serializedObject, "roomPrefab", prefab);
            SetEnum(serializedObject, "rewardType", (int)spec.RewardType);
            SetBool(serializedObject, "autoAdvanceAfterClear", spec.RewardType == RoomRewardType.None && spec.RoomType != RoomType.Shop);
            SetBool(serializedObject, "grantsAbilityReward", spec.RewardType == RoomRewardType.Ability);
            SetBool(serializedObject, "grantsHealReward", spec.RewardType == RoomRewardType.Heal);
            SetBool(serializedObject, "isBossRoom", spec.IsBoss);
            SetString(serializedObject, "environmentThemeId", spec.PrefabName.ToLowerInvariant());

            var waves = serializedObject.FindProperty("waves");
            if (waves != null)
            {
                waves.arraySize = wave != null ? 1 : 0;
                if (wave != null)
                {
                    waves.GetArrayElementAtIndex(0).objectReferenceValue = wave;
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(room);
            return room;
        }

        private static ChapterConfig CreateOrUpdateChapter(IReadOnlyList<RoomTemplateConfig> rooms)
        {
            var chapter = LoadOrCreateAsset<ChapterConfig>(ChapterPath, "Update Chapter 1 Config");
            var serializedObject = new SerializedObject(chapter);
            SetString(serializedObject, "chapterId", "chapter_001");
            SetString(serializedObject, "displayName", "Chapter 1");
            SetInt(serializedObject, "chapterIndex", 1);
            SetInt(serializedObject, "recommendedPower", 3);

            var roomSequence = serializedObject.FindProperty("rooms");
            if (roomSequence != null)
            {
                roomSequence.arraySize = rooms.Count;
                for (var i = 0; i < rooms.Count; i++)
                {
                    roomSequence.GetArrayElementAtIndex(i).objectReferenceValue = rooms[i];
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(chapter);
            return chapter;
        }

        private static EnemyConfig ResolveOrCreateEnemyConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<EnemyConfig>("Assets/_Project/ScriptableObjects/Enemies/EnemyConfig_MeleeChaser.asset");
            if (existing != null)
            {
                return existing;
            }

            var config = LoadOrCreateAsset<EnemyConfig>($"{EnemyFolder}/EnemyConfig_C01_Placeholder.asset", "Create Chapter 1 Placeholder Enemy");
            var serializedObject = new SerializedObject(config);
            SetString(serializedObject, "enemyId", "enemy_c01_placeholder");
            SetFloat(serializedObject, "maxHealth", 42f);
            SetFloat(serializedObject, "moveSpeed", 2.2f);
            SetFloat(serializedObject, "contactDamage", 4f);
            SetFloat(serializedObject, "attackRange", 1.2f);
            SetFloat(serializedObject, "attackCooldown", 1f);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            return config;
        }

        private static GameObject ResolveEnemyPrefab(List<string> warnings)
        {
            var prefab = FindPrefab("PF_Enemy_BasicMelee_GreenDemon_Generated")
                ?? FindPrefab("PF_Enemy_MeleeChaser_Test")
                ?? FindPrefab("PF_Enemy_FastMelee_Alien_Generated");
            if (prefab == null)
            {
                warnings.Add("No enemy prefab found for Chapter 1 generated waves. Run the character/enemy prefab builder before Play Mode.");
            }

            return prefab;
        }

        private static GameObject FindPrefab(string prefabName)
        {
            var folders = new[]
            {
                "Assets/_Project/Prefabs/Enemies/Generated",
                "Assets/_Project/Prefabs/Enemies",
                "Assets/_Project/Prefabs"
            };

            for (var i = 0; i < folders.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(folders[i]))
                {
                    continue;
                }

                var direct = AssetDatabase.LoadAssetAtPath<GameObject>($"{folders[i]}/{prefabName}.prefab");
                if (direct != null)
                {
                    return direct;
                }
            }

            return null;
        }

        private static T LoadOrCreateAsset<T>(string path, string undoName) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                Undo.RecordObject(asset, undoName);
                return asset;
            }

            EnsureFolder(Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "Assets/_Project/ScriptableObjects");
            asset = ScriptableObject.CreateInstance<T>();
            Undo.RegisterCreatedObjectUndo(asset, undoName);
            AssetDatabase.CreateAsset(asset, path);
            Undo.RecordObject(asset, undoName);
            return asset;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parts = folderPath.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.stringValue = value;
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

        private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
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

        private static void SetEnum(SerializedObject serializedObject, string propertyName, int enumValueIndex)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.enumValueIndex = enumValueIndex;
            }
        }

        private static void SetObject(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static void SetRelativeObject(SerializedProperty parent, string propertyName, UnityEngine.Object value)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static void SetRelativeInt(SerializedProperty parent, string propertyName, int value)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetRelativeFloat(SerializedProperty parent, string propertyName, float value)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private readonly struct ChapterRoomSpec
        {
            public ChapterRoomSpec(
                int index,
                string slug,
                RoomType roomType,
                RoomRewardType rewardType,
                string prefabName,
                bool hasWave,
                bool isBoss,
                int enemyCount)
            {
                Index = index;
                Slug = slug;
                RoomType = roomType;
                RewardType = rewardType;
                PrefabName = prefabName;
                HasWave = hasWave;
                IsBoss = isBoss;
                EnemyCount = enemyCount;
            }

            public int Index { get; }
            public string Slug { get; }
            public RoomType RoomType { get; }
            public RoomRewardType RewardType { get; }
            public string PrefabName { get; }
            public bool HasWave { get; }
            public bool IsBoss { get; }
            public int EnemyCount { get; }
        }
    }
}
