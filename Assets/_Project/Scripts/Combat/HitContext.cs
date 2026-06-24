using System;
using UnityEngine;

namespace TapKnockout.Combat
{
    /// <summary>
    /// Runtime payload for a single combat hit. It stays mutable so dash, projectile, ability, and
    /// damage-modifier systems can enrich the same context before resolution.
    /// </summary>
    [Serializable]
    public sealed class HitContext
    {
        public GameObject Source { get; set; }
        public GameObject Target { get; set; }
        public float DamageAmount { get; set; }
        public DamageType DamageType { get; set; }
        public float CriticalChance { get; set; }
        public float CriticalMultiplier { get; set; }
        public bool IsCritical { get; set; }
        public bool IsDashHit { get; set; }
        public bool IsProjectileHit { get; set; }
        public bool IsAbilityHit { get; set; }
        public bool WasIgnored { get; set; }
        public string AbilityId { get; set; }
        public KnockbackData Knockback { get; set; }
        public Vector3 HitPoint { get; set; }
        public Vector3 HitDirection { get; set; }

        public HitContext()
        {
            DamageType = DamageType.Physical;
            CriticalMultiplier = 1f;
            WasIgnored = false;
            AbilityId = string.Empty;
            Knockback = KnockbackData.None;
            HitPoint = Vector3.zero;
            HitDirection = Vector3.zero;
        }

        public HitContext(GameObject source, GameObject target, float damageAmount, DamageType damageType = DamageType.Physical)
            : this()
        {
            Source = source;
            Target = target;
            DamageAmount = Mathf.Max(0f, damageAmount);
            DamageType = damageType;
        }

        public bool HasSource => Source != null;
        public bool HasTarget => Target != null;
        public bool HasAbilitySource => !string.IsNullOrWhiteSpace(AbilityId);
    }
}
