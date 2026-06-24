using NUnit.Framework;
using UnityEngine;

namespace TapKnockout.Ability.Tests
{
    public sealed class AbilityChoiceProviderTests
    {
        [Test]
        public void GenerateChoices_ReturnsUpToRequestedChoiceCount()
        {
            var abilities = new[]
            {
                AbilityTestFactory.CreateAbility("attack_damage_up"),
                AbilityTestFactory.CreateAbility("attack_speed_up"),
                AbilityTestFactory.CreateAbility("dash_cooldown_down"),
                AbilityTestFactory.CreateAbility("dash_damage_up"),
                AbilityTestFactory.CreateAbility("max_health_up")
            };
            var provider = new AbilityChoiceProvider(7);

            try
            {
                var choices = provider.GenerateChoices(abilities, new RunAbilityState(), 3);

                Assert.That(choices.Count, Is.EqualTo(3));
                Assert.That(HasDuplicateAbilityIds(choices), Is.False);
            }
            finally
            {
                DestroyAll(abilities);
            }
        }

        [Test]
        public void GenerateChoices_AvoidsDuplicateAbilityIdsByDefault()
        {
            var ability = AbilityTestFactory.CreateAbility("attack_damage_up", allowDuplicateInOffer: false);
            var pool = new[] { ability, ability, ability };
            var provider = new AbilityChoiceProvider(3);

            try
            {
                var choices = provider.GenerateChoices(pool, new RunAbilityState(), 3);

                Assert.That(choices.Count, Is.EqualTo(1));
                Assert.That(choices[0], Is.SameAs(ability));
            }
            finally
            {
                Object.DestroyImmediate(ability);
            }
        }

        [Test]
        public void GenerateChoices_AllowsDuplicateOffersWhenDefinitionAllowsIt()
        {
            var ability = AbilityTestFactory.CreateAbility("stackable_offer", maxStacks: 5, allowDuplicateInOffer: true);
            var pool = new[] { ability };
            var provider = new AbilityChoiceProvider(5);

            try
            {
                var choices = provider.GenerateChoices(pool, new RunAbilityState(), 3);

                Assert.That(choices.Count, Is.EqualTo(3));
                Assert.That(choices[0], Is.SameAs(ability));
                Assert.That(choices[1], Is.SameAs(ability));
                Assert.That(choices[2], Is.SameAs(ability));
            }
            finally
            {
                Object.DestroyImmediate(ability);
            }
        }

        [Test]
        public void GenerateChoices_RespectsMaxStacksInRunState()
        {
            var maxedAbility = AbilityTestFactory.CreateAbility("dash_cooldown_down", maxStacks: 1);
            var availableAbility = AbilityTestFactory.CreateAbility("dash_damage_up", maxStacks: 1);
            var runState = new RunAbilityState();
            var provider = new AbilityChoiceProvider(11);

            try
            {
                runState.AddSelectedAbility(maxedAbility);
                var choices = provider.GenerateChoices(new[] { maxedAbility, availableAbility }, runState, 3);

                Assert.That(choices.Count, Is.EqualTo(1));
                Assert.That(choices[0], Is.SameAs(availableAbility));
            }
            finally
            {
                Object.DestroyImmediate(maxedAbility);
                Object.DestroyImmediate(availableAbility);
            }
        }

        [Test]
        public void GenerateChoices_IgnoresDisabledAndZeroWeightAbilities()
        {
            var disabledAbility = AbilityTestFactory.CreateAbility("disabled", isEnabled: false);
            var zeroWeightAbility = AbilityTestFactory.CreateAbility("zero_weight", weight: 0f);
            var validAbility = AbilityTestFactory.CreateAbility("valid");
            var provider = new AbilityChoiceProvider(13);

            try
            {
                var choices = provider.GenerateChoices(
                    new[] { disabledAbility, zeroWeightAbility, validAbility },
                    new RunAbilityState(),
                    3);

                Assert.That(choices.Count, Is.EqualTo(1));
                Assert.That(choices[0], Is.SameAs(validAbility));
            }
            finally
            {
                Object.DestroyImmediate(disabledAbility);
                Object.DestroyImmediate(zeroWeightAbility);
                Object.DestroyImmediate(validAbility);
            }
        }

        private static bool HasDuplicateAbilityIds(System.Collections.Generic.IReadOnlyList<AbilityDefinition> choices)
        {
            var ids = new System.Collections.Generic.HashSet<string>();
            for (var i = 0; i < choices.Count; i++)
            {
                if (!ids.Add(choices[i].AbilityId))
                {
                    return true;
                }
            }

            return false;
        }

        private static void DestroyAll(AbilityDefinition[] abilities)
        {
            for (var i = 0; i < abilities.Length; i++)
            {
                Object.DestroyImmediate(abilities[i]);
            }
        }
    }
}
