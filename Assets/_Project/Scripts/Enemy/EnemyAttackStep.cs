using System;
using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [Serializable]
    public struct EnemyAttackStep
    {
        [SerializeField] private EnemyAttackType attackType;
        [SerializeField, Min(0f)] private float windupDuration;
        [SerializeField, Min(0f)] private float activeDuration;
        [SerializeField, Min(0f)] private float cooldownDuration;
        [SerializeField, Min(0f)] private float damage;
        [SerializeField, Min(0f)] private float range;
        [SerializeField, Min(0f)] private float radius;
        [SerializeField, Min(0)] private int projectileCount;
        [SerializeField, Min(0f)] private float projectileSpeed;
        [SerializeField] private EnemyTelegraphType telegraphType;
        [SerializeField] private VFXEventType vfxEventType;
        [SerializeField] private bool canBeInterrupted;
        [SerializeField] private bool requiresLineOfSight;

        public EnemyAttackStep(
            EnemyAttackType attackType,
            float windupDuration,
            float activeDuration,
            float cooldownDuration,
            float damage,
            float range,
            float radius,
            int projectileCount,
            float projectileSpeed,
            EnemyTelegraphType telegraphType,
            VFXEventType vfxEventType = VFXEventType.EnemyAttackRelease,
            bool canBeInterrupted = true,
            bool requiresLineOfSight = false)
        {
            this.attackType = attackType;
            this.windupDuration = Mathf.Max(0f, windupDuration);
            this.activeDuration = Mathf.Max(0f, activeDuration);
            this.cooldownDuration = Mathf.Max(0f, cooldownDuration);
            this.damage = Mathf.Max(0f, damage);
            this.range = Mathf.Max(0f, range);
            this.radius = Mathf.Max(0f, radius);
            this.projectileCount = Mathf.Max(0, projectileCount);
            this.projectileSpeed = Mathf.Max(0f, projectileSpeed);
            this.telegraphType = telegraphType;
            this.vfxEventType = vfxEventType;
            this.canBeInterrupted = canBeInterrupted;
            this.requiresLineOfSight = requiresLineOfSight;
        }

        public EnemyAttackType AttackType => attackType;
        public float WindupDuration => windupDuration;
        public float ActiveDuration => activeDuration;
        public float CooldownDuration => cooldownDuration;
        public float Damage => damage;
        public float Range => range;
        public float Radius => radius;
        public int ProjectileCount => projectileCount;
        public float ProjectileSpeed => projectileSpeed;
        public EnemyTelegraphType TelegraphType => telegraphType;
        public VFXEventType VFXEventType => vfxEventType;
        public bool CanBeInterrupted => canBeInterrupted;
        public bool RequiresLineOfSight => requiresLineOfSight;

        public float TotalDuration => windupDuration + activeDuration + cooldownDuration;
        public bool HasReadableTelegraph => telegraphType == EnemyTelegraphType.None || windupDuration > 0f;

        public void ClampValues()
        {
            windupDuration = Mathf.Max(0f, windupDuration);
            activeDuration = Mathf.Max(0f, activeDuration);
            cooldownDuration = Mathf.Max(0f, cooldownDuration);
            damage = Mathf.Max(0f, damage);
            range = Mathf.Max(0f, range);
            radius = Mathf.Max(0f, radius);
            projectileCount = Mathf.Max(0, projectileCount);
            projectileSpeed = Mathf.Max(0f, projectileSpeed);
        }
    }
}
