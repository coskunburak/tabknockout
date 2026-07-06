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
        [SerializeField] private AbilityTag[] abilityTags = System.Array.Empty<AbilityTag>();

        [Header("Selection")]
        [SerializeField, Min(1)] private int maxStacks = 1;
        [SerializeField, Min(0f)] private float weight = 100f;
        [SerializeField] private bool allowDuplicateInOffer;
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private AbilityTag[] requiredTags = System.Array.Empty<AbilityTag>();
        [SerializeField] private AbilityTag[] blockedTags = System.Array.Empty<AbilityTag>();
        [SerializeField] private string[] prerequisiteAbilityIds = System.Array.Empty<string>();
        [SerializeField] private string upgradeGroupId = string.Empty;
        [SerializeField] private string mutuallyExclusiveGroupId = string.Empty;
        [SerializeField] private bool isPlaceholder;
        [SerializeField] private AbilityImplementationStatus implementationStatus = AbilityImplementationStatus.Implemented;

        [Header("Effect Values")]
        [SerializeField] private float value;
        [SerializeField] private float secondaryValue;
        [SerializeField, Min(0f)] private float duration;
        [SerializeField, Min(0f)] private float cooldown;
        [SerializeField, Range(0f, 1f)] private float procChance;

        [Header("Active Skill Feedback")]
        [SerializeField] private GameObject castVfxPrefab;
        [SerializeField] private GameObject impactVfxPrefab;
        [SerializeField] private GameObject telegraphVfxPrefab;
        [SerializeField, Min(0f)] private float vfxLifetime = 1.5f;
        [SerializeField] private AudioClip castSfx;
        [SerializeField] private AudioClip impactSfx;
        [SerializeField] private AudioClip loopSfx;
        [SerializeField, Min(0f)] private float loopSfxDuration;
        [SerializeField, Range(0f, 1f)] private float sfxVolumeScale = 1f;
        [SerializeField, Min(0f)] private float cameraShakeIntensity;
        [SerializeField, Min(0f)] private float cameraShakeDuration;

        public string AbilityId => abilityId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public AbilityRarity Rarity => rarity;
        public AbilityCategory Category => category;
        public AbilityEffectType EffectType => effectType;
        public IReadOnlyList<string> Tags => tags ?? System.Array.Empty<string>();
        public IReadOnlyList<AbilityTag> AbilityTags => abilityTags ?? System.Array.Empty<AbilityTag>();
        public int MaxStacks => Mathf.Max(1, maxStacks);
        public float Weight => Mathf.Max(0f, weight);
        public bool AllowDuplicateInOffer => allowDuplicateInOffer;
        public bool IsEnabled => isEnabled;
        public IReadOnlyList<AbilityTag> RequiredTags => requiredTags ?? System.Array.Empty<AbilityTag>();
        public IReadOnlyList<AbilityTag> BlockedTags => blockedTags ?? System.Array.Empty<AbilityTag>();
        public IReadOnlyList<string> PrerequisiteAbilityIds => prerequisiteAbilityIds ?? System.Array.Empty<string>();
        public string UpgradeGroupId => upgradeGroupId ?? string.Empty;
        public string MutuallyExclusiveGroupId => mutuallyExclusiveGroupId ?? string.Empty;
        public bool IsPlaceholder => isPlaceholder || implementationStatus == AbilityImplementationStatus.Placeholder;
        public AbilityImplementationStatus ImplementationStatus => implementationStatus;
        public float Value => value;
        public float SecondaryValue => secondaryValue;
        public float Duration => Mathf.Max(0f, duration);
        public float Cooldown => Mathf.Max(0f, cooldown);
        public float ProcChance => Mathf.Clamp01(procChance);
        public GameObject CastVFXPrefab => castVfxPrefab;
        public GameObject ImpactVFXPrefab => impactVfxPrefab;
        public GameObject TelegraphVFXPrefab => telegraphVfxPrefab;
        public float VFXLifetime => Mathf.Max(0f, vfxLifetime);
        public AudioClip CastSFX => castSfx;
        public AudioClip ImpactSFX => impactSfx;
        public AudioClip LoopSFX => loopSfx;
        public float LoopSFXDuration => Mathf.Max(0f, loopSfxDuration);
        public float SFXVolumeScale => Mathf.Clamp01(sfxVolumeScale);
        public float CameraShakeIntensity => Mathf.Max(0f, cameraShakeIntensity);
        public float CameraShakeDuration => Mathf.Max(0f, cameraShakeDuration);
        public bool HasValidId => !string.IsNullOrWhiteSpace(abilityId);
        public bool IsImplementedForNormalOffers =>
            implementationStatus == AbilityImplementationStatus.Implemented
            || implementationStatus == AbilityImplementationStatus.PartiallyImplemented;
        public bool IsDeferred => implementationStatus == AbilityImplementationStatus.Deferred;

        public bool HasTag(AbilityTag tag)
        {
            return ContainsTag(AbilityTags, tag);
        }

        public bool HasAnyTag(IReadOnlyList<AbilityTag> required)
        {
            if (required == null || required.Count == 0)
            {
                return true;
            }

            for (var i = 0; i < required.Count; i++)
            {
                if (HasTag(required[i]))
                {
                    return true;
                }
            }

            return false;
        }

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
            cooldown = Mathf.Max(0f, cooldown);
            procChance = Mathf.Clamp01(procChance);
            vfxLifetime = Mathf.Max(0f, vfxLifetime);
            loopSfxDuration = Mathf.Max(0f, loopSfxDuration);
            sfxVolumeScale = Mathf.Clamp01(sfxVolumeScale);
            cameraShakeIntensity = Mathf.Max(0f, cameraShakeIntensity);
            cameraShakeDuration = Mathf.Max(0f, cameraShakeDuration);
            tags ??= System.Array.Empty<string>();
            abilityTags ??= System.Array.Empty<AbilityTag>();
            requiredTags ??= System.Array.Empty<AbilityTag>();
            blockedTags ??= System.Array.Empty<AbilityTag>();
            prerequisiteAbilityIds ??= System.Array.Empty<string>();
            upgradeGroupId ??= string.Empty;
            mutuallyExclusiveGroupId ??= string.Empty;
        }

        private static bool ContainsTag(IReadOnlyList<AbilityTag> source, AbilityTag tag)
        {
            if (source == null)
            {
                return false;
            }

            for (var i = 0; i < source.Count; i++)
            {
                if (source[i] == tag)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
