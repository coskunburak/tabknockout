using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemyController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private EnemyConfig config;

        [Header("Components")]
        [SerializeField] private EnemyHealth health;
        [SerializeField] private EnemyMovement movement;
        [SerializeField] private KnockbackReceiver knockbackReceiver;
        [SerializeField] private EnemyAttackController attackController;

        [Header("Runtime Target")]
        [SerializeField] private Transform target;

        public EnemyConfig Config => config;
        public EnemyHealth Health => health;
        public EnemyMovement Movement => movement;
        public KnockbackReceiver KnockbackReceiver => knockbackReceiver;
        public EnemyAttackController AttackController => attackController;
        public Transform Target => target;

        private void Reset()
        {
            ResolveComponents();
        }

        private void Awake()
        {
            ResolveComponents();
            ApplyConfigToComponents();
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnDied += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDied -= HandleDied;
            }
        }

        public void Initialize(EnemyConfig enemyConfig, Transform runtimeTarget)
        {
            config = enemyConfig;
            target = runtimeTarget;
            ResolveComponents();
            ApplyConfigToComponents();
        }

        public void SetTarget(Transform runtimeTarget)
        {
            target = runtimeTarget;
            movement?.SetTarget(runtimeTarget);
            attackController?.SetTarget(runtimeTarget);
        }

        private void ResolveComponents()
        {
            if (health == null)
            {
                TryGetComponent(out health);
            }

            if (movement == null)
            {
                TryGetComponent(out movement);
            }

            if (knockbackReceiver == null)
            {
                TryGetComponent(out knockbackReceiver);
            }

            if (attackController == null)
            {
                TryGetComponent(out attackController);
            }
        }

        private void ApplyConfigToComponents()
        {
            health?.Initialize(config);
            movement?.Initialize(config, target);
            knockbackReceiver?.Initialize(config);
            attackController?.Initialize(config, target);
        }

        private void HandleDied(HitContext hitContext)
        {
            if (movement != null)
            {
                movement.enabled = false;
            }

            if (attackController != null)
            {
                attackController.enabled = false;
            }
        }
    }
}
