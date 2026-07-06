using NUnit.Framework;
using TapKnockout.Ability;
using TapKnockout.Combat;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Survivor.Tests
{
    public sealed class ActiveSkillControllerAreaDamageTests
    {
        [Test]
        public void EnergyBeamActiveAbility_DealsAreaDamageInForwardCone()
        {
            var previousTimeScale = Time.timeScale;
            var player = new GameObject("Player");
            var target = new GameObject("Target");
            var ability = CreateActiveAbility("skill_arc_blast", AbilityEffectType.EnergyBeam, 17f, 4f);

            try
            {
                Time.timeScale = 1f;
                player.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                var controller = player.AddComponent<ActiveSkillController>();
                Assert.That(controller.SetSlotAbility(0, ability), Is.True);

                var damageProbe = AddDamageProbe(target, new Vector3(0f, 0f, 2f));
                Physics.SyncTransforms();

                Assert.That(controller.TryCastSlot(0), Is.True);
                Assert.That(damageProbe.HitCount, Is.EqualTo(1));
                Assert.That(damageProbe.LastHit.IsAbilityHit, Is.True);
                Assert.That(damageProbe.LastHit.AbilityId, Is.EqualTo("skill_arc_blast"));
                Assert.That(damageProbe.LastHit.DamageAmount, Is.EqualTo(17f).Within(0.001f));
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                Object.DestroyImmediate(ability);
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void EnergyRingActiveAbility_DealsAreaDamageAroundPlayer()
        {
            var previousTimeScale = Time.timeScale;
            var player = new GameObject("Player");
            var target = new GameObject("Target");
            var ability = CreateActiveAbility("skill_ground_slam", AbilityEffectType.EnergyRing, 21f, 2.5f);

            try
            {
                Time.timeScale = 1f;
                player.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                var controller = player.AddComponent<ActiveSkillController>();
                Assert.That(controller.SetSlotAbility(1, ability), Is.True);

                var damageProbe = AddDamageProbe(target, new Vector3(1.5f, 0f, 0f));
                Physics.SyncTransforms();

                Assert.That(controller.TryCastSlot(1), Is.True);
                Assert.That(damageProbe.HitCount, Is.EqualTo(1));
                Assert.That(damageProbe.LastHit.IsAbilityHit, Is.True);
                Assert.That(damageProbe.LastHit.AbilityId, Is.EqualTo("skill_ground_slam"));
                Assert.That(damageProbe.LastHit.DamageAmount, Is.EqualTo(21f).Within(0.001f));
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                Object.DestroyImmediate(ability);
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(player);
            }
        }

        private static AbilityDefinition CreateActiveAbility(
            string abilityId,
            AbilityEffectType effectType,
            float damage,
            float secondaryValue)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            var serializedObject = new SerializedObject(ability);
            serializedObject.FindProperty("abilityId").stringValue = abilityId;
            serializedObject.FindProperty("displayName").stringValue = abilityId;
            serializedObject.FindProperty("description").stringValue = abilityId;
            serializedObject.FindProperty("effectType").intValue = (int)effectType;
            serializedObject.FindProperty("maxStacks").intValue = 1;
            serializedObject.FindProperty("weight").floatValue = 1f;
            serializedObject.FindProperty("isEnabled").boolValue = true;
            serializedObject.FindProperty("value").floatValue = damage;
            serializedObject.FindProperty("secondaryValue").floatValue = secondaryValue;
            serializedObject.FindProperty("cooldown").floatValue = 4f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return ability;
        }

        private static DamageProbe AddDamageProbe(GameObject target, Vector3 position)
        {
            target.transform.position = position;
            target.AddComponent<BoxCollider>();
            return target.AddComponent<DamageProbe>();
        }

        private sealed class DamageProbe : MonoBehaviour, IDamageable
        {
            public bool IsAlive => true;
            public GameObject GameObject => gameObject;
            public int HitCount { get; private set; }
            public HitContext LastHit { get; private set; }

            public void ReceiveHit(HitContext hitContext)
            {
                HitCount++;
                LastHit = hitContext;
            }
        }
    }
}
