using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Survivor
{
    [CreateAssetMenu(fileName = "WaveTimelineConfig", menuName = "Tap Knockout/Survivor/Wave Timeline Config")]
    public sealed class WaveTimelineConfig : ScriptableObject
    {
        [Serializable]
        public sealed class WaveTimelineEntry
        {
            [SerializeField, Min(0f)] private float startTime;
            [SerializeField, Min(0f)] private float endTime = 60f;
            [SerializeField, Min(0.05f)] private float spawnInterval = 2f;
            [SerializeField, Min(1)] private int liveEnemyCap = 12;
            [SerializeField, Min(0f)] private float intensityMultiplier = 1f;
            [SerializeField] private List<SpawnGroupConfig> spawnGroups = new List<SpawnGroupConfig>();

            public float StartTime => startTime;
            public float EndTime => endTime;
            public float SpawnInterval => spawnInterval;
            public int LiveEnemyCap => liveEnemyCap;
            public float IntensityMultiplier => intensityMultiplier;
            public IReadOnlyList<SpawnGroupConfig> SpawnGroups => spawnGroups;
            public bool IsActive(float elapsedSeconds) => elapsedSeconds >= startTime && elapsedSeconds < endTime;

            public void Validate()
            {
                startTime = Mathf.Max(0f, startTime);
                endTime = Mathf.Max(startTime + 0.05f, endTime);
                spawnInterval = Mathf.Max(0.05f, spawnInterval);
                liveEnemyCap = Mathf.Max(1, liveEnemyCap);
                intensityMultiplier = Mathf.Max(0f, intensityMultiplier);
                spawnGroups ??= new List<SpawnGroupConfig>();
            }
        }

        [Header("Identity")]
        [SerializeField] private string timelineId = "wave_timeline_prototype_01";

        [Header("Segments")]
        [SerializeField] private List<WaveTimelineEntry> entries = new List<WaveTimelineEntry>();

        [Header("Milestones")]
        [SerializeField, Min(0f)] private float bossWarningTimeSeconds = 510f;

        public string TimelineId => timelineId;
        public IReadOnlyList<WaveTimelineEntry> Entries => entries;
        public float BossWarningTimeSeconds => bossWarningTimeSeconds;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(timelineId))
            {
                timelineId = "wave_timeline_prototype_01";
            }

            entries ??= new List<WaveTimelineEntry>();
            for (var i = 0; i < entries.Count; i++)
            {
                entries[i]?.Validate();
            }

            bossWarningTimeSeconds = Mathf.Max(0f, bossWarningTimeSeconds);
        }

        public WaveTimelineEntry GetActiveEntry(float elapsedSeconds)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && entries[i].IsActive(elapsedSeconds))
                {
                    return entries[i];
                }
            }

            return null;
        }
    }
}
