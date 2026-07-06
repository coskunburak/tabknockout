using TapKnockout.Combat;
using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.Enemy
{
    /// <summary>
    /// Data-driven ScriptableObject defining one distinct attack behavior for an enemy.
    /// Each enemy has one or more of these assigned to EnemyDistinctAttackController.
    /// Designer-tunable, validated by EnemyAttackMechanicsValidator.
    /// </summary>
    [CreateAssetMenu(fileName = "AC_Enemy_Attack", menuName = "Tap Knockout/Enemies/Enemy Attack Config")]
    public sealed class EnemyAttackConfig : ScriptableObject
    {
        // ─── Identity ─────────────────────────────────────────────────────────
        [Header("Identity")]
        [SerializeField] private string attackId = "enemy_attack_default";
        [SerializeField] private string displayName = "Enemy Attack";
        [SerializeField] private EnemyDistinctAttackType attackType = EnemyDistinctAttackType.MeleeArc;

        // ─── Range ────────────────────────────────────────────────────────────
        [Header("Range")]
        [SerializeField, Min(0f)] private float triggerRange = 1.5f;
        [Tooltip("For ranged/projectile enemies — stay at least this far from player.")]
        [SerializeField, Min(0f)] private float preferredMinRange;
        [Tooltip("For ranged/projectile enemies — stay no farther than this.")]
        [SerializeField, Min(0f)] private float preferredMaxRange = 8f;

        // ─── Timing ───────────────────────────────────────────────────────────
        [Header("Timing (seconds)")]
        [SerializeField, Min(0.01f)] private float cooldown = 2f;
        [SerializeField, Min(0f)] private float initialCooldownOffset;
        [SerializeField, Min(0f)] private float windupTime = 0.4f;
        [SerializeField, Min(0f)] private float activeTime = 0.15f;
        [SerializeField, Min(0f)] private float recoveryTime = 0.6f;

        // ─── Movement during attack ───────────────────────────────────────────
        [Header("Movement Locks")]
        [SerializeField] private bool canMoveDuringWindup = true;
        [SerializeField] private bool commitLocksMovement = true;
        [SerializeField] private bool canMoveDuringRecovery = true;
        [SerializeField] private bool commitLocksRotation = true;

        // ─── Damage ───────────────────────────────────────────────────────────
        [Header("Damage")]
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0f)] private float knockbackForce = 4f;
        [SerializeField, Min(0f)] private float knockbackDuration = 0.15f;

        // ─── Hitbox ───────────────────────────────────────────────────────────
        [Header("Hitbox Shape")]
        [SerializeField] private EnemyHitboxShape hitboxShape = EnemyHitboxShape.Circle;
        [SerializeField, Min(0f)] private float hitboxRadius = 1.2f;
        [SerializeField, Min(0f)] private float hitboxLength = 3f;
        [Tooltip("Arc/cone half-angle in degrees (0–180).")]
        [SerializeField, Range(0f, 180f)] private float hitboxArcHalfAngle = 60f;
        [SerializeField] private LayerMask hitLayerMask = ~0;

        // ─── Projectile ───────────────────────────────────────────────────────
        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField, Min(0f)] private float projectileSpeed = 8f;
        [SerializeField, Min(0.01f)] private float projectileLifetime = 4f;
        [Tooltip("Mild homing — 0 = straight, 1 = tight tracking.")]
        [SerializeField, Range(0f, 1f)] private float homingStrength;
        [SerializeField, Min(0f)] private float homingMaxTurnDegreesPerSecond = 45f;

        // ─── Area Zone ────────────────────────────────────────────────────────
        [Header("Area Zone")]
        [SerializeField] private GameObject areaZonePrefab;
        [SerializeField, Min(0f)] private float areaZoneRadius = 2f;
        [SerializeField, Min(0.01f)] private float areaZoneDuration = 4f;
        [SerializeField, Min(0.05f)] private float areaZoneTickInterval = 1f;
        [SerializeField, Min(0f)] private float areaZoneTickDamage;
        [Tooltip("Max simultaneously active zones per enemy instance.")]
        [SerializeField, Min(1)] private int maxActiveZones = 3;

        // ─── Status Effect ────────────────────────────────────────────────────
        [Header("Status Effect")]
        [SerializeField] private StatusEffectType statusEffectType = StatusEffectType.None;
        [SerializeField, Min(0f)] private float statusEffectDuration = 2f;
        [Tooltip("Slow/freeze multiplier — 1 = no slow, 0 = full stop.")]
        [SerializeField, Range(0f, 1f)] private float statusEffectSlowMultiplier = 0.6f;

        // ─── Animation ────────────────────────────────────────────────────────
        [Header("Animation")]
        [Tooltip("Animator trigger name for attack windup.")]
        [SerializeField] private string animationTrigger = "Attack";
        [SerializeField] private bool useAnimationTrigger = true;

        // ─── VFX/SFX ─────────────────────────────────────────────────────────
        [Header("VFX / Telegraph")]
        [SerializeField] private GameObject telegraphPrefab;
        [SerializeField] private GameObject activeVfxPrefab;
        [SerializeField] private GameObject impactVfxPrefab;
        [SerializeField, Min(0.05f)] private float vfxLifetime = 1.25f;
        [SerializeField] private EnemyTelegraphType telegraphType = EnemyTelegraphType.Circle;
        [SerializeField] private Color debugColor = Color.red;

        // ─── Dive / Leap special params ───────────────────────────────────────
        [Header("Dive / Leap Params")]
        [Tooltip("Dive or leap speed multiplier on top of base move speed.")]
        [SerializeField, Min(0f)] private float diveSpeedMultiplier = 4f;
        [Tooltip("Overshooting distance beyond target (Dive/Leap).")]
        [SerializeField, Min(0f)] private float overshootDistance = 3f;

        // ─── Beam params ─────────────────────────────────────────────────────
        [Header("Beam Params")]
        [SerializeField, Min(0f)] private float beamLength = 8f;
        [SerializeField, Min(0f)] private float beamWidth = 0.35f;

        // ─── Public Accessors ─────────────────────────────────────────────────
        public string AttackId => attackId;
        public string DisplayName => displayName;
        public EnemyDistinctAttackType AttackType => attackType;

        public float TriggerRange => triggerRange;
        public float PreferredMinRange => preferredMinRange;
        public float PreferredMaxRange => preferredMaxRange;

        public float Cooldown => cooldown;
        public float InitialCooldownOffset => initialCooldownOffset;
        public float WindupTime => windupTime;
        public float ActiveTime => activeTime;
        public float RecoveryTime => recoveryTime;

        public bool CanMoveDuringWindup => canMoveDuringWindup;
        public bool CommitLocksMovement => commitLocksMovement;
        public bool CanMoveDuringRecovery => canMoveDuringRecovery;
        public bool CommitLocksRotation => commitLocksRotation;

        public float Damage => damage;
        public float KnockbackForce => knockbackForce;
        public float KnockbackDuration => knockbackDuration;

        public EnemyHitboxShape HitboxShape => hitboxShape;
        public float HitboxRadius => hitboxRadius;
        public float HitboxLength => hitboxLength;
        public float HitboxArcHalfAngle => hitboxArcHalfAngle;
        public LayerMask HitLayerMask => hitLayerMask;

        public GameObject ProjectilePrefab => projectilePrefab;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public float HomingStrength => homingStrength;
        public float HomingMaxTurnDegreesPerSecond => homingMaxTurnDegreesPerSecond;

        public GameObject AreaZonePrefab => areaZonePrefab;
        public float AreaZoneRadius => areaZoneRadius;
        public float AreaZoneDuration => areaZoneDuration;
        public float AreaZoneTickInterval => areaZoneTickInterval;
        public float AreaZoneTickDamage => areaZoneTickDamage;
        public int MaxActiveZones => maxActiveZones;

        public StatusEffectType StatusEffectType => statusEffectType;
        public float StatusEffectDuration => statusEffectDuration;
        public float StatusEffectSlowMultiplier => statusEffectSlowMultiplier;

        public string AnimationTrigger => animationTrigger;
        public bool UseAnimationTrigger => useAnimationTrigger;

        public GameObject TelegraphPrefab => telegraphPrefab;
        public GameObject ActiveVfxPrefab => activeVfxPrefab;
        public GameObject ImpactVfxPrefab => impactVfxPrefab;
        public float VfxLifetime => vfxLifetime;
        public EnemyTelegraphType TelegraphType => telegraphType;
        public Color DebugColor => debugColor;

        public float DiveSpeedMultiplier => diveSpeedMultiplier;
        public float OvershootDistance => overshootDistance;
        public float BeamLength => beamLength;
        public float BeamWidth => beamWidth;

        // ─── Derived helpers ──────────────────────────────────────────────────
        public bool NeedsProjectile => attackType == EnemyDistinctAttackType.Projectile
            || attackType == EnemyDistinctAttackType.SpikeProjectile
            || attackType == EnemyDistinctAttackType.SlimeProjectileArea
            || attackType == EnemyDistinctAttackType.HomingProjectile;

        public bool NeedsAreaZone => attackType == EnemyDistinctAttackType.SlimeProjectileArea
            || attackType == EnemyDistinctAttackType.SporeZone
            || attackType == EnemyDistinctAttackType.FrostSlamShockwave;

        public bool HasStatusEffect => statusEffectType != StatusEffectType.None && statusEffectDuration > 0f;

        // ─── Validation ───────────────────────────────────────────────────────
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(attackId))
            {
                attackId = "enemy_attack_default";
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = attackId;
            }

            triggerRange = Mathf.Max(0f, triggerRange);
            preferredMinRange = Mathf.Max(0f, preferredMinRange);
            preferredMaxRange = Mathf.Max(0f, preferredMaxRange);
            cooldown = Mathf.Max(0.01f, cooldown);
            initialCooldownOffset = Mathf.Max(0f, initialCooldownOffset);
            windupTime = Mathf.Max(0f, windupTime);
            activeTime = Mathf.Max(0f, activeTime);
            recoveryTime = Mathf.Max(0f, recoveryTime);
            damage = Mathf.Max(0f, damage);
            knockbackForce = Mathf.Max(0f, knockbackForce);
            knockbackDuration = Mathf.Max(0f, knockbackDuration);
            hitboxRadius = Mathf.Max(0f, hitboxRadius);
            hitboxLength = Mathf.Max(0f, hitboxLength);
            hitboxArcHalfAngle = Mathf.Clamp(hitboxArcHalfAngle, 0f, 180f);
            projectileSpeed = Mathf.Max(0f, projectileSpeed);
            projectileLifetime = Mathf.Max(0.01f, projectileLifetime);
            homingStrength = Mathf.Clamp01(homingStrength);
            homingMaxTurnDegreesPerSecond = Mathf.Max(0f, homingMaxTurnDegreesPerSecond);
            areaZoneRadius = Mathf.Max(0f, areaZoneRadius);
            areaZoneDuration = Mathf.Max(0.01f, areaZoneDuration);
            areaZoneTickInterval = Mathf.Max(0.05f, areaZoneTickInterval);
            areaZoneTickDamage = Mathf.Max(0f, areaZoneTickDamage);
            maxActiveZones = Mathf.Max(1, maxActiveZones);
            statusEffectDuration = Mathf.Max(0f, statusEffectDuration);
            statusEffectSlowMultiplier = Mathf.Clamp01(statusEffectSlowMultiplier);
            vfxLifetime = Mathf.Max(0.05f, vfxLifetime);
            diveSpeedMultiplier = Mathf.Max(0f, diveSpeedMultiplier);
            overshootDistance = Mathf.Max(0f, overshootDistance);
            beamLength = Mathf.Max(0f, beamLength);
            beamWidth = Mathf.Max(0f, beamWidth);

            if (string.IsNullOrWhiteSpace(animationTrigger))
            {
                animationTrigger = "Attack";
            }
        }
    }
}
