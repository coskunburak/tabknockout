using NUnit.Framework;
using TapKnockout.Ability;
using TapKnockout.UI;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.UI.Tests
{
    public sealed class AbilitySelectionPanelControllerTests
    {
        [Test]
        public void PanelWithoutConfiguredCanvasGroup_StaysSubscribedAcrossMultipleOffers()
        {
            var controllerObject = new GameObject("AbilitySelectionController");
            var panelObject = new GameObject("AbilitySelectionPanel");
            var cardObject = new GameObject("Card");
            var ability = CreateAbility();

            try
            {
                cardObject.transform.SetParent(panelObject.transform, false);
                var cardView = cardObject.AddComponent<AbilityChoiceCardView>();
                var selectionController = controllerObject.AddComponent<AbilitySelectionController>();
                selectionController.SetAbilityPool(new[] { ability });

                var panel = panelObject.AddComponent<AbilitySelectionPanelController>();
                AssignCardView(panel, cardView);
                panel.SetAbilitySelectionController(selectionController);

                selectionController.GenerateOffer();
                Assert.That(panel.IsOpen, Is.True);
                Assert.That(panelObject.activeSelf, Is.True);
                Assert.That(panelObject.GetComponent<CanvasGroup>(), Is.Not.Null);

                Assert.That(selectionController.SelectOffer(0), Is.True);
                Assert.That(panel.IsOpen, Is.False);
                Assert.That(panelObject.activeSelf, Is.True);

                selectionController.GenerateOffer();
                Assert.That(panel.IsOpen, Is.True);
                Assert.That(panelObject.activeSelf, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(ability);
                Object.DestroyImmediate(panelObject);
                Object.DestroyImmediate(controllerObject);
            }
        }

        private static AbilityDefinition CreateAbility()
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            var serializedObject = new SerializedObject(ability);
            serializedObject.FindProperty("abilityId").stringValue = "attack_damage_up";
            serializedObject.FindProperty("displayName").stringValue = "Attack Damage Up";
            serializedObject.FindProperty("effectType").enumValueIndex = (int)AbilityEffectType.AttackDamageUp;
            serializedObject.FindProperty("maxStacks").intValue = 5;
            serializedObject.FindProperty("weight").floatValue = 100f;
            serializedObject.FindProperty("isEnabled").boolValue = true;
            serializedObject.FindProperty("value").floatValue = 0.15f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return ability;
        }

        private static void AssignCardView(AbilitySelectionPanelController panel, AbilityChoiceCardView cardView)
        {
            var serializedObject = new SerializedObject(panel);
            var cardViews = serializedObject.FindProperty("cardViews");
            cardViews.arraySize = 1;
            cardViews.GetArrayElementAtIndex(0).objectReferenceValue = cardView;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
