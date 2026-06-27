using TapKnockout.Combat;
using TapKnockout.Player;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealthHudController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Text healthLabel;

        public string CurrentText { get; private set; } = string.Empty;

        private void OnEnable()
        {
            Subscribe(playerHealth);
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe(playerHealth);
        }

        public void SetPlayerHealth(PlayerHealth health)
        {
            if (playerHealth == health)
            {
                return;
            }

            Unsubscribe(playerHealth);
            playerHealth = health;
            Subscribe(playerHealth);
            Refresh();
        }

        public void Refresh()
        {
            if (playerHealth == null)
            {
                SetText("HP -- / --");
                return;
            }

            SetText($"HP {Mathf.CeilToInt(playerHealth.CurrentHealth)} / {Mathf.CeilToInt(playerHealth.MaxHealth)}");
        }

        private void Subscribe(PlayerHealth health)
        {
            if (health == null)
            {
                return;
            }

            health.OnDamaged -= HandleHealthChanged;
            health.OnDamaged += HandleHealthChanged;
            health.OnPlayerDied -= HandleHealthChanged;
            health.OnPlayerDied += HandleHealthChanged;
        }

        private void Unsubscribe(PlayerHealth health)
        {
            if (health == null)
            {
                return;
            }

            health.OnDamaged -= HandleHealthChanged;
            health.OnPlayerDied -= HandleHealthChanged;
        }

        private void HandleHealthChanged(HitContext hitContext)
        {
            Refresh();
        }

        private void SetText(string value)
        {
            CurrentText = value;
            if (healthLabel != null)
            {
                healthLabel.text = CurrentText;
            }
        }
    }
}
