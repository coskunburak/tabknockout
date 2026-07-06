using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Projectile
{
    public readonly struct EnemyProjectileRequest
    {
        public EnemyProjectileRequest(
            GameObject source,
            GameObject target,
            Vector3 origin,
            Vector3 direction,
            float damage,
            float speed,
            float lifetime)
        {
            Source = source;
            Target = target;
            Origin = origin;
            Direction = FlattenAndNormalize(direction);
            Damage = Mathf.Max(0f, damage);
            Speed = Mathf.Max(0f, speed);
            Lifetime = Mathf.Max(0.01f, lifetime);
        }

        public GameObject Source { get; }
        public GameObject Target { get; }
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public float Damage { get; }
        public float Speed { get; }
        public float Lifetime { get; }
        public bool CanSpawn => Source != null && Direction.sqrMagnitude > 0f && Speed > 0f && Lifetime > 0f;

        public HitContext CreateHitContext(GameObject resolvedTarget = null)
        {
            return new HitContext(Source, resolvedTarget != null ? resolvedTarget : Target, Damage, DamageType.Physical)
            {
                IsProjectileHit = true,
                HitDirection = Direction,
                HitPoint = Origin
            };
        }

        private static Vector3 FlattenAndNormalize(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }
    }
}
