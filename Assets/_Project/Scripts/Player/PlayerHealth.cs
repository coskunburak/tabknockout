using System;
using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Config")]
        [SerializeField] private PlayerConfig config;
        [SerializeField] private PlayerRuntimeStats runtimeStats;

        [Header("Dash I-Frame")]
        [SerializeField] private PlayerDashController dashController;
        [SerializeField] private bool ignoreDamageDuringDashIFrames = true;

        [Header("Fallback Values")]
        [SerializeField, Min(1f)] private float fallbackMaxHealth = 100f;
        [SerializeField, Min(0f)] private float fallbackContactDamageInvulnerabilityWindow = 0.2f;

        [Header("Debug")]
        [SerializeField] private bool logDamage;
        [SerializeField] private bool logIgnoredDamage;
        [SerializeField] private bool logDeath = true;

        private bool hasDied;
        private float nextDamageAllowedTime;

        public event Action<HitContext> OnDamaged;
        public event Action<HitContext> OnDamageIgnored;
        public event Action<HitContext> OnPlayerDied;

        /// <summary>Raised when Heal() succeeds. Arg: amount actually restored.</summary>
        public event Action<float> OnHealed;

        public bool IsAlive => !hasDied && CurrentHealth > 0f;
        public GameObject GameObject => gameObject;
        public float CurrentHealth { get; private set; }
        public float BaseMaxHealth => config != null ? config.MaxHealth : fallbackMaxHealth;
        public float MaxHealth => BaseMaxHealth + (runtimeStats != null ? runtimeStats.MaxHealthBonus : 0f);
        public bool IsDashInvulnerable => ignoreDamageDuringDashIFrames &&
            dashController != null &&
            dashController.IsDashInvulnerable;
        public bool IsDamageInvulnerabilityActive => Time.time < nextDamageAllowedTime;

        private float ContactDamageInvulnerabilityWindow => config != null
            ? config.ContactDamageInvulnerabilityWindow
            : fallbackContactDamageInvulnerabilityWindow;

        private void Reset()
        {
            dashController = GetComponent<PlayerDashController>();
            runtimeStats = GetComponent<PlayerRuntimeStats>();
        }

        private void Awake()
        {
            ResolveReferences();

            ResetHealth();
        }

        private void OnEnable()
        {
            SubscribeRuntimeStats();
        }

        private void OnDisable()
        {
            UnsubscribeRuntimeStats();
        }

        private void OnValidate()
        {
            fallbackMaxHealth = Mathf.Max(1f, fallbackMaxHealth);
            fallbackContactDamageInvulnerabilityWindow = Mathf.Max(0f, fallbackContactDamageInvulnerabilityWindow);
        }

        public void Initialize(PlayerConfig playerConfig, bool resetHealth = true)
        {
            config = playerConfig;

            if (resetHealth)
            {
                ResetHealth();
            }
        }

        public void SetDashController(PlayerDashController playerDashController)
        {
            dashController = playerDashController;
        }

        public void SetRuntimeStats(PlayerRuntimeStats stats)
        {
            if (runtimeStats == stats)
            {
                return;
            }

            UnsubscribeRuntimeStats();
            runtimeStats = stats;
            SubscribeRuntimeStats();
            RefreshFromRuntimeStats();
        }

        public void ResetHealth()
        {
            hasDied = false;
            CurrentHealth = MaxHealth;
            nextDamageAllowedTime = 0f;
        }

        public void RefreshFromRuntimeStats(float currentHealthIncrease = 0f)
        {
            var maxHealth = MaxHealth;
            CurrentHealth = Mathf.Clamp(CurrentHealth + Mathf.Max(0f, currentHealthIncrease), 0f, maxHealth);
            if (CurrentHealth > 0f)
            {
                hasDied = false;
            }
        }

        public void ReceiveHit(HitContext hitContext)
        {
            if (hitContext == null || !IsAlive)
            {
                return;
            }

            if (ShouldIgnoreDamage(hitContext))
            {
                hitContext.WasIgnored = true;

                if (logIgnoredDamage)
                {
                    Debug.Log($"{nameof(PlayerHealth)} on {name} ignored damage during invulnerability.", this);
                }

                OnDamageIgnored?.Invoke(hitContext);
                return;
            }

            var damageAmount = ResolveIncomingDamage(hitContext);
            hitContext.WasIgnored = false;
            hitContext.DamageAmount = damageAmount;
            CurrentHealth = Mathf.Max(0f, CurrentHealth - damageAmount);
            nextDamageAllowedTime = Time.time + ResolvePostHitInvulnerabilityWindow();

            if (logDamage)
            {
                Debug.Log(
                    $"{nameof(PlayerHealth)} on {name} received {damageAmount:0.##} {hitContext.DamageType} damage. HP: {CurrentHealth:0.##}/{MaxHealth:0.##}",
                    this);
            }

            OnDamaged?.Invoke(hitContext);

            if (CurrentHealth <= 0f)
            {
                Die(hitContext);
            }
        }

        public bool Heal(float amount)
        {
            if (amount <= 0f || !IsAlive)
            {
                return false;
            }

            var previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            var healed = CurrentHealth > previousHealth;
            if (healed)
            {
                OnHealed?.Invoke(CurrentHealth - previousHealth);
            }

            return healed;
        }

        private bool ShouldIgnoreDamage(HitContext hitContext)
        {
            if (IsDashInvulnerable || IsDamageInvulnerabilityActive)
            {
                return true;
            }

            if (hitContext == null || hitContext.DamageAmount <= 0f)
            {
                return false;
            }

            if (runtimeStats != null && runtimeStats.TryConsumeShieldCharge())
            {
                return true;
            }

            return runtimeStats != null &&
                runtimeStats.DodgeChance > 0f &&
                UnityEngine.Random.value <= runtimeStats.DodgeChance;
        }

        private float ResolveIncomingDamage(HitContext hitContext)
        {
            var damageAmount = Mathf.Max(0f, hitContext.DamageAmount);
            if (runtimeStats != null)
            {
                damageAmount *= runtimeStats.DamageReductionMultiplier;
            }

            return Mathf.Max(0f, damageAmount);
        }

        private float ResolvePostHitInvulnerabilityWindow()
        {
            var window = ContactDamageInvulnerabilityWindow;
            if (runtimeStats != null && runtimeStats.InvulnerabilityAfterHitDuration > 0f)
            {
                window = Mathf.Max(window, runtimeStats.InvulnerabilityAfterHitDuration);
            }

            return window;
        }

        private void ResolveReferences()
        {
            if (dashController == null)
            {
                dashController = GetComponent<PlayerDashController>();
            }

            if (runtimeStats == null)
            {
                runtimeStats = GetComponent<PlayerRuntimeStats>();
            }
        }

        private void SubscribeRuntimeStats()
        {
            if (runtimeStats != null)
            {
                runtimeStats.OnStatsChanged -= HandleRuntimeStatsChanged;
                runtimeStats.OnStatsChanged += HandleRuntimeStatsChanged;
            }
        }

        private void UnsubscribeRuntimeStats()
        {
            if (runtimeStats != null)
            {
                runtimeStats.OnStatsChanged -= HandleRuntimeStatsChanged;
            }
        }

        private void HandleRuntimeStatsChanged(PlayerRuntimeStats stats)
        {
            RefreshFromRuntimeStats();
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
                Debug.Log($"{nameof(PlayerHealth)} on {name} died.", this);
            }

            OnPlayerDied?.Invoke(killingHit);
            CombatEvents.RaiseEntityKilled(new EntityKilledEvent(gameObject, killingHit.Source, killingHit));
        }
    }
}
