using System;

namespace TapKnockout.Ability
{
    public readonly struct AbilityCatalogEntry
    {
        public AbilityCatalogEntry(
            string assetName,
            string abilityId,
            string displayName,
            string description,
            AbilityRarity rarity,
            AbilityCategory category,
            AbilityEffectType effectType,
            int maxStacks,
            float weight,
            float value,
            float secondaryValue,
            float duration,
            float cooldown,
            float procChance,
            AbilityImplementationStatus implementationStatus,
            AbilityTag[] abilityTags,
            AbilityTag[] requiredTags = null,
            AbilityTag[] blockedTags = null,
            string[] prerequisiteAbilityIds = null,
            string upgradeGroupId = "",
            string mutuallyExclusiveGroupId = "",
            bool isPlaceholder = false)
        {
            AssetName = assetName;
            AbilityId = abilityId;
            DisplayName = displayName;
            Description = description;
            Rarity = rarity;
            Category = category;
            EffectType = effectType;
            MaxStacks = maxStacks < 1 ? 1 : maxStacks;
            Weight = weight < 0f ? 0f : weight;
            Value = value;
            SecondaryValue = secondaryValue;
            Duration = duration < 0f ? 0f : duration;
            Cooldown = cooldown < 0f ? 0f : cooldown;
            ProcChance = Clamp01(procChance);
            ImplementationStatus = implementationStatus;
            AbilityTags = abilityTags ?? Array.Empty<AbilityTag>();
            RequiredTags = requiredTags ?? Array.Empty<AbilityTag>();
            BlockedTags = blockedTags ?? Array.Empty<AbilityTag>();
            PrerequisiteAbilityIds = prerequisiteAbilityIds ?? Array.Empty<string>();
            UpgradeGroupId = upgradeGroupId ?? string.Empty;
            MutuallyExclusiveGroupId = mutuallyExclusiveGroupId ?? string.Empty;
            IsPlaceholder = isPlaceholder;
        }

        public string AssetName { get; }
        public string AbilityId { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public AbilityRarity Rarity { get; }
        public AbilityCategory Category { get; }
        public AbilityEffectType EffectType { get; }
        public int MaxStacks { get; }
        public float Weight { get; }
        public float Value { get; }
        public float SecondaryValue { get; }
        public float Duration { get; }
        public float Cooldown { get; }
        public float ProcChance { get; }
        public AbilityImplementationStatus ImplementationStatus { get; }
        public AbilityTag[] AbilityTags { get; }
        public AbilityTag[] RequiredTags { get; }
        public AbilityTag[] BlockedTags { get; }
        public string[] PrerequisiteAbilityIds { get; }
        public string UpgradeGroupId { get; }
        public string MutuallyExclusiveGroupId { get; }
        public bool IsPlaceholder { get; }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            return value > 1f ? 1f : value;
        }
    }
}
