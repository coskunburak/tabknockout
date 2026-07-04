using System.Collections.Generic;
using TapKnockout.Enemy;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class EnemyConfigValidator
    {
        private const string MenuPath = "Tools/Tap Knockout/Enemies/Validate Enemy Configs";
        private const string EnemyFolder = "Assets/_Project/ScriptableObjects/Enemies";

        [MenuItem(MenuPath)]
        public static void ValidateEnemyConfigs()
        {
            var guids = AssetDatabase.FindAssets("t:EnemyConfig", new[] { EnemyFolder });
            var ids = new HashSet<string>();
            var issueCount = 0;

            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var config = AssetDatabase.LoadAssetAtPath<EnemyConfig>(path);
                if (config == null)
                {
                    continue;
                }

                issueCount += ValidateConfig(config, path, ids);
            }

            var message = issueCount == 0
                ? $"Enemy config validation passed for {guids.Length} asset(s)."
                : $"Enemy config validation found {issueCount} issue(s). Check Window > General > Console.";
            Debug.Log(message);
            EditorUtility.DisplayDialog("Validate Enemy Configs", message, "OK");
        }

        private static int ValidateConfig(EnemyConfig config, string path, HashSet<string> ids)
        {
            var issues = 0;
            if (string.IsNullOrWhiteSpace(config.EnemyId))
            {
                Debug.LogError($"EnemyConfig missing id: {path}", config);
                issues++;
            }
            else if (!ids.Add(config.EnemyId))
            {
                Debug.LogError($"Duplicate enemy id '{config.EnemyId}' in {path}", config);
                issues++;
            }

            if (config.MaxHealth <= 0f)
            {
                Debug.LogError($"EnemyConfig has invalid max health: {path}", config);
                issues++;
            }

            if (config.AttackCooldown <= 0f)
            {
                Debug.LogError($"EnemyConfig has invalid attack cooldown: {path}", config);
                issues++;
            }

            if ((config.Archetype == EnemyArchetype.RangedShooter || config.Archetype == EnemyArchetype.EliteRanged) && config.ProjectileSpeed <= 0f)
            {
                Debug.LogWarning($"Ranged enemy config has non-positive projectile speed: {path}", config);
                issues++;
            }

            if ((config.Archetype == EnemyArchetype.RangedShooter || config.Archetype == EnemyArchetype.EliteRanged) && config.ProjectilePrefab == null)
            {
                Debug.LogWarning($"Ranged enemy config has no projectile prefab assigned: {path}. Run Tools > Tap Knockout > Enemies > Repair Enemy/Boss Prefab References.", config);
                issues++;
            }

            if (config.Archetype == EnemyArchetype.SplitterEnemy && config.SplitSpawnCount > 0 && config.SplitSpawnPrefab == null)
            {
                Debug.LogWarning($"Splitter enemy has split count but no split prefab assigned yet: {path}. Run Tools > Tap Knockout > Enemies > Repair Enemy/Boss Prefab References.", config);
                issues++;
            }

            return issues;
        }
    }
}
