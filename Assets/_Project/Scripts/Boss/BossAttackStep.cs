using System;
using TapKnockout.Enemy;
using TapKnockout.VFX;
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
        [SerializeField, Min(0f)] private float damage;
        [SerializeField, Min(0f)] private float range;
        [SerializeField, Min(0f)] private float radius;
        [SerializeField, Min(0f)] private float chargeSpeed;
        [SerializeField, Min(0)] private int addCount;
        [SerializeField] private EnemyTelegraphType telegraphType;
        [SerializeField] private VFXEventType vfxEventType;

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
            damage = 0f;
            range = 0f;
            radius = 0f;
            chargeSpeed = 0f;
            addCount = 0;
            telegraphType = EnemyTelegraphType.None;
            vfxEventType = VFXEventType.BossPatternTelegraph;
        }

        public BossAttackStep(
            BossAttackType attackType,
            float windupDuration,
            float activeDuration,
            float cooldownDuration,
            float damage,
            float radius,
            float chargeSpeed,
            int addCount,
            EnemyTelegraphType telegraphType,
            VFXEventType vfxEventType = VFXEventType.BossPatternTelegraph,
            float damageMultiplier = 1f)
        {
            this.attackType = attackType;
            this.windupDuration = Mathf.Max(0f, windupDuration);
            this.activeDuration = Mathf.Max(0f, activeDuration);
            this.cooldownDuration = Mathf.Max(0f, cooldownDuration);
            this.damageMultiplier = Mathf.Max(0f, damageMultiplier);
            this.damage = Mathf.Max(0f, damage);
            this.range = 0f;
            this.radius = Mathf.Max(0f, radius);
            this.chargeSpeed = Mathf.Max(0f, chargeSpeed);
            this.addCount = Mathf.Max(0, addCount);
            this.telegraphType = telegraphType;
            this.vfxEventType = vfxEventType;
        }

        public BossAttackType AttackType => attackType;
        public float WindupDuration => windupDuration;
        public float ActiveDuration => activeDuration;
        public float CooldownDuration => cooldownDuration;
        public float DamageMultiplier => damageMultiplier <= 0f ? 1f : damageMultiplier;
        public float Damage => damage;
        public float Range => range;
        public float Radius => radius;
        public float ChargeSpeed => chargeSpeed;
        public int AddCount => addCount;
        public EnemyTelegraphType TelegraphType => telegraphType;
        public VFXEventType VFXEventType => vfxEventType;
        public bool HasTelegraph => telegraphType != EnemyTelegraphType.None && windupDuration > 0f;

        public void ClampValues()
        {
            windupDuration = Mathf.Max(0f, windupDuration);
            activeDuration = Mathf.Max(0f, activeDuration);
            cooldownDuration = Mathf.Max(0f, cooldownDuration);
            damageMultiplier = Mathf.Max(0f, damageMultiplier);
            damage = Mathf.Max(0f, damage);
            range = Mathf.Max(0f, range);
            radius = Mathf.Max(0f, radius);
            chargeSpeed = Mathf.Max(0f, chargeSpeed);
            addCount = Mathf.Max(0, addCount);
        }
    }
}
