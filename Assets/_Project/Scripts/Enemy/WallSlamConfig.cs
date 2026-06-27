using UnityEngine;

namespace TapKnockout.Enemy
{
    [CreateAssetMenu(fileName = "WallSlamConfig", menuName = "Tap Knockout/Combat/Wall Slam Config")]
    public sealed class WallSlamConfig : ScriptableObject
    {
        [SerializeField] private bool enabled = true;
        [SerializeField] private LayerMask wallLayers;
        [SerializeField, Min(0f)] private float minKnockbackForce = 2f;
        [SerializeField, Min(0f)] private float baseDamage = 6f;
        [SerializeField, Min(0f)] private float damagePerKnockbackForce = 0.5f;
        [SerializeField, Min(0f)] private float cooldownSeconds = 0.2f;
        [SerializeField] private bool stopKnockbackOnSlam = true;

        public bool Enabled => enabled;
        public LayerMask WallLayers => wallLayers;
        public float MinKnockbackForce => minKnockbackForce;
        public float BaseDamage => baseDamage;
        public float DamagePerKnockbackForce => damagePerKnockbackForce;
        public float CooldownSeconds => cooldownSeconds;
        public bool StopKnockbackOnSlam => stopKnockbackOnSlam;

        private void OnValidate()
        {
            minKnockbackForce = Mathf.Max(0f, minKnockbackForce);
            baseDamage = Mathf.Max(0f, baseDamage);
            damagePerKnockbackForce = Mathf.Max(0f, damagePerKnockbackForce);
            cooldownSeconds = Mathf.Max(0f, cooldownSeconds);
        }
    }
}
