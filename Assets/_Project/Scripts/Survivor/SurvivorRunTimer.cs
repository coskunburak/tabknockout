using System;
using UnityEngine;

namespace TapKnockout.Survivor
{
    [Serializable]
    public sealed class SurvivorRunTimer
    {
        [SerializeField, Min(1f)] private float durationSeconds = 600f;

        public float DurationSeconds => durationSeconds;
        public float ElapsedSeconds { get; private set; }
        public float RemainingSeconds => Mathf.Max(0f, durationSeconds - ElapsedSeconds);
        public float NormalizedTime => durationSeconds > 0f ? Mathf.Clamp01(ElapsedSeconds / durationSeconds) : 1f;
        public bool IsComplete => durationSeconds > 0f && ElapsedSeconds >= durationSeconds;

        public void Configure(float targetDurationSeconds)
        {
            durationSeconds = Mathf.Max(1f, targetDurationSeconds);
            ElapsedSeconds = Mathf.Min(ElapsedSeconds, durationSeconds);
        }

        public void Reset()
        {
            ElapsedSeconds = 0f;
        }

        public void Tick(float deltaTime)
        {
            ElapsedSeconds = Mathf.Min(durationSeconds, ElapsedSeconds + Mathf.Max(0f, deltaTime));
        }
    }
}
