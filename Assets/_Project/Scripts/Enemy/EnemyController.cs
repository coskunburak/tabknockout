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

        [Header("Target Recovery")]
        [SerializeField] private bool recoverPlayerTargetWhenMissing = true;
        [SerializeField] private string fallbackPlayerTag = "Player";
        [SerializeField, Min(0.05f)] private float targetRecoveryInterval = 0.25f;

        private float nextTargetRecoveryTime;

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

        private void Update()
        {
            TryRecoverTargetIfNeeded();
        }

        private void OnValidate()
        {
            targetRecoveryInterval = Mathf.Max(0.05f, targetRecoveryInterval);
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
            ApplyTargetToRuntimeReceivers(runtimeTarget);
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
            InitializeRuntimeConfigReceivers();
        }

        private void InitializeRuntimeConfigReceivers()
        {
            var behaviours = GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] == null || behaviours[i] == this)
                {
                    continue;
                }

                if (behaviours[i] is IEnemyRuntimeConfigReceiver receiver)
                {
                    receiver.Initialize(config, target);
                }
            }
        }

        private void ApplyTargetToRuntimeReceivers(Transform runtimeTarget)
        {
            var behaviours = GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] == null || behaviours[i] == this)
                {
                    continue;
                }

                if (behaviours[i] is IEnemyRuntimeTargetReceiver receiver)
                {
                    receiver.SetTarget(runtimeTarget);
                }
            }
        }

        private void TryRecoverTargetIfNeeded()
        {
            if (!recoverPlayerTargetWhenMissing || HasUsableTarget(target))
            {
                return;
            }

            if (Time.time < nextTargetRecoveryTime)
            {
                return;
            }

            nextTargetRecoveryTime = Time.time + targetRecoveryInterval;
            if (TryFindFallbackPlayerTarget(out var recoveredTarget))
            {
                SetTarget(recoveredTarget);
            }
        }

        private bool TryFindFallbackPlayerTarget(out Transform recoveredTarget)
        {
            recoveredTarget = null;
            if (string.IsNullOrWhiteSpace(fallbackPlayerTag))
            {
                return false;
            }

            GameObject playerObject = null;
            try
            {
                playerObject = GameObject.FindGameObjectWithTag(fallbackPlayerTag);
            }
            catch (UnityException)
            {
                return false;
            }

            if (playerObject == null || !playerObject.activeInHierarchy)
            {
                return false;
            }

            recoveredTarget = playerObject.transform;
            return recoveredTarget != null;
        }

        private static bool HasUsableTarget(Transform candidate)
        {
            return candidate != null && candidate.gameObject.activeInHierarchy;
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
