using TapKnockout.Combat;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Boss
{
    [DisallowMultipleComponent]
    public sealed class BossSlamAttack : MonoBehaviour
    {
        [SerializeField] private EnemyTelegraphController telegraphController;
        [SerializeField] private LayerMask damageLayers = ~0;
        [SerializeField, Range(4, 32)] private int overlapBufferSize = 12;
        [SerializeField, Min(0f)] private float fallbackRadius = 2f;
        [SerializeField, Min(0f)] private float fallbackDamage = 18f;

        private Collider[] overlapBuffer;

        public bool HasTelegraphedCurrentAttack { get; private set; }
        public bool LastDamageResolvedAfterTelegraph { get; private set; }

        private void Awake()
        {
            if (telegraphController == null)
            {
                telegraphController = GetComponentInChildren<EnemyTelegraphController>(true);
            }

            EnsureBuffer();
        }

        private void OnValidate()
        {
            overlapBufferSize = Mathf.Clamp(overlapBufferSize, 4, 32);
        }

        public void BeginTelegraph(BossAttackStep step)
        {
            HasTelegraphedCurrentAttack = true;
            LastDamageResolvedAfterTelegraph = false;
            var telegraphType = step.TelegraphType != EnemyTelegraphType.None ? step.TelegraphType : EnemyTelegraphType.BossSlamArea;
            telegraphController?.BeginTelegraph(null, telegraphType, step.WindupDuration, transform, null);
        }

        public int Execute(BossAttackStep step)
        {
            var radius = step.Radius > 0f ? step.Radius : fallbackRadius;
            var damage = (step.Damage > 0f ? step.Damage : fallbackDamage) * step.DamageMultiplier;
            var hitCount = ResolveDamage(transform.position, radius, damage);
            LastDamageResolvedAfterTelegraph = HasTelegraphedCurrentAttack;
            HasTelegraphedCurrentAttack = false;
            return hitCount;
        }

        public void EndTelegraph()
        {
            telegraphController?.EndTelegraph();
        }

        private int ResolveDamage(Vector3 center, float radius, float damage)
        {
            EnsureBuffer();
            var count = Physics.OverlapSphereNonAlloc(center, Mathf.Max(0f, radius), overlapBuffer, damageLayers, QueryTriggerInteraction.Collide);
            var hitCount = 0;
            for (var i = 0; i < count; i++)
            {
                var damageable = overlapBuffer[i] != null ? overlapBuffer[i].GetComponentInParent<IDamageable>() : null;
                if (damageable == null || !damageable.IsAlive || damageable.GameObject == gameObject)
                {
                    continue;
                }

                var targetObject = damageable.GameObject != null ? damageable.GameObject : overlapBuffer[i].gameObject;
                var direction = targetObject.transform.position - center;
                direction.y = 0f;
                var hitContext = new HitContext(gameObject, targetObject, damage, DamageType.Impact)
                {
                    HitPoint = center,
                    HitDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward
                };
                damageable.ReceiveHit(hitContext);
                RaiseDamageEvents(hitContext);
                hitCount++;
            }

            return hitCount;
        }

        private void EnsureBuffer()
        {
            if (overlapBuffer == null || overlapBuffer.Length != overlapBufferSize)
            {
                overlapBuffer = new Collider[overlapBufferSize];
            }
        }

        private static void RaiseDamageEvents(HitContext hitContext)
        {
            CombatEvents.RaiseHitResolved(hitContext);
            if (hitContext.WasIgnored)
            {
                return;
            }

            var damageEvent = new DamageEvent(hitContext.Source, hitContext.Target, hitContext.DamageAmount, hitContext.DamageType, hitContext);
            CombatEvents.RaiseDamageDealt(damageEvent);
            CombatEvents.RaiseDamageReceived(damageEvent);
        }
    }
}
