using NUnit.Framework;
using UnityEngine;

namespace TapKnockout.Ability.Tests
{
    public sealed class AbilitySelectionEffectTests
    {
        [Test]
        public void SelectOffer_CallsAssignedEffectApplier()
        {
            var gameObject = new GameObject("AbilitySelectionEffectTests");
            var controller = gameObject.AddComponent<AbilitySelectionController>();
            var applier = gameObject.AddComponent<RecordingAbilityEffectApplier>();
            var ability = AbilityTestFactory.CreateAbility("attack_damage_up");

            try
            {
                controller.SetAbilityPool(new[] { ability });
                controller.SetAbilityEffectApplier(applier);

                controller.GenerateOffer();
                Assert.That(controller.SelectOffer(0), Is.True);

                Assert.That(applier.ApplyCount, Is.EqualTo(1));
                Assert.That(applier.LastAbility, Is.SameAs(ability));
                Assert.That(applier.LastStackCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(ability);
                Object.DestroyImmediate(gameObject);
            }
        }

        private sealed class RecordingAbilityEffectApplier : MonoBehaviour, IAbilityEffectApplier
        {
            public int ApplyCount { get; private set; }
            public AbilityDefinition LastAbility { get; private set; }
            public int LastStackCount { get; private set; }

            public void ApplyAbility(AbilityEffectContext context)
            {
                ApplyCount++;
                LastAbility = context.Ability;
                LastStackCount = context.StackCount;
            }
        }
    }
}
