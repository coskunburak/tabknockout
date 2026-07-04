using System;
using System.Collections;
using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemyHealth : MonoBehaviour, IDamageable, ITargetable, ICombatTargetTraits, IPoolLifecycle
    {
        [Header("Config")]
        [SerializeField] private EnemyConfig config;

        [Header("Targeting")]
        [SerializeField] private Transform targetTransform;
        [SerializeField] private bool targetableWhenAlive = true;

        [Header("Death")]
        [SerializeField] private bool deactivateOnDeath;
        [SerializeField] private bool disableCollidersOnDeath = true;

        [Header("Debug")]
        [SerializeField] private bool logHits;
        [SerializeField] private bool logDeath = true;

        private KnockbackReceiver knockbackReceiver;
        private ShieldDamageFilter shieldDamageFilter;
        private Coroutine deathCoroutine;
        private bool hasDied;
        private bool isPoolInactive;

        public event Action<HitContext> OnDamaged;
        public event Action<HitContext> OnDied;

        public bool IsAlive => !isPoolInactive && !hasDied && CurrentHealth > 0f;
        public bool IsTargetable => !isPoolInactive && targetableWhenAlive && IsAlive;
        public GameObject GameObject => gameObject;
        public Transform TargetTransform => targetTransform != null ? targetTransform : transform;
        public float CurrentHealth { get; private set; }
        public float MaxHealth => config != null ? config.MaxHealth : 40f;
        public EnemyConfig Config => config;
        public bool IsBossTarget => config != null && (config.Rank == EnemyRank.Boss || config.Rank == EnemyRank.MiniBoss);
        public bool IsEliteTarget => config != null && (config.Rank == EnemyRank.Elite || config.Rank == EnemyRank.MiniBoss || config.Rank == EnemyRank.Boss);

        private void Awake()
        {
            knockbackReceiver = GetComponent<KnockbackReceiver>();
            shieldDamageFilter = GetComponent<ShieldDamageFilter>();
            ResetHealth();
        }

        private void OnValidate()
        {
            if (targetTransform == null)
            {
                targetTransform = transform;
            }
        }

        public void Initialize(EnemyConfig enemyConfig, bool resetHealth = true)
        {
            config = enemyConfig;

            if (resetHealth)
            {
                ResetHealth();
            }
        }

        public void ResetHealth()
        {
            isPoolInactive = false;
            hasDied = false;
            CurrentHealth = MaxHealth;
            SetCollidersEnabled(true);

            if (deathCoroutine != null)
            {
                StopCoroutine(deathCoroutine);
                deathCoroutine = null;
            }
        }

        public void OnBeforeSpawnFromPool()
        {
            ResetHealth();
        }

        public void OnSpawnedFromPool()
        {
            ResetHealth();
        }

        public void OnBeforeDespawnToPool()
        {
            MarkInactiveForPool();
        }

        public void ResetForPool()
        {
            MarkInactiveForPool();
        }

        public void ReceiveHit(HitContext hitContext)
        {
            if (hitContext == null || !IsAlive)
            {
                return;
            }

            if (shieldDamageFilter == null)
            {
                TryGetComponent(out shieldDamageFilter);
            }

            shieldDamageFilter?.ApplyToHit(hitContext);

            var damageAmount = Mathf.Max(0f, hitContext.DamageAmount);
            CurrentHealth = Mathf.Max(0f, CurrentHealth - damageAmount);

            if (logHits)
            {
                Debug.Log(
                    $"{nameof(EnemyHealth)} on {name} received {damageAmount:0.##} {hitContext.DamageType} damage. DashHit: {hitContext.IsDashHit}. HP: {CurrentHealth:0.##}/{MaxHealth:0.##}",
                    this);
            }

            OnDamaged?.Invoke(hitContext);

            if (IsAlive)
            {
                ApplyKnockbackIfAllowed(hitContext);
                return;
            }

            Die(hitContext);
        }

        private void ApplyKnockbackIfAllowed(HitContext hitContext)
        {
            if (!hitContext.Knockback.HasKnockback || config != null && !config.CanBeKnockedBack)
            {
                return;
            }

            if (knockbackReceiver == null)
            {
                TryGetComponent(out knockbackReceiver);
            }

            knockbackReceiver?.ApplyKnockback(hitContext);
        }

        private void Die(HitContext killingHit)
        {
            if (hasDied)
            {
                return;
            }

            hasDied = true;
            CurrentHealth = 0f;

            if (logDeath)
            {
                Debug.Log($"{nameof(EnemyHealth)} on {name} died.", this);
            }

            OnDied?.Invoke(killingHit);
            CombatEvents.RaiseEntityKilled(new EntityKilledEvent(gameObject, killingHit.Source, killingHit));
            DisableRuntimeBlockingIfNeeded();

            if (deactivateOnDeath)
            {
                deathCoroutine = StartCoroutine(DeactivateAfterDelay());
            }
        }

        private void DisableRuntimeBlockingIfNeeded()
        {
            if (disableCollidersOnDeath)
            {
                SetCollidersEnabled(false);
            }
        }

        private void SetCollidersEnabled(bool enabled)
        {
            var colliders = GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = enabled;
            }
        }

        private IEnumerator DeactivateAfterDelay()
        {
            var delay = config != null ? config.DeathDelay : 0f;
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            gameObject.SetActive(false);
            deathCoroutine = null;
        }

        private void MarkInactiveForPool()
        {
            isPoolInactive = true;
            hasDied = true;
            CurrentHealth = 0f;
            SetCollidersEnabled(false);

            if (deathCoroutine != null)
            {
                StopCoroutine(deathCoroutine);
                deathCoroutine = null;
            }
        }
    }
}
