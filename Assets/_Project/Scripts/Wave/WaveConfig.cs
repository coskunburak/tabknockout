using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Wave
{
    [CreateAssetMenu(fileName = "WaveConfig", menuName = "Tap Knockout/Waves/Wave Config")]
    public sealed class WaveConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string waveId = "wave_001";

        [Header("Spawn")]
        [SerializeField] private List<WaveEnemyEntry> enemies = new List<WaveEnemyEntry>();
        [SerializeField, Min(0f)] private float startDelay = 0.5f;

        [Header("Clear Condition")]
        [SerializeField] private bool completeWhenAllSpawnedEnemiesDead = true;

        public string WaveId => waveId;
        public IReadOnlyList<WaveEnemyEntry> Enemies => enemies;
        public float StartDelay => startDelay;
        public bool CompleteWhenAllSpawnedEnemiesDead => completeWhenAllSpawnedEnemiesDead;
        public bool HasEnemyEntries => enemies != null && enemies.Count > 0;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(waveId))
            {
                waveId = "wave_001";
            }

            startDelay = Mathf.Max(0f, startDelay);
            enemies ??= new List<WaveEnemyEntry>();

            for (var i = 0; i < enemies.Count; i++)
            {
                enemies[i]?.Validate();
            }
        }
    }
}
