using UnityEngine;

namespace TapKnockout.Combat
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Tap Knockout/Debug/Dummy Damageable")]
    public sealed class DummyDamageable : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField, Min(1f)] private float maxHealth = 30f;
        [SerializeField] private bool resetHealthOnEnable = true;

        [Header("Debug")]
        [SerializeField] private bool logHits = true;
        [SerializeField] private bool disableOnDeath;

        private float currentHealth;

        public bool IsAlive => currentHealth > 0f;
        public GameObject GameObject => gameObject;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            ResetHealth();
        }

        private void OnEnable()
        {
            if (resetHealthOnEnable)
            {
                ResetHealth();
            }
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        public void ReceiveHit(HitContext hitContext)
        {
            if (hitContext == null || !IsAlive)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - Mathf.Max(0f, hitContext.DamageAmount));

            if (logHits)
            {
                Debug.Log(
                    $"{nameof(DummyDamageable)} on {name} received {hitContext.DamageAmount:0.##} {hitContext.DamageType} damage. DashHit: {hitContext.IsDashHit}. Knockback: {hitContext.Knockback.HasKnockback}. HP: {currentHealth:0.##}/{maxHealth:0.##}",
                    this);
            }

            if (!IsAlive)
            {
                CombatEvents.RaiseEntityKilled(new EntityKilledEvent(gameObject, hitContext.Source, hitContext));

                if (disableOnDeath)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        public void ResetHealth()
        {
            currentHealth = maxHealth;
        }
    }
}
