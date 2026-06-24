using NUnit.Framework;
using TapKnockout.Ability;
using TapKnockout.Player;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Player.Tests
{
    public sealed class PlayerAbilityEffectApplierTests
    {
        [Test]
        public void ApplyAbility_AppliesSupportedRuntimeStats()
        {
            var player = new GameObject("Player");
            var abilities = new[]
            {
                CreateAbility("attack_damage_up", AbilityEffectType.AttackDamageUp, 0.15f),
                CreateAbility("attack_speed_up", AbilityEffectType.AttackSpeedUp, 0.10f),
                CreateAbility("dash_cooldown_down", AbilityEffectType.DashCooldownDown, 0.15f),
                CreateAbility("dash_damage_up", AbilityEffectType.DashDamageUp, 0.20f),
                CreateAbility("extra_projectile", AbilityEffectType.ExtraProjectile, 1f)
            };

            try
            {
                var stats = player.AddComponent<PlayerRuntimeStats>();
                var applier = player.AddComponent<PlayerAbilityEffectApplier>();
                applier.SetRuntimeStats(stats);

                for (var i = 0; i < abilities.Length; i++)
                {
                    applier.ApplyAbility(new AbilityEffectContext(null, abilities[i], null, 1));
                }

                Assert.That(stats.AttackDamageMultiplier, Is.EqualTo(1.15f).Within(0.0001f));
                Assert.That(stats.AttackCooldownMultiplier, Is.EqualTo(0.90f).Within(0.0001f));
                Assert.That(stats.DashCooldownMultiplier, Is.EqualTo(0.85f).Within(0.0001f));
                Assert.That(stats.DashDamageMultiplier, Is.EqualTo(1.20f).Within(0.0001f));
                Assert.That(stats.ExtraProjectileCount, Is.EqualTo(1));
            }
            finally
            {
                DestroyAbilities(abilities);
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void ApplyAbility_MaxHealthUp_IncreasesMaxAndCurrentHealth()
        {
            var player = new GameObject("Player");
            var ability = CreateAbility("max_health_up", AbilityEffectType.MaxHealthUp, 20f);

            try
            {
                var stats = player.AddComponent<PlayerRuntimeStats>();
                var health = player.AddComponent<PlayerHealth>();
                health.SetRuntimeStats(stats);
                health.ResetHealth();

                var applier = player.AddComponent<PlayerAbilityEffectApplier>();
                applier.SetRuntimeStats(stats);
                applier.SetPlayerHealth(health);

                applier.ApplyAbility(new AbilityEffectContext(null, ability, null, 1));

                Assert.That(stats.MaxHealthBonus, Is.EqualTo(20f));
                Assert.That(health.MaxHealth, Is.EqualTo(120f));
                Assert.That(health.CurrentHealth, Is.EqualTo(120f));
            }
            finally
            {
                Object.DestroyImmediate(ability);
                Object.DestroyImmediate(player);
            }
        }

        private static AbilityDefinition CreateAbility(string abilityId, AbilityEffectType effectType, float value)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            var serializedObject = new SerializedObject(ability);
            serializedObject.FindProperty("abilityId").stringValue = abilityId;
            serializedObject.FindProperty("displayName").stringValue = abilityId;
            serializedObject.FindProperty("effectType").enumValueIndex = (int)effectType;
            serializedObject.FindProperty("maxStacks").intValue = 5;
            serializedObject.FindProperty("weight").floatValue = 100f;
            serializedObject.FindProperty("isEnabled").boolValue = true;
            serializedObject.FindProperty("value").floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return ability;
        }

        private static void DestroyAbilities(AbilityDefinition[] abilities)
        {
            for (var i = 0; i < abilities.Length; i++)
            {
                Object.DestroyImmediate(abilities[i]);
            }
        }
    }
}
