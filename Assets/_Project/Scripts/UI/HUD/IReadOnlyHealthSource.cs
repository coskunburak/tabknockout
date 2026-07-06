using System;

namespace TapKnockout.UI.HUD
{
    /// <summary>
    /// Read-only health abstraction used by the Player Health HUD Presenter.
    /// Implement this on any health component to make it compatible with the HUD
    /// without modifying the HUD itself.
    /// </summary>
    public interface IReadOnlyHealthSource
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }

        /// <summary>Raised when health changes (damage or heal). Args: (currentHealth, maxHealth).</summary>
        event Action<float, float> OnHealthChanged;
    }
}
