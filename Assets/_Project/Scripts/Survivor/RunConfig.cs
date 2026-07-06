using System;
using System.Collections.Generic;
using TapKnockout.Ability;
using UnityEngine;

namespace TapKnockout.Survivor
{
    [CreateAssetMenu(fileName = "RunConfig", menuName = "Tap Knockout/Survivor/Run Config")]
    public sealed class RunConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string runId = "run_prototype_01";

        [Header("Structure")]
        [SerializeField, Min(30f)] private float targetRunDurationSeconds = 600f;
        [SerializeField, Min(1)] private int startingEnemyCap = 12;
        [SerializeField, Min(1)] private int maxEnemyCap = 80;
        [SerializeField, Min(0f)] private float bossSpawnTimeSeconds = 540f;
        [SerializeField] private float[] eliteSpawnTimes = { 180f, 360f };
        [SerializeField] private AnimationCurve difficultyMultiplierCurve = AnimationCurve.Linear(0f, 1f, 1f, 1.8f);

        [Header("Configs")]
        [SerializeField] private ArenaConfig arenaConfig;
        [SerializeField] private WaveTimelineConfig waveTimeline;
        [SerializeField] private SpawnGroupConfig bossSpawnGroup;

        [Header("XP")]
        [SerializeField] private int[] xpRequirementsPerLevel = { 5, 8, 12, 18, 25, 35, 48, 64, 85, 110 };

        [Header("Ability Pool")]
        [SerializeField] private List<AbilityDefinition> startingAbilityPool = new List<AbilityDefinition>();

        public string RunId => runId;
        public float TargetRunDurationSeconds => targetRunDurationSeconds;
        public int StartingEnemyCap => startingEnemyCap;
        public int MaxEnemyCap => maxEnemyCap;
        public float BossSpawnTimeSeconds => bossSpawnTimeSeconds;
        public IReadOnlyList<float> EliteSpawnTimes => eliteSpawnTimes;
        public AnimationCurve DifficultyMultiplierCurve => difficultyMultiplierCurve;
        public ArenaConfig ArenaConfig => arenaConfig;
        public WaveTimelineConfig WaveTimeline => waveTimeline;
        public SpawnGroupConfig BossSpawnGroup => bossSpawnGroup;
        public IReadOnlyList<int> XPRequirementsPerLevel => xpRequirementsPerLevel;
        public IReadOnlyList<AbilityDefinition> StartingAbilityPool => startingAbilityPool;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(runId))
            {
                runId = "run_prototype_01";
            }

            targetRunDurationSeconds = Mathf.Max(30f, targetRunDurationSeconds);
            startingEnemyCap = Mathf.Max(1, startingEnemyCap);
            maxEnemyCap = Mathf.Max(startingEnemyCap, maxEnemyCap);
            bossSpawnTimeSeconds = Mathf.Clamp(bossSpawnTimeSeconds, 0f, targetRunDurationSeconds);
            eliteSpawnTimes ??= Array.Empty<float>();
            for (var i = 0; i < eliteSpawnTimes.Length; i++)
            {
                eliteSpawnTimes[i] = Mathf.Clamp(eliteSpawnTimes[i], 0f, targetRunDurationSeconds);
            }

            if (xpRequirementsPerLevel == null || xpRequirementsPerLevel.Length == 0)
            {
                xpRequirementsPerLevel = new[] { 5, 8, 12, 18, 25 };
            }

            for (var i = 0; i < xpRequirementsPerLevel.Length; i++)
            {
                xpRequirementsPerLevel[i] = Mathf.Max(1, xpRequirementsPerLevel[i]);
            }

            startingAbilityPool ??= new List<AbilityDefinition>();
        }

        public int GetXPRequiredForLevel(int level)
        {
            var index = Mathf.Max(0, level - 1);
            if (index < xpRequirementsPerLevel.Length)
            {
                return Mathf.Max(1, xpRequirementsPerLevel[index]);
            }

            return xpRequirementsPerLevel.Length > 0 ? xpRequirementsPerLevel[xpRequirementsPerLevel.Length - 1] : 100;
        }

        public float EvaluateDifficultyMultiplier(float elapsedSeconds)
        {
            if (difficultyMultiplierCurve == null)
            {
                return 1f;
            }

            var normalizedTime = targetRunDurationSeconds > 0f
                ? Mathf.Clamp01(elapsedSeconds / targetRunDurationSeconds)
                : 0f;
            return Mathf.Max(0f, difficultyMultiplierCurve.Evaluate(normalizedTime));
        }
    }
}
