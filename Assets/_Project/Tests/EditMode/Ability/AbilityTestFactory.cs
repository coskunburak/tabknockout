using UnityEditor;
using UnityEngine;

namespace TapKnockout.Ability.Tests
{
    internal static class AbilityTestFactory
    {
        public static AbilityDefinition CreateAbility(
            string abilityId,
            int maxStacks = 3,
            float weight = 100f,
            bool isEnabled = true,
            bool allowDuplicateInOffer = false,
            AbilityRarity rarity = AbilityRarity.Common,
            AbilityCategory category = AbilityCategory.Attack,
            AbilityEffectType effectType = AbilityEffectType.AttackDamageUp,
            AbilityImplementationStatus implementationStatus = AbilityImplementationStatus.Implemented,
            bool isPlaceholder = false)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            ability.name = abilityId;

            var serializedObject = new SerializedObject(ability);
            serializedObject.FindProperty("abilityId").stringValue = abilityId;
            serializedObject.FindProperty("displayName").stringValue = abilityId;
            serializedObject.FindProperty("description").stringValue = $"Description for {abilityId}.";
            serializedObject.FindProperty("rarity").intValue = (int)rarity;
            serializedObject.FindProperty("category").intValue = (int)category;
            serializedObject.FindProperty("effectType").intValue = (int)effectType;
            serializedObject.FindProperty("maxStacks").intValue = maxStacks;
            serializedObject.FindProperty("weight").floatValue = weight;
            serializedObject.FindProperty("allowDuplicateInOffer").boolValue = allowDuplicateInOffer;
            serializedObject.FindProperty("isEnabled").boolValue = isEnabled;
            serializedObject.FindProperty("isPlaceholder").boolValue = isPlaceholder;
            serializedObject.FindProperty("implementationStatus").intValue = (int)implementationStatus;
            serializedObject.FindProperty("value").floatValue = 1f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            return ability;
        }
    }
}
