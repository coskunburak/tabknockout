using System;
using NUnit.Framework;
using UnityEngine;

namespace TapKnockout.Ability.Tests
{
    public sealed class AbilityDefinitionExpansionTests
    {
        [Test]
        public void NewMetadataFields_HaveSafeDefaults()
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();

            try
            {
                Assert.That(ability.AbilityTags, Is.Not.Null);
                Assert.That(ability.RequiredTags, Is.Not.Null);
                Assert.That(ability.BlockedTags, Is.Not.Null);
                Assert.That(ability.PrerequisiteAbilityIds, Is.Not.Null);
                Assert.That(ability.ImplementationStatus, Is.EqualTo(AbilityImplementationStatus.Implemented));
                Assert.That(ability.IsPlaceholder, Is.False);
                Assert.That(ability.Cooldown, Is.EqualTo(0f));
                Assert.That(ability.ProcChance, Is.EqualTo(0f));
                Assert.That(ability.IsImplementedForNormalOffers, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(ability);
            }
        }

        [Test]
        public void AbilityEffectType_IncludesExpandedFamilies()
        {
            Assert.That(Enum.IsDefined(typeof(AbilityEffectType), AbilityEffectType.DashIFrameDurationUp), Is.True);
            Assert.That(Enum.IsDefined(typeof(AbilityEffectType), AbilityEffectType.ProjectileRicochet), Is.True);
            Assert.That(Enum.IsDefined(typeof(AbilityEffectType), AbilityEffectType.PoisonOnHit), Is.True);
            Assert.That(Enum.IsDefined(typeof(AbilityEffectType), AbilityEffectType.OrbitalNeutral), Is.True);
            Assert.That(Enum.IsDefined(typeof(AbilityEffectType), AbilityEffectType.DroneBasic), Is.True);
            Assert.That(Enum.IsDefined(typeof(AbilityEffectType), AbilityEffectType.MeteorOnAttack), Is.True);
            Assert.That(Enum.IsDefined(typeof(AbilityEffectType), AbilityEffectType.LowHealthDamageUp), Is.True);
            Assert.That(Enum.IsDefined(typeof(AbilityEffectType), AbilityEffectType.RewardLuckUp), Is.True);
        }
    }
}
