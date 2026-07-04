using System.Collections.Generic;
using TapKnockout.Enemy;
using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.Boss
{
    [CreateAssetMenu(fileName = "BossConfig", menuName = "Tap Knockout/Boss/Boss Config")]
    public sealed class BossConfig : ScriptableObject
    {
        [SerializeField] private string bossId = "boss_dash_counter_brute";
        [SerializeField] private string displayName = "Dash-Counter Brute";
        [SerializeField] private EnemyConfig enemyConfig;
        [SerializeField] private List<BossPhaseConfig> phases = new List<BossPhaseConfig>
        {
            new BossPhaseConfig(BossPhaseState.Phase1, 1f, null, false, 1f, 1f),
            new BossPhaseConfig(BossPhaseState.Phase2, 0.66f, null, false, 0.9f, 1.05f),
            new BossPhaseConfig(BossPhaseState.Phase3, 0.33f, null, true, 0.72f, 1.18f)
        };

        [Header("Adds")]
        [SerializeField] private EnemyConfig addEnemyConfig;
        [SerializeField] private GameObject addEnemyPrefab;
        [SerializeField, Min(0)] private int maxActiveAdds = 4;

        [Header("VFX Hooks")]
        [SerializeField] private VFXEventType introVfx = VFXEventType.BossWarning;
        [SerializeField] private VFXEventType enrageVfx = VFXEventType.BossPatternTelegraph;
        [SerializeField] private VFXEventType deathVfx = VFXEventType.BossDeath;

        public string BossId => bossId;
        public string DisplayName => displayName;
        public EnemyConfig EnemyConfig => enemyConfig;
        public IReadOnlyList<BossPhaseConfig> Phases => phases;
        public EnemyConfig AddEnemyConfig => addEnemyConfig;
        public GameObject AddEnemyPrefab => addEnemyPrefab;
        public int MaxActiveAdds => maxActiveAdds;
        public VFXEventType IntroVfx => introVfx;
        public VFXEventType EnrageVfx => enrageVfx;
        public VFXEventType DeathVfx => deathVfx;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(bossId))
            {
                bossId = "boss_dash_counter_brute";
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = bossId;
            }

            maxActiveAdds = Mathf.Max(0, maxActiveAdds);
            phases ??= new List<BossPhaseConfig>();
            for (var i = 0; i < phases.Count; i++)
            {
                phases[i]?.ClampValues();
            }
        }

        public BossPhaseConfig ResolvePhaseForHealthPercent(float healthPercent)
        {
            return ResolvePhaseForHealthPercent(phases, healthPercent);
        }

        public void SetPhases(IEnumerable<BossPhaseConfig> phaseConfigs)
        {
            phases.Clear();
            if (phaseConfigs == null)
            {
                return;
            }

            foreach (var phase in phaseConfigs)
            {
                if (phase == null)
                {
                    continue;
                }

                phase.ClampValues();
                phases.Add(phase);
            }
        }

        public static BossPhaseConfig ResolvePhaseForHealthPercent(IReadOnlyList<BossPhaseConfig> phaseConfigs, float healthPercent)
        {
            if (phaseConfigs == null || phaseConfigs.Count == 0)
            {
                return null;
            }

            var safeHealthPercent = Mathf.Clamp01(healthPercent);
            BossPhaseConfig selected = null;
            var selectedThreshold = 2f;

            for (var i = 0; i < phaseConfigs.Count; i++)
            {
                var phase = phaseConfigs[i];
                if (phase == null)
                {
                    continue;
                }

                var threshold = phase.EnterAtOrBelowHealthPercent;
                if (safeHealthPercent <= threshold && threshold < selectedThreshold)
                {
                    selected = phase;
                    selectedThreshold = threshold;
                }
            }

            return selected ?? phaseConfigs[0];
        }
    }
}
