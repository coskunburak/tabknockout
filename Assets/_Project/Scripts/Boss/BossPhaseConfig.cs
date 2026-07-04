using System;
using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.Boss
{
    [Serializable]
    public sealed class BossPhaseConfig
    {
        [SerializeField] private BossPhaseState phaseState = BossPhaseState.Phase1;
        [SerializeField, Range(0f, 1f)] private float enterAtOrBelowHealthPercent = 1f;
        [SerializeField] private BossPatternConfig pattern;
        [SerializeField] private bool enrage;
        [SerializeField, Min(0.1f)] private float cooldownDurationMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float chargeSpeedMultiplier = 1f;
        [SerializeField] private VFXEventType phaseChangedVfx = VFXEventType.BossPatternTelegraph;

        public BossPhaseConfig()
        {
        }

        public BossPhaseConfig(
            BossPhaseState phaseState,
            float enterAtOrBelowHealthPercent,
            BossPatternConfig pattern,
            bool enrage = false,
            float cooldownDurationMultiplier = 1f,
            float chargeSpeedMultiplier = 1f)
        {
            this.phaseState = phaseState;
            this.enterAtOrBelowHealthPercent = Mathf.Clamp01(enterAtOrBelowHealthPercent);
            this.pattern = pattern;
            this.enrage = enrage;
            this.cooldownDurationMultiplier = Mathf.Max(0.1f, cooldownDurationMultiplier);
            this.chargeSpeedMultiplier = Mathf.Max(0.1f, chargeSpeedMultiplier);
        }

        public BossPhaseState PhaseState => phaseState;
        public float EnterAtOrBelowHealthPercent => enterAtOrBelowHealthPercent;
        public BossPatternConfig Pattern => pattern;
        public bool Enrage => enrage;
        public float CooldownDurationMultiplier => cooldownDurationMultiplier;
        public float ChargeSpeedMultiplier => chargeSpeedMultiplier;
        public VFXEventType PhaseChangedVfx => phaseChangedVfx;

        public void ClampValues()
        {
            enterAtOrBelowHealthPercent = Mathf.Clamp01(enterAtOrBelowHealthPercent);
            cooldownDurationMultiplier = Mathf.Max(0.1f, cooldownDurationMultiplier);
            chargeSpeedMultiplier = Mathf.Max(0.1f, chargeSpeedMultiplier);
        }
    }
}
