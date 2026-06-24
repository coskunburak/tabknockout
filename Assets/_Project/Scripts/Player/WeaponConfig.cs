using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Player
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Tap Knockout/Weapons/Weapon Config")]
    public sealed class WeaponConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string weaponId = "starter_bolt";

        [Header("Attack")]
        [SerializeField, Min(0f)] private float attackDamage = 10f;
        [SerializeField, Min(0.01f)] private float attackCooldown = 0.8f;
        [SerializeField, Min(0.1f)] private float attackRange = 8f;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField, Range(0f, 1f)] private float criticalChance;
        [SerializeField, Min(1f)] private float criticalMultiplier = 1.5f;

        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField, Min(0f)] private float projectileSpeed = 12f;
        [SerializeField, Min(0.01f)] private float projectileLifetime = 3f;

        [Header("Targeting")]
        [SerializeField] private LayerMask targetLayers;

        public string WeaponId => weaponId;
        public float AttackDamage => attackDamage;
        public float AttackCooldown => attackCooldown;
        public float AttackRange => attackRange;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public DamageType DamageType => damageType;
        public GameObject ProjectilePrefab => projectilePrefab;
        public LayerMask TargetLayers => targetLayers;
        public float CriticalChance => criticalChance;
        public float CriticalMultiplier => criticalMultiplier;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(weaponId))
            {
                weaponId = "starter_bolt";
            }

            attackDamage = Mathf.Max(0f, attackDamage);
            attackCooldown = Mathf.Max(0.01f, attackCooldown);
            attackRange = Mathf.Max(0.1f, attackRange);
            projectileSpeed = Mathf.Max(0f, projectileSpeed);
            projectileLifetime = Mathf.Max(0.01f, projectileLifetime);
            criticalChance = Mathf.Clamp01(criticalChance);
            criticalMultiplier = Mathf.Max(1f, criticalMultiplier);
        }
    }
}
