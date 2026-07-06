using System.Collections.Generic;
using TapKnockout.Room;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class RoomContractValidator
    {
        private const string MenuPath = "Tools/Tap Knockout/Rooms/Validate Room Prefab Contracts";
        private const string RoomPrefabFolder = "Assets/_Project/Prefabs/Rooms";

        [MenuItem(MenuPath)]
        public static void ValidateRoomPrefabContracts()
        {
            if (!AssetDatabase.IsValidFolder(RoomPrefabFolder))
            {
                EditorUtility.DisplayDialog("Room Contract Validator", $"Missing folder: {RoomPrefabFolder}", "OK");
                return;
            }

            var errors = new List<string>();
            var warnings = new List<string>();
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { RoomPrefabFolder });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                var contract = prefab.GetComponentInChildren<RoomPrefabContract>(true);
                if (contract == null)
                {
                    errors.Add($"{path}: Error missing {nameof(RoomPrefabContract)}. Suggested action: run Tools > Tap Knockout > Rooms > Repair Room Prefab Contracts.");
                    continue;
                }

                ValidateContract(path, prefab, contract, errors, warnings);
            }

            for (var i = 0; i < warnings.Count; i++)
            {
                Debug.LogWarning(warnings[i]);
            }

            for (var i = 0; i < errors.Count; i++)
            {
                Debug.LogError(errors[i]);
            }

            var passed = errors.Count == 0;
            Debug.Log($"{nameof(RoomContractValidator)} {(passed ? "Done" : "Error")}: scanned {guids.Length} prefab(s), warnings {warnings.Count}, errors {errors.Count}.");
            EditorUtility.DisplayDialog(
                "Room Contract Validator",
                passed
                    ? $"All room prefab contracts passed validation. Warnings: {warnings.Count}."
                    : $"Room prefab validation found {errors.Count} error(s) and {warnings.Count} warning(s). Check Window > General > Console.",
                "OK");
        }

        private static void ValidateContract(
            string path,
            GameObject prefab,
            RoomPrefabContract contract,
            List<string> errors,
            List<string> warnings)
        {
            if (string.IsNullOrWhiteSpace(contract.RoomId))
            {
                errors.Add($"{path}: Error missing field RoomId. Suggested action: run room prefab contract repair.");
            }

            AddMissingReferenceError(path, "ArenaRoot", contract.ArenaRoot, errors);
            AddMissingReferenceError(path, "PlayerSpawn", contract.PlayerSpawn, errors);
            AddMissingReferenceError(path, "EnemySpawnPointsRoot", contract.EnemySpawnPointsRoot, errors);
            AddMissingReferenceError(path, "CameraBounds", contract.CameraBounds, errors);
            AddMissingReferenceError(path, "RoomBounds", contract.Bounds, errors);
            AddMissingReferenceError(path, "VisualRoot", contract.VisualRoot, errors);
            AddMissingReferenceError(path, "GameplayRoot", contract.GameplayRoot, errors);

            if (RequiresEnemySpawns(contract.RoomType) && (contract.EnemySpawnPoints == null || contract.EnemySpawnPoints.Count == 0))
            {
                errors.Add($"{path}: Error missing field EnemySpawnPoints. Suggested action: create children under EnemySpawnPoints.");
            }

            if (RequiresRewardSpawn(contract.RoomType) && contract.GetRewardSpawnPoint() == null)
            {
                errors.Add($"{path}: Error missing field RewardSpawnPoint. Suggested action: run room prefab contract repair.");
            }

            if (contract.RoomType == RoomType.Boss && contract.GetBossSpawnPoint() == null)
            {
                errors.Add($"{path}: Error missing field BossSpawnPoint for boss room. Suggested action: run room prefab contract repair.");
            }

            if (RequiresExitGate(contract.RoomType) && (contract.GetExitGates() == null || contract.GetExitGates().Count == 0))
            {
                errors.Add($"{path}: Error missing field ExitGates. Suggested action: add RoomExitGate to gate marker or run room prefab contract repair.");
            }

            if (contract.HazardRoot == null)
            {
                warnings.Add($"{path}: Warning missing optional HazardRoot. Suggested action: run room prefab contract repair if hazards are needed.");
            }

            var contractWarnings = contract.ValidateContract();
            for (var i = 0; i < contractWarnings.Count; i++)
            {
                var message = contractWarnings[i];
                if (message.StartsWith("Optional", System.StringComparison.Ordinal)
                    || message.IndexOf("fallback", System.StringComparison.Ordinal) >= 0
                    || message.IndexOf("lock/unlock", System.StringComparison.Ordinal) >= 0)
                {
                    warnings.Add($"{path}: Warning {message} Suggested action: repair if this room type needs the optional hook.");
                    continue;
                }

                errors.Add($"{path}: Error {message} Suggested action: run Tools > Tap Knockout > Rooms > Repair Room Prefab Contracts.");
            }

            if (errors.Count == 0)
            {
                Debug.Log($"{nameof(RoomContractValidator)} Done: {path}", prefab);
            }
        }

        private static void AddMissingReferenceError(string path, string fieldName, Object value, List<string> errors)
        {
            if (value == null)
            {
                errors.Add($"{path}: Error missing field {fieldName}. Suggested action: run Tools > Tap Knockout > Rooms > Repair Room Prefab Contracts.");
            }
        }

        private static bool RequiresEnemySpawns(RoomType roomType)
        {
            return roomType == RoomType.Combat || roomType == RoomType.Elite || roomType == RoomType.Boss;
        }

        private static bool RequiresRewardSpawn(RoomType roomType)
        {
            return roomType == RoomType.Reward || roomType == RoomType.AbilityReward || roomType == RoomType.Heal || roomType == RoomType.Shop;
        }

        private static bool RequiresExitGate(RoomType roomType)
        {
            return roomType == RoomType.Combat || roomType == RoomType.Elite || roomType == RoomType.Boss;
        }
    }
}
