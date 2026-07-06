using NUnit.Framework;
using TapKnockout.Ability;
using TapKnockout.Pickups;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Survivor.Tests
{
    public sealed class ArenaRunDirectorLevelUpTests
    {
        [Test]
        public void LevelUp_WithNoOfferPresentationListener_AutoSelectsAndResumes()
        {
            var previousTimeScale = Time.timeScale;
            var player = new GameObject("Player");
            var directorObject = new GameObject("ArenaRunDirector");
            var ability = CreateAbility("fallback_power");

            try
            {
                Time.timeScale = 1f;
                var xp = player.AddComponent<PlayerXPController>();
                xp.SetXPCurve(new[] { 1 });

                var selection = directorObject.AddComponent<AbilitySelectionController>();
                selection.SetAbilityPool(new[] { ability });

                var director = directorObject.AddComponent<ArenaRunDirector>();
                var serializedDirector = new SerializedObject(director);
                serializedDirector.FindProperty("xpController").objectReferenceValue = xp;
                serializedDirector.FindProperty("abilitySelectionController").objectReferenceValue = selection;
                serializedDirector.FindProperty("autoStartOnStart").boolValue = false;
                serializedDirector.FindProperty("pauseTimeScaleOnLevelUp").boolValue = true;
                serializedDirector.ApplyModifiedPropertiesWithoutUndo();

                director.StartRun();
                xp.AddXP(1);

                Assert.That(selection.RunState.GetStackCount(ability), Is.EqualTo(1));
                Assert.That(director.State, Is.EqualTo(SurvivorRunState.Running));
                Assert.That(Time.timeScale, Is.EqualTo(1f));
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                Object.DestroyImmediate(directorObject);
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(ability);
            }
        }

        private static AbilityDefinition CreateAbility(string abilityId)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            var serializedObject = new SerializedObject(ability);
            serializedObject.FindProperty("abilityId").stringValue = abilityId;
            serializedObject.FindProperty("displayName").stringValue = abilityId;
            serializedObject.FindProperty("description").stringValue = abilityId;
            serializedObject.FindProperty("effectType").intValue = (int)AbilityEffectType.AttackDamageUp;
            serializedObject.FindProperty("maxStacks").intValue = 5;
            serializedObject.FindProperty("weight").floatValue = 100f;
            serializedObject.FindProperty("isEnabled").boolValue = true;
            serializedObject.FindProperty("value").floatValue = 0.15f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return ability;
        }
    }
}
