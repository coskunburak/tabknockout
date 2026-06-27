using System;
using UnityEngine;

namespace TapKnockout.Boss
{
    [Serializable]
    public struct BossAttackStep
    {
        [SerializeField] private BossAttackType attackType;
        [SerializeField, Min(0f)] private float windupDuration;
        [SerializeField, Min(0f)] private float activeDuration;
        [SerializeField, Min(0f)] private float cooldownDuration;
        [SerializeField, Min(0f)] private float damageMultiplier;

        public BossAttackStep(
            BossAttackType attackType,
            float windupDuration,
            float activeDuration,
            float cooldownDuration,
            float damageMultiplier = 1f)
        {
            this.attackType = attackType;
            this.windupDuration = Mathf.Max(0f, windupDuration);
            this.activeDuration = Mathf.Max(0f, activeDuration);
            this.cooldownDuration = Mathf.Max(0f, cooldownDuration);
            this.damageMultiplier = Mathf.Max(0f, damageMultiplier);
        }

        public BossAttackType AttackType => attackType;
        public float WindupDuration => windupDuration;
        public float ActiveDuration => activeDuration;
        public float CooldownDuration => cooldownDuration;
        public float DamageMultiplier => damageMultiplier <= 0f ? 1f : damageMultiplier;

        public void ClampValues()
        {
            windupDuration = Mathf.Max(0f, windupDuration);
            activeDuration = Mathf.Max(0f, activeDuration);
            cooldownDuration = Mathf.Max(0f, cooldownDuration);
            damageMultiplier = Mathf.Max(0f, damageMultiplier);
        }
    }
}
