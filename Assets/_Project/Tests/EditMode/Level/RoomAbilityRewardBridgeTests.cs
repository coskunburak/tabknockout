using NUnit.Framework;
using TapKnockout.Ability;
using TapKnockout.Level;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Level.Tests
{
    public sealed class RoomAbilityRewardBridgeTests
    {
        [Test]
        public void RequestAbilityOffer_WithMissingSelectionController_ReturnsFalse()
        {
            var gameObject = new GameObject("Bridge");

            try
            {
                var bridge = gameObject.AddComponent<RoomAbilityRewardBridge>();

                Assert.That(bridge.RequestAbilityOffer(), Is.False);
                Assert.That(bridge.IsWaitingForSelection, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void OnEnable_WhenChapterRewardFlowControllerExists_DisablesLegacyBridge()
        {
            var gameObject = new GameObject("Bridge");

            try
            {
                gameObject.AddComponent<ChapterRoomRewardFlowController>();
                var bridge = gameObject.AddComponent<RoomAbilityRewardBridge>();

                Assert.That(bridge.enabled, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void RequestAbilityOffer_WithChoices_PausesUntilSelection()
        {
            var previousTimeScale = Time.timeScale;
            var gameObject = new GameObject("Bridge");
            var selectionObject = new GameObject("Selection");
            var ability = CreateAbility("attack_damage_up");

            try
            {
                var selectionController = selectionObject.AddComponent<AbilitySelectionController>();
                selectionController.SetAbilityPool(new[] { ability });

                var bridge = gameObject.AddComponent<RoomAbilityRewardBridge>();
                bridge.SetReferences(null, selectionController, null);

                Assert.That(bridge.RequestAbilityOffer(), Is.True);
                Assert.That(bridge.IsWaitingForSelection, Is.True);
                Assert.That(Time.timeScale, Is.EqualTo(0f));

                Assert.That(selectionController.SelectOffer(0), Is.True);
                Assert.That(bridge.IsWaitingForSelection, Is.False);
                Assert.That(Time.timeScale, Is.EqualTo(previousTimeScale));
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                Object.DestroyImmediate(ability);
                Object.DestroyImmediate(selectionObject);
                Object.DestroyImmediate(gameObject);
            }
        }

        private static AbilityDefinition CreateAbility(string abilityId)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            var serializedObject = new SerializedObject(ability);
            serializedObject.FindProperty("abilityId").stringValue = abilityId;
            serializedObject.FindProperty("displayName").stringValue = abilityId;
            serializedObject.FindProperty("maxStacks").intValue = 5;
            serializedObject.FindProperty("weight").floatValue = 100f;
            serializedObject.FindProperty("isEnabled").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return ability;
        }
    }
}
