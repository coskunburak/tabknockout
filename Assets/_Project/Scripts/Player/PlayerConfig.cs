using UnityEngine;

namespace TapKnockout.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Tap Knockout/Player/Player Config")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [Header("Health")]
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField, Min(0f)] private float contactDamageInvulnerabilityWindow = 0.2f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 5f;
        [SerializeField, Min(0f)] private float acceleration = 45f;
        [SerializeField, Min(0f)] private float rotationSpeed = 720f;
        [SerializeField, Range(0f, 0.95f)] private float movementDeadZone = 0.12f;
        [SerializeField, Min(0f)] private float stopToAttackMovementThreshold = 0.08f;

        [Header("Dash")]
        [SerializeField, Min(0.1f)] private float dashDistance = 3.5f;
        [SerializeField, Min(0.01f)] private float dashDuration = 0.18f;
        [SerializeField, Min(0f)] private float dashCooldown = 4f;
        [SerializeField, Min(0f)] private float dashImpactDamage = 12f;
        [SerializeField, Min(0f)] private float dashKnockbackForce = 8f;
        [SerializeField, Min(0f)] private float dashKnockbackDuration = 0.2f;
        [SerializeField, Min(0.05f)] private float dashHitRadius = 0.9f;
        [SerializeField] private LayerMask dashHitLayers;
        [SerializeField] private bool dashHasIFrames = true;
        [SerializeField, Min(0f)] private float dashIFrameDuration = 0.12f;

        public float MaxHealth => maxHealth;
        public float ContactDamageInvulnerabilityWindow => contactDamageInvulnerabilityWindow;
        public float MoveSpeed => moveSpeed;
        public float Acceleration => acceleration;
        public float RotationSpeed => rotationSpeed;
        public float MovementDeadZone => movementDeadZone;
        public float StopToAttackMovementThreshold => stopToAttackMovementThreshold;
        public float DashDistance => dashDistance;
        public float DashDuration => dashDuration;
        public float DashCooldown => dashCooldown;
        public float DashImpactDamage => dashImpactDamage;
        public float DashKnockbackForce => dashKnockbackForce;
        public float DashKnockbackDuration => dashKnockbackDuration;
        public float DashHitRadius => dashHitRadius;
        public LayerMask DashHitLayers => dashHitLayers;
        public bool DashHasIFrames => dashHasIFrames;
        public float DashIFrameDuration => dashIFrameDuration;

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
            contactDamageInvulnerabilityWindow = Mathf.Max(0f, contactDamageInvulnerabilityWindow);
            moveSpeed = Mathf.Max(0f, moveSpeed);
            acceleration = Mathf.Max(0f, acceleration);
            rotationSpeed = Mathf.Max(0f, rotationSpeed);
            movementDeadZone = Mathf.Clamp(movementDeadZone, 0f, 0.95f);
            stopToAttackMovementThreshold = Mathf.Max(0f, stopToAttackMovementThreshold);
            dashDistance = Mathf.Max(0.1f, dashDistance);
            dashDuration = Mathf.Max(0.01f, dashDuration);
            dashCooldown = Mathf.Max(dashDuration + 0.01f, dashCooldown);
            dashImpactDamage = Mathf.Max(0f, dashImpactDamage);
            dashKnockbackForce = Mathf.Max(0f, dashKnockbackForce);
            dashKnockbackDuration = Mathf.Max(0f, dashKnockbackDuration);
            dashHitRadius = Mathf.Max(0.05f, dashHitRadius);
            dashIFrameDuration = Mathf.Clamp(dashIFrameDuration, 0f, dashDuration);
        }
    }
}
