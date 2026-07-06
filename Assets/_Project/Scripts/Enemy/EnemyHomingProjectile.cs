using UnityEngine;

namespace TapKnockout.Enemy
{
    /// <summary>
    /// Optional component added to homing projectiles (e.g. Ghost curse orb).
    /// Rotates the projectile's Rigidbody velocity toward the target each frame
    /// with a maximum turn rate so the projectile remains dodgeable.
    ///
    /// Must be placed on the same GameObject as a Rigidbody.
    /// Call Initialize() immediately after spawning.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EnemyHomingProjectile : MonoBehaviour
    {
        [SerializeField] private Transform homingTarget;
        [SerializeField, Range(0f, 1f)] private float homingStrength = 0.25f;
        [SerializeField, Min(0f)] private float maxTurnDegreesPerSecond = 45f;

        private Rigidbody cachedRigidbody;
        private bool initialized;

        private void Awake()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
        }

        private void OnDisable()
        {
            initialized = false;
            homingTarget = null;
        }

        public void Initialize(Transform target, float strength, float maxTurnDegrees)
        {
            homingTarget = target;
            homingStrength = Mathf.Clamp01(strength);
            maxTurnDegreesPerSecond = Mathf.Max(0f, maxTurnDegrees);
            initialized = true;
        }

        private void FixedUpdate()
        {
            if (!initialized || cachedRigidbody == null || homingTarget == null || homingStrength <= 0f)
            {
                return;
            }

            var currentVelocity = cachedRigidbody.linearVelocity;
            var speed = currentVelocity.magnitude;
            if (speed < 0.01f)
            {
                return;
            }

            var toTarget = homingTarget.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return;
            }

            var desiredDir = toTarget.normalized;
            var currentDir = currentVelocity.normalized;

            // Clamp turn by maxTurnDegreesPerSecond
            var maxTurnThisFrame = maxTurnDegreesPerSecond * Time.fixedDeltaTime;
            var newDir = Vector3.RotateTowards(currentDir, desiredDir, maxTurnThisFrame * Mathf.Deg2Rad, 0f);

            cachedRigidbody.linearVelocity = newDir * speed;
            if (newDir.sqrMagnitude > 0.0001f)
            {
                cachedRigidbody.rotation = Quaternion.LookRotation(newDir, Vector3.up);
            }
        }
    }
}
