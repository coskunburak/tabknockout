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
            return AddSelectedAbility(definition, false);
        }

        public bool AddSelectedAbility(AbilityDefinition definition, bool allowPlaceholderAbilities)
        {
            if (!CanSelect(definition, allowPlaceholderAbilities))
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
            return CanBeOffered(definition, false);
        }

        public bool CanBeOffered(AbilityDefinition definition, bool allowPlaceholderAbilities)
        {
            return IsSelectableDefinition(definition, allowPlaceholderAbilities)
                && !HasReachedMaxStacks(definition)
                && HasPrerequisiteAbilities(definition)
                && HasRequiredTags(definition)
                && !HasBlockedTags(definition)
                && !HasMutuallyExclusiveGroup(definition);
        }

        public bool CanSelect(AbilityDefinition definition)
        {
            return CanBeOffered(definition);
        }

        public bool CanSelect(AbilityDefinition definition, bool allowPlaceholderAbilities)
        {
            return CanBeOffered(definition, allowPlaceholderAbilities);
        }

        public void Clear()
        {
            selectedAbilities.Clear();
            stackCountsByAbilityId.Clear();
        }

        public bool HasSelectedAbilityId(string abilityId)
        {
            return GetStackCount(abilityId) > 0;
        }

        public bool HasSelectedTag(AbilityTag tag)
        {
            for (var i = 0; i < selectedAbilities.Count; i++)
            {
                var ability = selectedAbilities[i];
                if (ability != null && ability.HasTag(tag))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasMutuallyExclusiveGroup(AbilityDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.MutuallyExclusiveGroupId))
            {
                return false;
            }

            for (var i = 0; i < selectedAbilities.Count; i++)
            {
                var selectedAbility = selectedAbilities[i];
                if (selectedAbility == null)
                {
                    continue;
                }

                if (string.Equals(
                    selectedAbility.MutuallyExclusiveGroupId,
                    definition.MutuallyExclusiveGroupId,
                    StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasPrerequisiteAbilities(AbilityDefinition definition)
        {
            var prerequisites = definition.PrerequisiteAbilityIds;
            for (var i = 0; i < prerequisites.Count; i++)
            {
                var prerequisiteId = prerequisites[i];
                if (!string.IsNullOrWhiteSpace(prerequisiteId) && !HasSelectedAbilityId(prerequisiteId))
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasRequiredTags(AbilityDefinition definition)
        {
            var required = definition.RequiredTags;
            for (var i = 0; i < required.Count; i++)
            {
                if (!HasSelectedTag(required[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasBlockedTags(AbilityDefinition definition)
        {
            var blocked = definition.BlockedTags;
            for (var i = 0; i < blocked.Count; i++)
            {
                if (HasSelectedTag(blocked[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSelectableDefinition(AbilityDefinition definition, bool allowPlaceholderAbilities)
        {
            if (definition == null || !definition.IsEnabled || !definition.HasValidId || definition.MaxStacks <= 0)
            {
                return false;
            }

            if (allowPlaceholderAbilities)
            {
                return true;
            }

            return !definition.IsPlaceholder && definition.IsImplementedForNormalOffers;
        }
    }
}
