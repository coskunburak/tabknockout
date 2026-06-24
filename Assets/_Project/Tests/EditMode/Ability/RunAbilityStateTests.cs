using NUnit.Framework;
using UnityEngine;

namespace TapKnockout.Ability.Tests
{
    public sealed class RunAbilityStateTests
    {
        [Test]
        public void AbilityDefinition_DefaultValuesAreSane()
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();

            try
            {
                Assert.That(ability.AbilityId, Is.EqualTo("ability_new"));
                Assert.That(ability.DisplayName, Is.EqualTo("New Ability"));
                Assert.That(ability.Rarity, Is.EqualTo(AbilityRarity.Common));
                Assert.That(ability.EffectType, Is.EqualTo(AbilityEffectType.None));
                Assert.That(ability.MaxStacks, Is.EqualTo(1));
                Assert.That(ability.Weight, Is.EqualTo(100f));
                Assert.That(ability.IsEnabled, Is.True);
                Assert.That(ability.Duration, Is.EqualTo(0f));
            }
            finally
            {
                Object.DestroyImmediate(ability);
            }
        }

        [Test]
        public void AddSelectedAbility_IncrementsStackCount()
        {
            var ability = AbilityTestFactory.CreateAbility("attack_damage_up", maxStacks: 3);
            var state = new RunAbilityState();

            try
            {
                Assert.That(state.AddSelectedAbility(ability), Is.True);
                Assert.That(state.AddSelectedAbility(ability), Is.True);
                Assert.That(state.GetStackCount(ability), Is.EqualTo(2));
                Assert.That(state.TotalSelections, Is.EqualTo(2));
                Assert.That(state.SelectedAbilities.Count, Is.EqualTo(2));
            }
            finally
            {
                Object.DestroyImmediate(ability);
            }
        }

        [Test]
        public void AddSelectedAbility_RespectsMaxStacks()
        {
            var ability = AbilityTestFactory.CreateAbility("dash_cooldown_down", maxStacks: 1);
            var state = new RunAbilityState();

            try
            {
                Assert.That(state.AddSelectedAbility(ability), Is.True);
                Assert.That(state.AddSelectedAbility(ability), Is.False);
                Assert.That(state.CanBeOffered(ability), Is.False);
                Assert.That(state.CanSelect(ability), Is.False);
                Assert.That(state.GetStackCount(ability), Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(ability);
            }
        }

        [Test]
        public void DisabledAbility_CannotBeOfferedOrSelected()
        {
            var ability = AbilityTestFactory.CreateAbility("disabled_ability", isEnabled: false);
            var state = new RunAbilityState();

            try
            {
                Assert.That(state.CanBeOffered(ability), Is.False);
                Assert.That(state.CanSelect(ability), Is.False);
                Assert.That(state.AddSelectedAbility(ability), Is.False);
                Assert.That(state.GetStackCount(ability), Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(ability);
            }
        }

        [Test]
        public void Clear_RemovesSelectedAbilitiesAndStacks()
        {
            var ability = AbilityTestFactory.CreateAbility("max_health_up");
            var state = new RunAbilityState();

            try
            {
                state.AddSelectedAbility(ability);
                state.Clear();

                Assert.That(state.TotalSelections, Is.EqualTo(0));
                Assert.That(state.GetStackCount(ability), Is.EqualTo(0));
                Assert.That(state.CanBeOffered(ability), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(ability);
            }
        }
    }
}
