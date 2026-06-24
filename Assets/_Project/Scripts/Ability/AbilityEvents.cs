using System;

namespace TapKnockout.Ability
{
    public static class AbilityEvents
    {
        public static event Action<AbilityOfferEventArgs> OnAbilityOfferGenerated;
        public static event Action<AbilitySelectedEventArgs> OnAbilitySelected;
        public static event Action<AbilityOfferEventArgs> OnAbilityOfferCleared;

        public static void RaiseAbilityOfferGenerated(AbilityOfferEventArgs eventArgs)
        {
            OnAbilityOfferGenerated?.Invoke(eventArgs);
        }

        public static void RaiseAbilitySelected(AbilitySelectedEventArgs eventArgs)
        {
            OnAbilitySelected?.Invoke(eventArgs);
        }

        public static void RaiseAbilityOfferCleared(AbilityOfferEventArgs eventArgs)
        {
            OnAbilityOfferCleared?.Invoke(eventArgs);
        }
    }
}
