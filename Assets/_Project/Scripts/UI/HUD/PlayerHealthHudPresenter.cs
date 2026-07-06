using TapKnockout.Combat;
using TapKnockout.Pickups;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.UI.HUD
{
    /// <summary>
    /// Presenter binding gameplay health and XP data to the PlayerHealthHudView.
    /// Subscribes to PlayerHealth events — no per-frame polling.
    /// Can hot-swap the health source at runtime (e.g. after respawn).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerHealthHudPresenter : MonoBehaviour
    {
        [Header("View")]
        [SerializeField] private PlayerHealthHudView view;

        [Header("Gameplay Sources")]
        [Tooltip("Player health source. Can be assigned at runtime via SetPlayerHealth().")]
        [SerializeField] private PlayerHealth playerHealth;

        [Tooltip("Player XP controller for level display. Optional.")]
        [SerializeField] private PlayerXPController xpController;

        [Header("Fallback")]
        [Tooltip("Portrait sprite to show when no portrait data is available.")]
        [SerializeField] private Sprite fallbackPortrait;

        // ──────────────────────────────────────────────────────────
        // Lifecycle
        // ──────────────────────────────────────────────────────────

        private void OnEnable()
        {
            SubscribeHealth(playerHealth);
            SubscribeXP(xpController);
            Refresh();
        }

        private void OnDisable()
        {
            UnsubscribeHealth(playerHealth);
            UnsubscribeXP(xpController);
        }

        // ──────────────────────────────────────────────────────────
        // Public API — hot-swap sources at runtime
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Binds the presenter to a new PlayerHealth source.
        /// Unsubscribes from the previous source automatically.
        /// Safe to call multiple times.
        /// </summary>
        public void SetPlayerHealth(PlayerHealth health)
        {
            if (playerHealth == health) return;

            UnsubscribeHealth(playerHealth);
            playerHealth = health;
            SubscribeHealth(playerHealth);
            Refresh();
        }

        /// <summary>
        /// Binds the presenter to a new PlayerXPController for level display.
        /// </summary>
        public void SetXPController(PlayerXPController xp)
        {
            if (xpController == xp) return;

            UnsubscribeXP(xpController);
            xpController = xp;
            SubscribeXP(xpController);
            RefreshLevel();
        }

        /// <summary>Manually sets a portrait sprite on the view.</summary>
        public void SetPortrait(Sprite portrait)
        {
            view?.SetPortrait(portrait);
        }

        /// <summary>
        /// Performs a full refresh of all view elements from current gameplay state.
        /// Call this after binding sources or after a respawn.
        /// </summary>
        public void Refresh()
        {
            RefreshHealth(animate: false);
            RefreshLevel();
            view?.SetPortrait(fallbackPortrait);
        }

        // ──────────────────────────────────────────────────────────
        // Private Refresh Helpers
        // ──────────────────────────────────────────────────────────

        private void RefreshHealth(bool animate)
        {
            if (view == null) return;

            if (playerHealth == null)
            {
                // Show fallback state — view handles null gracefully
                view.SetHealth(0f, 0f, false, false);
                return;
            }

            view.SetHealth(
                playerHealth.CurrentHealth,
                playerHealth.MaxHealth,
                isDamage: false,
                animate: animate);
        }

        private void RefreshLevel()
        {
            if (view == null) return;

            var level = xpController != null ? xpController.Level : 1;
            view.SetLevel(level);
        }

        // ──────────────────────────────────────────────────────────
        // Subscriptions
        // ──────────────────────────────────────────────────────────

        private void SubscribeHealth(PlayerHealth health)
        {
            if (health == null) return;

            health.OnDamaged    -= HandleDamaged;
            health.OnDamaged    += HandleDamaged;

            health.OnHealed     -= HandleHealed;
            health.OnHealed     += HandleHealed;

            health.OnPlayerDied -= HandlePlayerDied;
            health.OnPlayerDied += HandlePlayerDied;
        }

        private void UnsubscribeHealth(PlayerHealth health)
        {
            if (health == null) return;

            health.OnDamaged    -= HandleDamaged;
            health.OnHealed     -= HandleHealed;
            health.OnPlayerDied -= HandlePlayerDied;
        }

        private void SubscribeXP(PlayerXPController xp)
        {
            if (xp == null) return;

            xp.OnXPChanged -= HandleXPChanged;
            xp.OnXPChanged += HandleXPChanged;
        }

        private void UnsubscribeXP(PlayerXPController xp)
        {
            if (xp == null) return;

            xp.OnXPChanged -= HandleXPChanged;
        }

        // ──────────────────────────────────────────────────────────
        // Event Handlers
        // ──────────────────────────────────────────────────────────

        private void HandleDamaged(HitContext hitContext)
        {
            if (view == null || playerHealth == null) return;

            view.SetHealth(
                playerHealth.CurrentHealth,
                playerHealth.MaxHealth,
                isDamage: true,
                animate: true);
        }

        private void HandleHealed(float healAmount)
        {
            if (view == null || playerHealth == null) return;

            view.SetHealth(
                playerHealth.CurrentHealth,
                playerHealth.MaxHealth,
                isDamage: false,
                animate: true);
        }

        private void HandlePlayerDied(HitContext hitContext)
        {
            if (view == null || playerHealth == null) return;

            view.SetHealth(0f, playerHealth.MaxHealth, isDamage: true, animate: true);
        }

        private void HandleXPChanged(PlayerXPChangedEventArgs eventArgs)
        {
            view?.SetLevel(eventArgs.Level);
        }
    }
}
