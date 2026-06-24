using UnityEngine;

namespace TapKnockout.Enemy
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Tap Knockout/Enemies/Enemy Config")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string enemyId = "melee_chaser_basic";

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
        [SerializeField, Range(0f, 1f)] private float knockbackResistance = 0.2f;
        [SerializeField] private bool canBeKnockedBack = true;
        [SerializeField] private bool canBeInterrupted = true;

        [Header("Reward Placeholders")]
        [SerializeField, Min(0)] private int coinReward = 1;
        [SerializeField, Min(0)] private int xpReward = 1;

        public string EnemyId => enemyId;
        public float MaxHealth => maxHealth;
        public float DeathDelay => deathDelay;
        public float MoveSpeed => moveSpeed;
        public float Acceleration => acceleration;
        public float RotationSpeed => rotationSpeed;
        public float StoppingDistance => stoppingDistance;
        public float ContactDamage => contactDamage;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public float KnockbackResistance => knockbackResistance;
        public bool CanBeKnockedBack => canBeKnockedBack;
        public bool CanBeInterrupted => canBeInterrupted;
        public int CoinReward => coinReward;
        public int XpReward => xpReward;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                enemyId = "melee_chaser_basic";
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
            knockbackResistance = Mathf.Clamp01(knockbackResistance);
            coinReward = Mathf.Max(0, coinReward);
            xpReward = Mathf.Max(0, xpReward);
        }
    }
}
