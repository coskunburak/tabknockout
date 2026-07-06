using UnityEngine;
using TapKnockout.VFX;

namespace TapKnockout.Enemy
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Tap Knockout/Enemies/Enemy Config")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string enemyId = "melee_chaser_basic";
        [SerializeField] private string displayName = "Melee Chaser";
        [SerializeField] private EnemyArchetype archetype = EnemyArchetype.MeleeChaser;
        [SerializeField] private EnemyRank rank = EnemyRank.Normal;

        [Header("Health")]
        [SerializeField, Min(1f)] private float maxHealth = 40f;
        [SerializeField, Min(0f)] private float deathDelay = 0.3f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 2.2f;
        [SerializeField, Min(0f)] private float acceleration = 18f;
        [SerializeField, Min(0f)] private float rotationSpeed = 720f;
        [SerializeField, Min(0f)] private float stoppingDistance = 1.1f;

        [Header("Combat")]
        [SerializeField, Min(0f)] private float contactDamage = 8f;
        [SerializeField, Min(0f)] private float attackRange = 1.2f;
        [SerializeField, Min(0.01f)] private float attackCooldown = 1f;
        [SerializeField, Min(0f)] private float attackWindup = 0.25f;
        [SerializeField, Min(0f)] private float projectileSpeed = 8f;
        [SerializeField, Min(0)] private int projectileCount = 1;
        [SerializeField, Min(0f)] private float explosionRadius = 1.5f;
        [SerializeField, Range(0f, 1f)] private float stunResistance;
        [SerializeField, Range(0f, 1f)] private float knockbackResistance = 0.2f;
        [SerializeField] private bool canBeKnockedBack = true;
        [SerializeField] private bool canBeInterrupted = true;
        [SerializeField] private EnemyAttackPatternConfig attackPattern;
        [SerializeField] private EnemyTelegraphConfig telegraphConfig;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject splitSpawnPrefab;
        [SerializeField, Min(0)] private int splitSpawnCount;
        [SerializeField, Range(0f, 180f)] private float shieldBlockAngle = 120f;
        [SerializeField, Range(0f, 1f)] private float shieldDamageReduction = 0.65f;

        [Header("Behavior Tuning")]
        [SerializeField, Min(0f)] private float chargeSpeedMultiplier = 2.8f;
        [SerializeField, Min(0f)] private float chargeDuration = 0.45f;
        [SerializeField, Min(0f)] private float chargeRecoveryDuration = 0.55f;

        [Header("Reward Placeholders")]
        [SerializeField, Min(0)] private int coinReward = 1;
        [SerializeField, Min(0)] private int xpReward = 1;

        [Header("VFX Hooks")]
        [SerializeField] private VFXEventType spawnVfx = VFXEventType.EnemyTelegraph;
        [SerializeField] private VFXEventType attackVfx = VFXEventType.EnemyAttackRelease;
        [SerializeField] private VFXEventType deathVfx = VFXEventType.EnemyDeath;

        public string EnemyId => enemyId;
        public string DisplayName => displayName;
        public EnemyArchetype Archetype => archetype;
        public EnemyRank Rank => rank;
        public float MaxHealth => maxHealth;
        public float DeathDelay => deathDelay;
        public float MoveSpeed => moveSpeed;
        public float Acceleration => acceleration;
        public float RotationSpeed => rotationSpeed;
        public float StoppingDistance => stoppingDistance;
        public float ContactDamage => contactDamage;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public float AttackWindup => attackWindup;
        public float ProjectileSpeed => projectileSpeed;
        public int ProjectileCount => projectileCount;
        public float ExplosionRadius => explosionRadius;
        public float StunResistance => stunResistance;
        public float KnockbackResistance => knockbackResistance;
        public bool CanBeKnockedBack => canBeKnockedBack;
        public bool CanBeInterrupted => canBeInterrupted;
        public EnemyAttackPatternConfig AttackPattern => attackPattern;
        public EnemyTelegraphConfig TelegraphConfig => telegraphConfig;
        public GameObject ProjectilePrefab => projectilePrefab;
        public GameObject SplitSpawnPrefab => splitSpawnPrefab;
        public int SplitSpawnCount => splitSpawnCount;
        public float ShieldBlockAngle => shieldBlockAngle;
        public float ShieldDamageReduction => shieldDamageReduction;
        public float ChargeSpeedMultiplier => chargeSpeedMultiplier;
        public float ChargeDuration => chargeDuration;
        public float ChargeRecoveryDuration => chargeRecoveryDuration;
        public int CoinReward => coinReward;
        public int XpReward => xpReward;
        public VFXEventType SpawnVfx => spawnVfx;
        public VFXEventType AttackVfx => attackVfx;
        public VFXEventType DeathVfx => deathVfx;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                enemyId = "melee_chaser_basic";
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = enemyId;
            }

            maxHealth = Mathf.Max(1f, maxHealth);
            deathDelay = Mathf.Max(0f, deathDelay);
            moveSpeed = Mathf.Max(0f, moveSpeed);
            acceleration = Mathf.Max(0f, acceleration);
            rotationSpeed = Mathf.Max(0f, rotationSpeed);
            stoppingDistance = Mathf.Max(0f, stoppingDistance);
            contactDamage = Mathf.Max(0f, contactDamage);
            attackRange = Mathf.Max(0f, attackRange);
            attackCooldown = Mathf.Max(0.01f, attackCooldown);
            attackWindup = Mathf.Max(0f, attackWindup);
            projectileSpeed = Mathf.Max(0f, projectileSpeed);
            projectileCount = Mathf.Max(0, projectileCount);
            explosionRadius = Mathf.Max(0f, explosionRadius);
            stunResistance = Mathf.Clamp01(stunResistance);
            knockbackResistance = Mathf.Clamp01(knockbackResistance);
            splitSpawnCount = Mathf.Max(0, splitSpawnCount);
            shieldBlockAngle = Mathf.Clamp(shieldBlockAngle, 0f, 180f);
            shieldDamageReduction = Mathf.Clamp01(shieldDamageReduction);
            chargeSpeedMultiplier = Mathf.Max(0f, chargeSpeedMultiplier);
            chargeDuration = Mathf.Max(0f, chargeDuration);
            chargeRecoveryDuration = Mathf.Max(0f, chargeRecoveryDuration);
            coinReward = Mathf.Max(0, coinReward);
            xpReward = Mathf.Max(0, xpReward);
        }
    }
}
