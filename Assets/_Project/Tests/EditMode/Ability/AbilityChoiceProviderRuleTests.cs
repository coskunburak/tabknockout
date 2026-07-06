using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Ability.Tests
{
    public sealed class AbilityChoiceProviderRuleTests
    {
        [Test]
        public void GenerateChoices_ExcludesPlaceholderAbilitiesByDefault()
        {
            var placeholder = AbilityTestFactory.CreateAbility(
                "placeholder",
                implementationStatus: AbilityImplementationStatus.Placeholder,
                isPlaceholder: true);
            var implemented = AbilityTestFactory.CreateAbility("implemented");
            var provider = new AbilityChoiceProvider(17);

            try
            {
                var choices = provider.GenerateChoices(new[] { placeholder, implemented }, new RunAbilityState(), 3);

                Assert.That(choices.Count, Is.EqualTo(1));
                Assert.That(choices[0], Is.SameAs(implemented));
            }
            finally
            {
                Object.DestroyImmediate(placeholder);
                Object.DestroyImmediate(implemented);
            }
        }

        [Test]
        public void GenerateChoices_CanIncludePlaceholderAbilitiesWhenExplicitlyAllowed()
        {
            var placeholder = AbilityTestFactory.CreateAbility(
                "placeholder",
                implementationStatus: AbilityImplementationStatus.Placeholder,
                isPlaceholder: true);
            var provider = new AbilityChoiceProvider(19)
            {
                AllowPlaceholderAbilitiesInOffers = true
            };

            try
            {
                var choices = provider.GenerateChoices(new[] { placeholder }, new RunAbilityState(), 3);

                Assert.That(choices.Count, Is.EqualTo(1));
                Assert.That(choices[0], Is.SameAs(placeholder));
            }
            finally
            {
                Object.DestroyImmediate(placeholder);
            }
        }

        [Test]
        public void GenerateChoices_RespectsPrerequisiteAbilityIds()
        {
            var prerequisite = AbilityTestFactory.CreateAbility("ember_mark");
            var superAbility = AbilityTestFactory.CreateAbility("super_ember");
            SetStringArray(superAbility, "prerequisiteAbilityIds", "ember_mark");
            var provider = new AbilityChoiceProvider(23);
            var runState = new RunAbilityState();

            try
            {
                var lockedChoices = provider.GenerateChoices(new[] { superAbility }, runState, 3);
                Assert.That(lockedChoices.Count, Is.EqualTo(0));

                runState.AddSelectedAbility(prerequisite);
                var unlockedChoices = provider.GenerateChoices(new[] { superAbility }, runState, 3);

                Assert.That(unlockedChoices.Count, Is.EqualTo(1));
                Assert.That(unlockedChoices[0], Is.SameAs(superAbility));
            }
            finally
            {
                Object.DestroyImmediate(prerequisite);
                Object.DestroyImmediate(superAbility);
            }
        }

        [Test]
        public void GenerateChoices_RespectsMutuallyExclusiveGroups()
        {
            var first = AbilityTestFactory.CreateAbility("twin_shot");
            var second = AbilityTestFactory.CreateAbility("focused_pair");
            SetString(first, "mutuallyExclusiveGroupId", "forward_projectile_pattern");
            SetString(second, "mutuallyExclusiveGroupId", "forward_projectile_pattern");
            var provider = new AbilityChoiceProvider(29);

            try
            {
                var choices = provider.GenerateChoices(new[] { first, second }, new RunAbilityState(), 2);

                Assert.That(choices.Count, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        private static void SetString(AbilityDefinition ability, string propertyName, string value)
        {
            var serializedObject = new SerializedObject(ability);
            serializedObject.FindProperty(propertyName).stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetStringArray(AbilityDefinition ability, string propertyName, params string[] values)
        {
            var serializedObject = new SerializedObject(ability);
            var property = serializedObject.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).stringValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
