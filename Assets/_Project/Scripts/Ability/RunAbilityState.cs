using System;
using System.Collections.Generic;

namespace TapKnockout.Ability
{
    [Serializable]
    public sealed class RunAbilityState
    {
        private readonly List<AbilityDefinition> selectedAbilities = new List<AbilityDefinition>();
        private readonly Dictionary<string, int> stackCountsByAbilityId = new Dictionary<string, int>(StringComparer.Ordinal);

        public IReadOnlyList<AbilityDefinition> SelectedAbilities => selectedAbilities;
        public int TotalSelections => selectedAbilities.Count;

        public bool AddSelectedAbility(AbilityDefinition definition)
        {
            if (!CanSelect(definition))
            {
                return false;
            }

            selectedAbilities.Add(definition);
            stackCountsByAbilityId[definition.AbilityId] = GetStackCount(definition.AbilityId) + 1;
            return true;
        }

        public int GetStackCount(AbilityDefinition definition)
        {
            return definition == null ? 0 : GetStackCount(definition.AbilityId);
        }

        public int GetStackCount(string abilityId)
        {
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                return 0;
            }

            return stackCountsByAbilityId.TryGetValue(abilityId, out var count) ? count : 0;
        }

        public bool HasReachedMaxStacks(AbilityDefinition definition)
        {
            return definition != null && GetStackCount(definition) >= definition.MaxStacks;
        }

        public bool CanBeOffered(AbilityDefinition definition)
        {
            return IsSelectableDefinition(definition) && !HasReachedMaxStacks(definition);
        }

        public bool CanSelect(AbilityDefinition definition)
        {
            return CanBeOffered(definition);
        }

        public void Clear()
        {
            selectedAbilities.Clear();
            stackCountsByAbilityId.Clear();
        }

        private static bool IsSelectableDefinition(AbilityDefinition definition)
        {
            return definition != null && definition.IsEnabled && definition.HasValidId && definition.MaxStacks > 0;
        }
    }
}
