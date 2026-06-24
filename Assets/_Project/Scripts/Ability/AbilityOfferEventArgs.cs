using System;
using System.Collections.Generic;

namespace TapKnockout.Ability
{
    public readonly struct AbilityOfferEventArgs
    {
        public AbilityOfferEventArgs(AbilitySelectionController source, IReadOnlyList<AbilityDefinition> choices)
        {
            Source = source;
            Choices = CopyChoices(choices);
        }

        public AbilitySelectionController Source { get; }
        public IReadOnlyList<AbilityDefinition> Choices { get; }
        public int ChoiceCount => Choices.Count;
        public bool HasChoices => ChoiceCount > 0;

        private static AbilityDefinition[] CopyChoices(IReadOnlyList<AbilityDefinition> choices)
        {
            if (choices == null || choices.Count == 0)
            {
                return Array.Empty<AbilityDefinition>();
            }

            var copy = new AbilityDefinition[choices.Count];
            for (var i = 0; i < choices.Count; i++)
            {
                copy[i] = choices[i];
            }

            return copy;
        }
    }
}
