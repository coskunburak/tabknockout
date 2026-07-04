using System.Collections.Generic;
using TapKnockout.Boss;
using TapKnockout.Enemy;
using TapKnockout.Level;
using TapKnockout.Room;
using TapKnockout.Wave;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class Phase4WiringValidator
    {
        private const string MenuPath = "Tools/Tap Knockout/Validation/Validate Phase 4 Wiring";
        private const string EnemyConfigFolder = "Assets/_Project/ScriptableObjects/Enemies";
        private const string BossFolder = "Assets/_Project/ScriptableObjects/Bosses";
        private const string WaveFolder = "Assets/_Project/ScriptableObjects/Waves";
        private const string RoomPrefabFolder = "Assets/_Project/Prefabs/Rooms";
        private const string EnemyPrefabFolder = "Assets/_Project/Prefabs/Enemies";
        private const string BossPrefabPath = "Assets/_Project/Prefabs/Bosses/PF_Boss_DashCounterBrute.prefab";
        private const string PlaytestChapterPath = "Assets/_Project/ScriptableObjects/Chapters/Chapter_Playtest_EnemiesBosses.asset";

        private static readonly string[] EnemyConfigNames =
        {
            "EnemyConfig_MeleeChaser",
            "EnemyConfig_FastCharger",
            "EnemyConfig_RangedShooter",
            "EnemyConfig_AreaBomber",
            "EnemyConfig_ShieldEnemy",
            "EnemyConfig_SplitterEnemy",
            "EnemyConfig_EliteChaser",
            "EnemyConfig_EliteRanged"
        };

        private static readonly string[] EnemyPrefabNames =
        {
            "PF_Enemy_MeleeChaser",
            "PF_Enemy_FastCharger",
            "PF_Enemy_RangedShooter",
            "PF_Enemy_AreaBomber",
            "PF_Enemy_ShieldEnemy",
            "PF_Enemy_SplitterEnemy",
            "PF_Enemy_EliteChaser",
            "PF_Enemy_EliteRanged"
        };

        private static readonly string[] TestWaveNames =
        {
            "Wave_Test_MeleeChaser",
            "Wave_Test_FastCharger",
            "Wave_Test_RangedShooter",
            "Wave_Test_AreaBomber",
            "Wave_Test_ShieldEnemy",
            "Wave_Test_SplitterEnemy",
            "Wave_Test_EliteChaser",
            "Wave_Test_EliteRanged",
            "Wave_Test_Boss1_DashCounterBrute"
        };

        [MenuItem(MenuPath)]
        public static void ValidatePhase4Wiring()
        {
            var report = BuildReport();
            report.LogToConsole();
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Phase 4 Wiring Validator",
                    $"Done: {report.DoneCount}\nWarnings: {report.WarningCount}\nErrors: {report.ErrorCount}\nBlocked: {report.BlockedCount}\n\nCheck Window > General > Console for details.",
                    "OK");
            }
        }

        public static ValidationReport BuildReport()
        {
            var report = new ValidationReport();
            ValidateRoomPrefabs(report);
            ValidateEnemyConfigsAndPrefabs(report);
            ValidateBossConfig(report);
            ValidateTestWaves(report);
            ValidateTelegraphPrefabs(report);
            ValidatePlaytestChapter(report);
            ValidateBossHealthBarSetup(report);
            return report;
        }

        private static void ValidateRoomPrefabs(ValidationReport report)
        {
            if (!AssetDatabase.IsValidFolder(RoomPrefabFolder))
            {
                report.Blocked(RoomPrefabFolder, "Room prefab folder is missing.", "Create/restore Assets/_Project/Prefabs/Rooms.");
                return;
            }

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { RoomPrefabFolder });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var contract = prefab != null ? prefab.GetComponentInChildren<RoomPrefabContract>(true) : null;
                if (contract == null)
                {
                    report.Error(path, "Missing RoomPrefabContract.", "Run Tools > Tap Knockout > Rooms > Repair Room Prefab Contracts.");
                    continue;
                }

                if (!contract.HasRequiredReferences)
                {
                    report.Error(path, "RoomPrefabContract required references are incomplete.", "Run room contract repair and validator.");
                    continue;
                }

                if (contract.VisualRoot == null || contract.GameplayRoot == null || contract.Bounds == null)
                {
                    report.Warning(path, "Optional production wiring fields are incomplete.", "Run room contract repair.");
                    continue;
                }

                report.Done(path, "Room prefab contract is wired.");
            }
        }

        private static void ValidateEnemyConfigsAndPrefabs(ValidationReport report)
        {
            for (var i = 0; i < EnemyConfigNames.Length; i++)
            {
                var configPath = $"{EnemyConfigFolder}/{EnemyConfigNames[i]}.asset";
                var config = AssetDatabase.LoadAssetAtPath<EnemyConfig>(configPath);
                if (config == null)
                {
                    report.Error(configPath, "Missing enemy config.", "Run Tools > Tap Knockout > Enemies > Create Enemy Archetype Configs.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(config.EnemyId))
                {
                    report.Error(configPath, "Enemy config id is empty.", "Repair config id.");
                }
                else
                {
                    report.Done(configPath, $"Enemy config {config.EnemyId} exists.");
                }

                var prefabPath = $"{EnemyPrefabFolder}/{EnemyPrefabNames[i]}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    report.Error(prefabPath, "Missing generated enemy playtest prefab.", "Run Tools > Tap Knockout > Enemies > Repair Enemy/Boss Prefab References.");
                    continue;
                }

                if (prefab.GetComponentInChildren<EnemyController>(true) == null || prefab.GetComponentInChildren<EnemyHealth>(true) == null)
                {
                    report.Error(prefabPath, "Enemy prefab is missing runtime controller/health.", "Run enemy/boss prefab repair.");
                    continue;
                }

                report.Done(prefabPath, "Enemy prefab has runtime components.");
            }
        }

        private static void ValidateBossConfig(ValidationReport report)
        {
            var bossConfigPath = $"{BossFolder}/BossConfig_DashCounterBrute.asset";
            var bossConfig = AssetDatabase.LoadAssetAtPath<BossConfig>(bossConfigPath);
            if (bossConfig == null)
            {
                report.Error(bossConfigPath, "Missing BossConfig_DashCounterBrute.", "Run Tools > Tap Knockout > Bosses > Create Boss 1 Dash-Counter Brute.");
                return;
            }

            if (bossConfig.EnemyConfig == null)
            {
                report.Error(bossConfigPath, "BossConfig missing EnemyConfig.", "Run Boss 1 content builder.");
            }

            if (bossConfig.AddEnemyConfig == null || bossConfig.AddEnemyPrefab == null)
            {
                report.Error(bossConfigPath, "BossConfig add config/prefab references are missing.", "Run enemy/boss prefab reference repair.");
            }

            if (bossConfig.Phases == null || bossConfig.Phases.Count < 3)
            {
                report.Error(bossConfigPath, "BossConfig must define 3 phases.", "Run Boss 1 content builder.");
            }
            else
            {
                for (var i = 0; i < bossConfig.Phases.Count; i++)
                {
                    if (bossConfig.Phases[i] == null || bossConfig.Phases[i].Pattern == null)
                    {
                        report.Error(bossConfigPath, $"Boss phase {i + 1} is missing pattern.", "Run Boss 1 content builder.");
                    }
                }
            }

            var bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath);
            if (bossPrefab == null)
            {
                report.Error(BossPrefabPath, "Boss prefab is missing.", "Run enemy/boss prefab reference repair.");
            }
            else if (bossPrefab.GetComponentInChildren<BossRuntimeBindingBridge>(true) == null)
            {
                report.Error(BossPrefabPath, "Boss prefab missing BossRuntimeBindingBridge.", "Run enemy/boss prefab reference repair.");
            }
            else
            {
                report.Done(BossPrefabPath, "Boss prefab has runtime binding bridge.");
            }
        }

        private static void ValidateTestWaves(ValidationReport report)
        {
            for (var i = 0; i < TestWaveNames.Length; i++)
            {
                var path = $"{WaveFolder}/{TestWaveNames[i]}.asset";
                var wave = AssetDatabase.LoadAssetAtPath<WaveConfig>(path);
                if (wave == null)
                {
                    report.Error(path, "Missing test wave.", "Run enemy or boss content builder.");
                    continue;
                }

                if (wave.Enemies == null || wave.Enemies.Count == 0)
                {
                    report.Error(path, "Wave has no enemy entries.", "Repair wave data.");
                    continue;
                }

                var hasMissingReference = false;
                for (var entryIndex = 0; entryIndex < wave.Enemies.Count; entryIndex++)
                {
                    var entry = wave.Enemies[entryIndex];
                    if (entry == null || entry.EnemyConfig == null || entry.EnemyPrefab == null || entry.Count <= 0)
                    {
                        hasMissingReference = true;
                    }
                }

                if (hasMissingReference)
                {
                    report.Error(path, "Wave has missing config/prefab/count references.", "Run enemy/boss prefab reference repair.");
                    continue;
                }

                report.Done(path, "Wave entries are wired.");
            }
        }

        private static void ValidateTelegraphPrefabs(ValidationReport report)
        {
            var paths = new[]
            {
                "Assets/_Project/Prefabs/Telegraphs/PF_Telegraph_Circle.prefab",
                "Assets/_Project/Prefabs/Telegraphs/PF_Telegraph_Line.prefab",
                "Assets/_Project/Prefabs/Telegraphs/PF_Telegraph_ChargePath.prefab"
            };

            for (var i = 0; i < paths.Length; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                if (prefab == null)
                {
                    report.Warning(paths[i], "Telegraph placeholder prefab is missing.", "Run Tools > Tap Knockout > Enemies > Create Telegraph Placeholder Prefabs.");
                    continue;
                }

                report.Done(paths[i], "Telegraph placeholder exists.");
            }
        }

        private static void ValidatePlaytestChapter(ValidationReport report)
        {
            var chapter = AssetDatabase.LoadAssetAtPath<ChapterConfig>(PlaytestChapterPath);
            if (chapter == null)
            {
                report.Error(PlaytestChapterPath, "Missing playtest chapter.", "Run Tools > Tap Knockout > Content > Create Enemy/Boss Playtest Chapter.");
                return;
            }

            if (chapter.Rooms == null || chapter.Rooms.Count != 10)
            {
                report.Error(PlaytestChapterPath, "Playtest chapter should contain exactly 10 rooms.", "Run playtest chapter builder.");
                return;
            }

            for (var i = 0; i < chapter.Rooms.Count; i++)
            {
                var room = chapter.Rooms[i];
                if (room == null || room.RoomPrefab == null || !room.HasWaves)
                {
                    report.Error(PlaytestChapterPath, $"Room {i + 1:00} is missing room prefab or wave.", "Run playtest chapter builder.");
                }
            }

            var finalRoom = chapter.Rooms[chapter.Rooms.Count - 1];
            if (finalRoom == null || finalRoom.RoomType != RoomType.Boss || finalRoom.RewardType != RoomRewardType.BossClear)
            {
                report.Error(PlaytestChapterPath, "Final playtest room must be Boss/BossClear.", "Run playtest chapter builder.");
                return;
            }

            report.Done(PlaytestChapterPath, "Playtest chapter is wired with 10 rooms and boss final room.");
        }

        private static void ValidateBossHealthBarSetup(ValidationReport report)
        {
            const string path = "Assets/_Project/Prefabs/UI/PF_BossHealthBar_Playtest.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                report.Warning(path, "Boss health bar placeholder prefab is missing.", "Run Tools > Tap Knockout > UI > Create Boss Health Bar Placeholder or wire an existing BossHealthBarController in scene.");
                return;
            }

            if (prefab.GetComponentInChildren<TapKnockout.UI.BossHealthBarController>(true) == null)
            {
                report.Error(path, "Boss health bar prefab has no BossHealthBarController.", "Run boss health bar setup builder.");
                return;
            }

            report.Done(path, "Boss health bar prefab exists.");
        }

        public sealed class ValidationReport
        {
            private readonly List<string> lines = new List<string>();

            public int DoneCount { get; private set; }
            public int WarningCount { get; private set; }
            public int ErrorCount { get; private set; }
            public int BlockedCount { get; private set; }

            public void Done(string path, string message)
            {
                DoneCount++;
                lines.Add($"Done | {path} | {message}");
            }

            public void Warning(string path, string message, string action)
            {
                WarningCount++;
                lines.Add($"Warning | {path} | {message} Suggested action: {action}");
            }

            public void Error(string path, string message, string action)
            {
                ErrorCount++;
                lines.Add($"Error | {path} | {message} Suggested action: {action}");
            }

            public void Blocked(string path, string message, string action)
            {
                BlockedCount++;
                lines.Add($"Blocked | {path} | {message} Suggested action: {action}");
            }

            public void LogToConsole()
            {
                for (var i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    if (line.StartsWith("Error", System.StringComparison.Ordinal) || line.StartsWith("Blocked", System.StringComparison.Ordinal))
                    {
                        Debug.LogError(line);
                    }
                    else if (line.StartsWith("Warning", System.StringComparison.Ordinal))
                    {
                        Debug.LogWarning(line);
                    }
                    else
                    {
                        Debug.Log(line);
                    }
                }

                Debug.Log($"{nameof(Phase4WiringValidator)} summary: Done {DoneCount}, Warning {WarningCount}, Error {ErrorCount}, Blocked {BlockedCount}.");
            }
        }
    }
}
