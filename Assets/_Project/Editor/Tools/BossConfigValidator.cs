using TapKnockout.Boss;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class BossConfigValidator
    {
        private const string MenuPath = "Tools/Tap Knockout/Bosses/Validate Boss Configs";
        private const string BossFolder = "Assets/_Project/ScriptableObjects/Bosses";

        [MenuItem(MenuPath)]
        public static void ValidateBossConfigs()
        {
            var guids = AssetDatabase.FindAssets("t:BossConfig", new[] { BossFolder });
            var issueCount = 0;
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var config = AssetDatabase.LoadAssetAtPath<BossConfig>(path);
                if (config != null)
                {
                    issueCount += ValidateBossConfig(config, path);
                }
            }

            var patternGuids = AssetDatabase.FindAssets("t:BossPatternConfig", new[] { BossFolder });
            for (var i = 0; i < patternGuids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(patternGuids[i]);
                var pattern = AssetDatabase.LoadAssetAtPath<BossPatternConfig>(path);
                if (pattern != null)
                {
                    issueCount += ValidatePattern(pattern, path);
                }
            }

            var message = issueCount == 0
                ? $"Boss config validation passed for {guids.Length} boss config(s) and {patternGuids.Length} pattern(s)."
                : $"Boss config validation found {issueCount} issue(s). Check Window > General > Console.";
            Debug.Log(message);
            EditorUtility.DisplayDialog("Validate Boss Configs", message, "OK");
        }

        private static int ValidateBossConfig(BossConfig config, string path)
        {
            var issues = 0;
            if (string.IsNullOrWhiteSpace(config.BossId))
            {
                Debug.LogError($"BossConfig missing id: {path}", config);
                issues++;
            }

            if (config.EnemyConfig == null)
            {
                Debug.LogWarning($"BossConfig has no EnemyConfig assigned yet: {path}", config);
                issues++;
            }

            if (config.AddEnemyConfig == null)
            {
                Debug.LogWarning($"BossConfig has no add EnemyConfig assigned yet: {path}", config);
                issues++;
            }

            if (config.AddEnemyPrefab == null)
            {
                Debug.LogWarning($"BossConfig has no add enemy prefab assigned yet: {path}", config);
                issues++;
            }

            if (config.Phases == null || config.Phases.Count < 3)
            {
                Debug.LogError($"BossConfig should define at least 3 phases: {path}", config);
                issues++;
            }

            var previousThreshold = 2f;
            for (var i = 0; config.Phases != null && i < config.Phases.Count; i++)
            {
                var phase = config.Phases[i];
                if (phase == null)
                {
                    Debug.LogError($"BossConfig has null phase at index {i}: {path}", config);
                    issues++;
                    continue;
                }

                if (phase.EnterAtOrBelowHealthPercent > previousThreshold)
                {
                    Debug.LogError($"BossConfig phase thresholds must be descending: {path}", config);
                    issues++;
                }

                previousThreshold = phase.EnterAtOrBelowHealthPercent;

                if (phase.Pattern == null)
                {
                    Debug.LogWarning($"BossConfig phase {phase.PhaseState} has no pattern assigned yet: {path}", config);
                }
            }

            return issues;
        }

        private static int ValidatePattern(BossPatternConfig pattern, string path)
        {
            var issues = 0;
            if (pattern.Steps == null || pattern.Steps.Count == 0)
            {
                Debug.LogError($"BossPatternConfig has no steps: {path}", pattern);
                return 1;
            }

            for (var i = 0; i < pattern.Steps.Count; i++)
            {
                var step = pattern.Steps[i];
                if (step.WindupDuration < 0f || step.ActiveDuration < 0f || step.CooldownDuration < 0f)
                {
                    Debug.LogError($"BossPatternConfig has negative timing at step {i}: {path}", pattern);
                    issues++;
                }

                if (step.AttackType != BossAttackType.IdleWait && step.WindupDuration <= 0f)
                {
                    Debug.LogWarning($"BossPatternConfig step {i} has no windup telegraph window: {path}", pattern);
                    issues++;
                }
            }

            return issues;
        }
    }
}
