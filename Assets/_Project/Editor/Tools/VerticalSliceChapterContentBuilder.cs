using System;
using System.Collections.Generic;
using System.IO;
using TapKnockout.Ability;
using TapKnockout.Enemy;
using TapKnockout.Level;
using TapKnockout.Room;
using TapKnockout.Wave;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.EditorTools
{
    public static class VerticalSliceChapterContentBuilder
    {
        public const string ChapterPath = "Assets/_Project/ScriptableObjects/Chapters/Chapter_VerticalSlice_01.asset";

        private const string MenuPath = "Tools/Tap Knockout/Content/Create Vertical Slice Chapter Content";
        private const string ScriptableObjectsFolder = "Assets/_Project/ScriptableObjects";
        private const string ChapterFolder = ScriptableObjectsFolder + "/Chapters";
        private const string RoomFolder = ScriptableObjectsFolder + "/Rooms";
        private const string WaveFolder = ScriptableObjectsFolder + "/Waves";
        private const string EnemyFolder = ScriptableObjectsFolder + "/Enemies";
        private const string AbilityFolder = ScriptableObjectsFolder + "/Abilities";
        private static readonly string[] EnemyPrefabFolders =
        {
            "Assets/_Project/Prefabs/Enemies",
            "Assets/_Project/Prefabs"
        };

        [MenuItem(MenuPath)]
        public static void CreateVerticalSliceChapterContent()
        {
            if (Application.isPlaying)
            {
                ShowDialog(
                    "Vertical Slice Chapter Content",
                    "Stop Play Mode before creating vertical slice content.",
                    "OK");
                return;
            }

            EnsureFolder(WaveFolder);
            EnsureFolder(RoomFolder);
            EnsureFolder(ChapterFolder);
            EnsureFolder(EnemyFolder);
            EnsureFolder(AbilityFolder);

            var warnings = new List<string>();
            EnsureVerticalSliceAbilityDefinitions(warnings);
            var meleePrefab = FindPrefab("PF_Enemy_BasicMelee_GreenDemon_Generated");
            var rangedPrefab = meleePrefab;
            var elitePrefab = meleePrefab;
            var bossPrefab = meleePrefab;

            if (meleePrefab == null)
            {
                warnings.Add("Generated BasicMelee enemy prefab missing: run Tools > Tap Knockout > Characters > Apply Generated Characters To Gameplay before Play.");
            }

            var meleeConfig = CreateOrUpdateEnemyConfig(
                EnemyFolder + "/EnemyConfig_MeleeChaser.asset",
                "melee_chaser_basic",
                42f,
                2.25f,
                18f,
                720f,
                1.08f,
                4f,
                1.25f,
                0.95f,
                0.2f,
                true,
                true,
                1,
                1);
            var wave1 = CreateOrUpdateWave(
                WaveFolder + "/Wave_VS_01_SmallMelee.asset",
                "wave_vs_01_small_melee",
                new[]
                {
                    new WaveEntrySpec(meleeConfig, meleePrefab, 3, 0.35f, -1)
                });
            var wave2 = CreateOrUpdateWave(
                WaveFolder + "/Wave_VS_02_MeleeGroup.asset",
                "wave_vs_02_melee_group",
                new[]
                {
                    new WaveEntrySpec(meleeConfig, meleePrefab, 5, 0.32f, -1)
                });
            var wave3 = CreateOrUpdateWave(
                WaveFolder + "/Wave_VS_03_MixedPressure.asset",
                "wave_vs_03_mixed_pressure",
                new[]
                {
                    new WaveEntrySpec(meleeConfig, meleePrefab, 3, 0.28f, -1),
                    new WaveEntrySpec(meleeConfig, rangedPrefab, 2, 0.55f, -1)
                });
            var wave4 = CreateOrUpdateWave(
                WaveFolder + "/Wave_VS_04_ElitePlaceholder.asset",
                "wave_vs_04_elite_placeholder",
                new[]
                {
                    new WaveEntrySpec(meleeConfig, elitePrefab, 2, 0.42f, -1)
                });
            var wave5 = CreateOrUpdateWave(
                WaveFolder + "/Wave_VS_05_LightRecoveryCombat.asset",
                "wave_vs_05_light_recovery_combat",
                new[]
                {
                    new WaveEntrySpec(meleeConfig, meleePrefab, 3, 0.4f, -1)
                });
            var wave6 = CreateOrUpdateWave(
                WaveFolder + "/Wave_VS_06_CombatPressure.asset",
                "wave_vs_06_combat_pressure",
                new[]
                {
                    new WaveEntrySpec(meleeConfig, meleePrefab, 4, 0.24f, -1),
                    new WaveEntrySpec(meleeConfig, meleePrefab, 2, 0.5f, -1)
                });
            var wave7 = CreateOrUpdateWave(
                WaveFolder + "/Wave_VS_07_RangedPressure.asset",
                "wave_vs_07_ranged_pressure",
                new[]
                {
                    new WaveEntrySpec(meleeConfig, meleePrefab, 3, 0.32f, -1),
                    new WaveEntrySpec(meleeConfig, rangedPrefab, 3, 0.6f, -1)
                });
            var wave8 = CreateOrUpdateWave(
                WaveFolder + "/Wave_VS_08_EliteAbility.asset",
                "wave_vs_08_elite_ability",
                new[]
                {
                    new WaveEntrySpec(meleeConfig, meleePrefab, 2, 0.3f, -1),
                    new WaveEntrySpec(meleeConfig, elitePrefab, 2, 0.65f, -1)
                });
            var wave9 = CreateOrUpdateWave(
                WaveFolder + "/Wave_VS_09_PreBossPressure.asset",
                "wave_vs_09_pre_boss_pressure",
                new[]
                {
                    new WaveEntrySpec(meleeConfig, meleePrefab, 5, 0.22f, -1),
                    new WaveEntrySpec(meleeConfig, rangedPrefab, 3, 0.52f, -1)
                });
            var wave10 = CreateOrUpdateWave(
                WaveFolder + "/Wave_VS_10_BossPlaceholder.asset",
                "wave_vs_10_boss_placeholder",
                new[]
                {
                    new WaveEntrySpec(meleeConfig, bossPrefab, 1, 0.18f, 0)
                });

            var rooms = new[]
            {
                CreateOrUpdateRoom(RoomFolder + "/RoomTemplate_VS_01_AbilityIntro.asset", "room_vs_01_ability_intro", RoomType.Combat, RoomRewardType.Ability, false, false, wave1),
                CreateOrUpdateRoom(RoomFolder + "/RoomTemplate_VS_02_Combat.asset", "room_vs_02_combat", RoomType.Combat, RoomRewardType.None, false, false, wave2),
                CreateOrUpdateRoom(RoomFolder + "/RoomTemplate_VS_03_AbilityReward.asset", "room_vs_03_ability_reward", RoomType.AbilityReward, RoomRewardType.Ability, false, false, wave3),
                CreateOrUpdateRoom(RoomFolder + "/RoomTemplate_VS_04_Elite.asset", "room_vs_04_elite", RoomType.Elite, RoomRewardType.None, false, false, wave4),
                CreateOrUpdateRoom(RoomFolder + "/RoomTemplate_VS_05_RecoveryPlaceholder.asset", "room_vs_05_recovery_placeholder", RoomType.Heal, RoomRewardType.Heal, false, false, wave5),
                CreateOrUpdateRoom(RoomFolder + "/RoomTemplate_VS_06_CombatPressureAbility.asset", "room_vs_06_combat_pressure_ability", RoomType.Combat, RoomRewardType.Ability, false, false, wave6),
                CreateOrUpdateRoom(RoomFolder + "/RoomTemplate_VS_07_RangedPressure.asset", "room_vs_07_ranged_pressure", RoomType.Combat, RoomRewardType.None, false, false, wave7),
                CreateOrUpdateRoom(RoomFolder + "/RoomTemplate_VS_08_EliteAbility.asset", "room_vs_08_elite_ability", RoomType.Elite, RoomRewardType.Ability, false, false, wave8),
                CreateOrUpdateRoom(RoomFolder + "/RoomTemplate_VS_09_CombatPressure.asset", "room_vs_09_combat_pressure", RoomType.Combat, RoomRewardType.None, false, false, wave9),
                CreateOrUpdateRoom(RoomFolder + "/RoomTemplate_VS_10_BossPlaceholder.asset", "room_vs_10_boss_placeholder", RoomType.Boss, RoomRewardType.BossClear, false, true, wave10)
            };

            var chapter = CreateOrUpdateChapter(ChapterPath, rooms);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"{nameof(VerticalSliceChapterContentBuilder)} created/updated vertical slice content: {ChapterPath}, {rooms.Length} rooms, 10 waves, placeholder enemy configs, and supported ability assets.",
                chapter);

            Selection.activeObject = chapter;
            EditorGUIUtility.PingObject(chapter);

            if (warnings.Count > 0)
            {
                for (var i = 0; i < warnings.Count; i++)
                {
                    Debug.LogWarning(warnings[i]);
                }

                ShowDialog(
                    "Vertical Slice Chapter Content",
                    "Vertical slice content was created/updated with warnings. Check Window > General > Console for fallback assignments.",
                    "OK");
                return;
            }

            ShowDialog(
                "Vertical Slice Chapter Content",
                "Vertical slice content was created/updated successfully.",
                "OK");
        }

        private static void ShowDialog(string title, string message, string ok)
        {
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(title, message, ok);
            }
        }

        private static void EnsureVerticalSliceAbilityDefinitions(List<string> warnings)
        {
            var createdCount = 0;
            createdCount += CreateAbilityIfMissing(
                AbilityFolder + "/Ability_DashKnockbackUp.asset",
                "dash_knockback_up",
                "Heavy Impact",
                "Dash impacts push enemies farther.",
                AbilityRarity.Uncommon,
                AbilityCategory.Dash,
                AbilityEffectType.DashKnockbackUp,
                0.25f,
                4,
                55f) ? 1 : 0;
            createdCount += CreateAbilityIfMissing(
                AbilityFolder + "/Ability_MoveSpeedUp.asset",
                "move_speed_up",
                "Footwork",
                "Increases movement speed.",
                AbilityRarity.Common,
                AbilityCategory.Utility,
                AbilityEffectType.MoveSpeedUp,
                0.1f,
                4,
                50f) ? 1 : 0;
            createdCount += CreateAbilityIfMissing(
                AbilityFolder + "/Ability_ProjectileSpeedUp.asset",
                "projectile_speed_up",
                "Sharper Shots",
                "Projectiles travel faster.",
                AbilityRarity.Common,
                AbilityCategory.Projectile,
                AbilityEffectType.ProjectileSpeedUp,
                0.2f,
                4,
                45f) ? 1 : 0;

            if (createdCount > 0)
            {
                warnings.Add("New vertical slice ability assets were created. Add them to AbilitySelectionController ability pool if the pool is assigned manually in the scene.");
            }
        }

        private static bool CreateAbilityIfMissing(
            string path,
            string abilityId,
            string displayName,
            string description,
            AbilityRarity rarity,
            AbilityCategory category,
            AbilityEffectType effectType,
            float value,
            int maxStacks,
            float weight)
        {
            if (AssetDatabase.LoadAssetAtPath<AbilityDefinition>(path) != null)
            {
                return false;
            }

            EnsureFolder(Path.GetDirectoryName(path)?.Replace('\\', '/') ?? AbilityFolder);
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            Undo.RegisterCreatedObjectUndo(ability, "Create Vertical Slice AbilityDefinition");
            AssetDatabase.CreateAsset(ability, path);

            var serializedObject = new SerializedObject(ability);
            SetString(serializedObject, "abilityId", abilityId);
            SetString(serializedObject, "displayName", displayName);
            SetString(serializedObject, "description", description);
            SetEnum(serializedObject, "rarity", (int)rarity);
            SetEnum(serializedObject, "category", (int)category);
            SetEnum(serializedObject, "effectType", (int)effectType);
            SetInt(serializedObject, "maxStacks", maxStacks);
            SetFloat(serializedObject, "weight", weight);
            SetBool(serializedObject, "allowDuplicateInOffer", false);
            SetBool(serializedObject, "isEnabled", true);
            SetFloat(serializedObject, "value", value);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(ability);
            return true;
        }

        private static EnemyConfig CreateOrUpdateEnemyConfig(
            string path,
            string enemyId,
            float maxHealth,
            float moveSpeed,
            float acceleration,
            float rotationSpeed,
            float stoppingDistance,
            float contactDamage,
            float attackRange,
            float attackCooldown,
            float knockbackResistance,
            bool canBeKnockedBack,
            bool canBeInterrupted,
            int coinReward,
            int xpReward)
        {
            var config = LoadOrCreateAsset<EnemyConfig>(path, "Update Vertical Slice EnemyConfig");
            var serializedObject = new SerializedObject(config);
            SetString(serializedObject, "enemyId", enemyId);
            SetFloat(serializedObject, "maxHealth", maxHealth);
            SetFloat(serializedObject, "deathDelay", 0.25f);
            SetFloat(serializedObject, "moveSpeed", moveSpeed);
            SetFloat(serializedObject, "acceleration", acceleration);
            SetFloat(serializedObject, "rotationSpeed", rotationSpeed);
            SetFloat(serializedObject, "stoppingDistance", stoppingDistance);
            SetFloat(serializedObject, "contactDamage", contactDamage);
            SetFloat(serializedObject, "attackRange", attackRange);
            SetFloat(serializedObject, "attackCooldown", attackCooldown);
            SetFloat(serializedObject, "knockbackResistance", knockbackResistance);
            SetBool(serializedObject, "canBeKnockedBack", canBeKnockedBack);
            SetBool(serializedObject, "canBeInterrupted", canBeInterrupted);
            SetInt(serializedObject, "coinReward", coinReward);
            SetInt(serializedObject, "xpReward", xpReward);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            return config;
        }

        private static WaveConfig CreateOrUpdateWave(string path, string waveId, IReadOnlyList<WaveEntrySpec> enemyEntries)
        {
            var config = LoadOrCreateAsset<WaveConfig>(path, "Update Vertical Slice WaveConfig");
            var serializedObject = new SerializedObject(config);
            SetString(serializedObject, "waveId", waveId);
            SetFloat(serializedObject, "startDelay", 0.45f);
            SetBool(serializedObject, "completeWhenAllSpawnedEnemiesDead", true);

            var entries = serializedObject.FindProperty("enemies");
            if (entries != null)
            {
                entries.arraySize = enemyEntries != null ? enemyEntries.Count : 0;
                for (var i = 0; i < entries.arraySize; i++)
                {
                    var spec = enemyEntries[i];
                    var entry = entries.GetArrayElementAtIndex(i);
                    SetRelativeObject(entry, "enemyConfig", spec.EnemyConfig);
                    SetRelativeObject(entry, "enemyPrefab", spec.EnemyPrefab);
                    SetRelativeInt(entry, "count", spec.Count);
                    SetRelativeFloat(entry, "spawnDelay", spec.SpawnDelay);
                    SetRelativeInt(entry, "spawnPointIndex", spec.SpawnPointIndex);
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            return config;
        }

        private static RoomTemplateConfig CreateOrUpdateRoom(
            string path,
            string roomId,
            RoomType roomType,
            RoomRewardType rewardType,
            bool autoAdvanceAfterClear,
            bool isBossRoom,
            WaveConfig wave)
        {
            var config = LoadOrCreateAsset<RoomTemplateConfig>(path, "Update Vertical Slice RoomTemplateConfig");
            var serializedObject = new SerializedObject(config);
            SetString(serializedObject, "roomId", roomId);
            SetEnum(serializedObject, "roomType", (int)roomType);
            SetFloat(serializedObject, "startDelay", 0.45f);
            SetBool(serializedObject, "lockExitsUntilCleared", true);
            SetEnum(serializedObject, "rewardType", (int)rewardType);
            SetBool(serializedObject, "autoAdvanceAfterClear", autoAdvanceAfterClear);
            SetBool(serializedObject, "grantsAbilityReward", rewardType == RoomRewardType.Ability);
            SetBool(serializedObject, "grantsHealReward", rewardType == RoomRewardType.Heal);
            SetBool(serializedObject, "isBossRoom", isBossRoom);
            SetString(serializedObject, "environmentThemeId", isBossRoom ? "theme_kaykit_boss_arena_01" : "theme_kaykit_dungeon_01");

            var waves = serializedObject.FindProperty("waves");
            if (waves != null)
            {
                waves.arraySize = 1;
                waves.GetArrayElementAtIndex(0).objectReferenceValue = wave;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            return config;
        }

        private static ChapterConfig CreateOrUpdateChapter(string path, IReadOnlyList<RoomTemplateConfig> rooms)
        {
            var config = LoadOrCreateAsset<ChapterConfig>(path, "Update Vertical Slice ChapterConfig");
            var serializedObject = new SerializedObject(config);
            SetString(serializedObject, "chapterId", "chapter_vertical_slice_01");
            SetString(serializedObject, "displayName", "Vertical Slice 01");
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
            EditorUtility.SetDirty(config);
            return config;
        }

        private static T LoadOrCreateAsset<T>(string path, string undoName) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                Undo.RecordObject(asset, undoName);
                return asset;
            }

            EnsureFolder(Path.GetDirectoryName(path)?.Replace('\\', '/') ?? ScriptableObjectsFolder);
            asset = ScriptableObject.CreateInstance<T>();
            Undo.RegisterCreatedObjectUndo(asset, undoName);
            AssetDatabase.CreateAsset(asset, path);
            Undo.RecordObject(asset, undoName);
            return asset;
        }

        private static T FindAssetByName<T>(string folder, string assetName) where T : UnityEngine.Object
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                return null;
            }

            var exactAsset = AssetDatabase.LoadAssetAtPath<T>(folder + "/" + assetName + ".asset");
            if (exactAsset != null)
            {
                return exactAsset;
            }

            var exactPrefab = AssetDatabase.LoadAssetAtPath<T>(folder + "/" + assetName + ".prefab");
            if (exactPrefab != null)
            {
                return exactPrefab;
            }

            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder });
            Array.Sort(guids, StringComparer.Ordinal);
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.Equals(Path.GetFileNameWithoutExtension(path), assetName, StringComparison.Ordinal))
                {
                    return AssetDatabase.LoadAssetAtPath<T>(path);
                }
            }

            return null;
        }

        private static GameObject FindPrefab(params string[] assetNames)
        {
            for (var folderIndex = 0; folderIndex < EnemyPrefabFolders.Length; folderIndex++)
            {
                var folder = EnemyPrefabFolders[folderIndex];
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    continue;
                }

                for (var i = 0; i < assetNames.Length; i++)
                {
                    var prefab = FindAssetByName<GameObject>(folder, assetNames[i]);
                    if (prefab != null)
                    {
                        return prefab;
                    }
                }
            }

            return null;
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

        private readonly struct WaveEntrySpec
        {
            public WaveEntrySpec(EnemyConfig enemyConfig, GameObject enemyPrefab, int count, float spawnDelay, int spawnPointIndex)
            {
                EnemyConfig = enemyConfig;
                EnemyPrefab = enemyPrefab;
                Count = Mathf.Max(0, count);
                SpawnDelay = Mathf.Max(0f, spawnDelay);
                SpawnPointIndex = Mathf.Max(-1, spawnPointIndex);
            }

            public EnemyConfig EnemyConfig { get; }
            public GameObject EnemyPrefab { get; }
            public int Count { get; }
            public float SpawnDelay { get; }
            public int SpawnPointIndex { get; }
        }
    }
}
