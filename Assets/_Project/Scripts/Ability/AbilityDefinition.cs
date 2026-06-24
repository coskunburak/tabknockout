using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Ability
{
    [CreateAssetMenu(menuName = "Tap Knockout/Abilities/Ability Definition")]
    public sealed class AbilityDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string abilityId = "ability_new";
        [SerializeField] private string displayName = "New Ability";
        [SerializeField, TextArea] private string description = string.Empty;
        [SerializeField] private Sprite icon;

        [Header("Classification")]
        [SerializeField] private AbilityRarity rarity = AbilityRarity.Common;
        [SerializeField] private AbilityCategory category = AbilityCategory.Attack;
        [SerializeField] private AbilityEffectType effectType = AbilityEffectType.None;
        [SerializeField] private string[] tags = System.Array.Empty<string>();

        [Header("Selection")]
        [SerializeField, Min(1)] private int maxStacks = 1;
        [SerializeField, Min(0f)] private float weight = 100f;
        [SerializeField] private bool allowDuplicateInOffer;
        [SerializeField] private bool isEnabled = true;

        [Header("Effect Values")]
        [SerializeField] private float value;
        [SerializeField] private float secondaryValue;
        [SerializeField, Min(0f)] private float duration;

        public string AbilityId => abilityId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public AbilityRarity Rarity => rarity;
        public AbilityCategory Category => category;
        public AbilityEffectType EffectType => effectType;
        public IReadOnlyList<string> Tags => tags ?? System.Array.Empty<string>();
        public int MaxStacks => Mathf.Max(1, maxStacks);
        public float Weight => Mathf.Max(0f, weight);
        public bool AllowDuplicateInOffer => allowDuplicateInOffer;
        public bool IsEnabled => isEnabled;
        public float Value => value;
        public float SecondaryValue => secondaryValue;
        public float Duration => Mathf.Max(0f, duration);
        public bool HasValidId => !string.IsNullOrWhiteSpace(abilityId);

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                abilityId = "ability_new";
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = "New Ability";
            }

            maxStacks = Mathf.Max(1, maxStacks);
            weight = Mathf.Max(0f, weight);
            duration = Mathf.Max(0f, duration);
            tags ??= System.Array.Empty<string>();
        }
    }
}
