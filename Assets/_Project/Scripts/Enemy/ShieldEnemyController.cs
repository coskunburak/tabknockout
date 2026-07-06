using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    public sealed class ShieldEnemyController : MonoBehaviour, IEnemyRuntimeConfigReceiver, IEnemyRuntimeTargetReceiver, TapKnockout.Combat.IPoolLifecycle
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private Transform target;
        [SerializeField] private ShieldDamageFilter shieldDamageFilter;
        [SerializeField] private bool faceTargetWhileShielding = true;
        [SerializeField, Min(0f)] private float turnSpeed = 540f;

        public EnemyConfig Config => config;
        public Transform Target => target;

        private void Awake()
        {
            if (shieldDamageFilter == null)
            {
                shieldDamageFilter = GetComponent<ShieldDamageFilter>();
            }
        }

        private void Update()
        {
            if (!faceTargetWhileShielding || target == null)
            {
                return;
            }

            var direction = target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        public void Initialize(EnemyConfig enemyConfig, Transform runtimeTarget)
        {
            config = enemyConfig;
            target = runtimeTarget;
            shieldDamageFilter?.Initialize(enemyConfig, runtimeTarget);
        }

        public void SetTarget(Transform runtimeTarget)
        {
            target = runtimeTarget;
            shieldDamageFilter?.Initialize(config, runtimeTarget);
        }

        public void ResetRuntimeState(bool clearTarget = true)
        {
            if (clearTarget)
            {
                target = null;
            }
        }

        public void OnBeforeSpawnFromPool()
        {
            ResetRuntimeState();
        }

        public void OnSpawnedFromPool()
        {
        }

        public void OnBeforeDespawnToPool()
        {
            ResetRuntimeState();
        }

        public void ResetForPool()
        {
            ResetRuntimeState();
        }
    }
}
