using UnityEngine;

namespace TapKnockout.Enemy
{
    [CreateAssetMenu(fileName = "ChainKnockbackConfig", menuName = "Tap Knockout/Combat/Chain Knockback Config")]
    public sealed class ChainKnockbackConfig : ScriptableObject
    {
        [SerializeField] private bool enabled = true;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField, Min(0f)] private float baseDamage = 4f;
        [SerializeField, Min(0f)] private float damagePerKnockbackForce = 0.25f;
        [SerializeField, Range(0f, 1f)] private float secondaryKnockbackForceMultiplier = 0.35f;
        [SerializeField, Min(0f)] private float targetCooldownSeconds = 0.2f;
        [SerializeField, Min(0)] private int maxHitsPerKnockback = 1;

        public bool Enabled => enabled;
        public LayerMask TargetLayers => targetLayers;
        public float BaseDamage => baseDamage;
        public float DamagePerKnockbackForce => damagePerKnockbackForce;
        public float SecondaryKnockbackForceMultiplier => secondaryKnockbackForceMultiplier;
        public float TargetCooldownSeconds => targetCooldownSeconds;
        public int MaxHitsPerKnockback => maxHitsPerKnockback;

        private void OnValidate()
        {
            baseDamage = Mathf.Max(0f, baseDamage);
            damagePerKnockbackForce = Mathf.Max(0f, damagePerKnockbackForce);
            secondaryKnockbackForceMultiplier = Mathf.Clamp01(secondaryKnockbackForceMultiplier);
            targetCooldownSeconds = Mathf.Max(0f, targetCooldownSeconds);
            maxHitsPerKnockback = Mathf.Max(0, maxHitsPerKnockback);
        }
    }
}
