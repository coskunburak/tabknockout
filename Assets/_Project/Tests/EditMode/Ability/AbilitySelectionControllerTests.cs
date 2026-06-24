using NUnit.Framework;
using UnityEngine;

namespace TapKnockout.Ability.Tests
{
    public sealed class AbilitySelectionControllerTests
    {
        [Test]
        public void SelectOffer_StoresSelectedAbilityAndRaisesEvent()
        {
            var gameObject = new GameObject("AbilitySelectionControllerTests");
            var controller = gameObject.AddComponent<AbilitySelectionController>();
            var abilities = new[]
            {
                AbilityTestFactory.CreateAbility("attack_damage_up"),
                AbilityTestFactory.CreateAbility("attack_speed_up"),
                AbilityTestFactory.CreateAbility("dash_cooldown_down")
            };
            var raisedSelectedEvent = false;
            var selectedEventArgs = default(AbilitySelectedEventArgs);

            try
            {
                controller.SetAbilityPool(abilities);
                controller.SetRandomSeed(17);
                controller.OnAbilitySelected += eventArgs =>
                {
                    raisedSelectedEvent = true;
                    selectedEventArgs = eventArgs;
                };

                var offer = controller.GenerateOffer();
                var selectedAbility = offer[0];

                Assert.That(controller.SelectOffer(0), Is.True);
                Assert.That(controller.RunState.GetStackCount(selectedAbility), Is.EqualTo(1));
                Assert.That(controller.CurrentOffer.Count, Is.EqualTo(0));
                Assert.That(raisedSelectedEvent, Is.True);
                Assert.That(selectedEventArgs.SelectedAbility, Is.SameAs(selectedAbility));
                Assert.That(selectedEventArgs.StackCount, Is.EqualTo(1));
                Assert.That(selectedEventArgs.SelectedIndex, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
                DestroyAll(abilities);
            }
        }

        [Test]
        public void GenerateOffer_RaisesOfferEventWithSnapshot()
        {
            var gameObject = new GameObject("AbilitySelectionControllerTests");
            var controller = gameObject.AddComponent<AbilitySelectionController>();
            var abilities = new[]
            {
                AbilityTestFactory.CreateAbility("attack_damage_up"),
                AbilityTestFactory.CreateAbility("attack_speed_up"),
                AbilityTestFactory.CreateAbility("dash_cooldown_down")
            };
            var raisedOfferEvent = false;
            var offerEventArgs = default(AbilityOfferEventArgs);

            try
            {
                controller.SetAbilityPool(abilities);
                controller.SetRandomSeed(19);
                controller.OnAbilityOfferGenerated += eventArgs =>
                {
                    raisedOfferEvent = true;
                    offerEventArgs = eventArgs;
                };

                var offer = controller.GenerateOffer();
                controller.ClearCurrentOffer();

                Assert.That(offer.Count, Is.EqualTo(0));
                Assert.That(raisedOfferEvent, Is.True);
                Assert.That(offerEventArgs.ChoiceCount, Is.EqualTo(3));
                Assert.That(offerEventArgs.HasChoices, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
                DestroyAll(abilities);
            }
        }

        [Test]
        public void SelectOffer_RejectsInvalidIndexWithoutChangingRunState()
        {
            var gameObject = new GameObject("AbilitySelectionControllerTests");
            var controller = gameObject.AddComponent<AbilitySelectionController>();
            var ability = AbilityTestFactory.CreateAbility("attack_damage_up");

            try
            {
                controller.SetAbilityPool(new[] { ability });
                controller.GenerateOffer();

                Assert.That(controller.SelectOffer(2), Is.False);
                Assert.That(controller.RunState.TotalSelections, Is.EqualTo(0));
                Assert.That(controller.CurrentOffer.Count, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
                Object.DestroyImmediate(ability);
            }
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
