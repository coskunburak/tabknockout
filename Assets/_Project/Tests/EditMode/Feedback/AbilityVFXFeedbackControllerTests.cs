using NUnit.Framework;
using TapKnockout.Ability;
using TapKnockout.Combat;
using TapKnockout.Player;
using TapKnockout.VFX;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Feedback.Tests
{
    public sealed class AbilityVFXFeedbackControllerTests
    {
        [Test]
        public void Resolver_MapsVerticalSliceAbilityFamiliesToProductionEvents()
        {
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.MaxHealthUp), Is.EqualTo(VFXEventType.AbilityHealthBuff));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.AttackDamageUp), Is.EqualTo(VFXEventType.AbilityAttackBuff));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.AttackSpeedUp), Is.EqualTo(VFXEventType.AbilityAttackSpeedBuff));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.MoveSpeedUp), Is.EqualTo(VFXEventType.AbilityMoveSpeedBuff));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.ProjectileSpeedUp), Is.EqualTo(VFXEventType.AbilityProjectileBuff));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.DashDamageUp), Is.EqualTo(VFXEventType.AbilityDashBuff));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.DashCooldownDown), Is.EqualTo(VFXEventType.AbilityDashBuff));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.DashKnockbackUp), Is.EqualTo(VFXEventType.AbilityDashBuff));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.DashIFrameDurationUp), Is.EqualTo(VFXEventType.AbilityDashPhase));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.ExtraProjectile), Is.EqualTo(VFXEventType.AbilityProjectileSplit));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.FrontProjectile), Is.EqualTo(VFXEventType.AbilityProjectileSplit));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.ProjectilePierce), Is.EqualTo(VFXEventType.AbilityProjectilePierce));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.ProjectileRicochet), Is.EqualTo(VFXEventType.AbilityProjectileRicochet));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.BurnOnHit), Is.EqualTo(VFXEventType.AbilityFireProc));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.PoisonOnHit), Is.EqualTo(VFXEventType.AbilityPoisonProc));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.FreezeOnHit), Is.EqualTo(VFXEventType.AbilityIceProc));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.LightningOnHit), Is.EqualTo(VFXEventType.AbilityLightningProc));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.ShieldPerRoom), Is.EqualTo(VFXEventType.AbilityShield));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.HealOnKill), Is.EqualTo(VFXEventType.AbilitySoulHeal));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.BossDamageUp), Is.EqualTo(VFXEventType.AbilityBossBreaker));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.LowHealthDamageUp), Is.EqualTo(VFXEventType.AbilityLowHealthSurge));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.DashShockwave), Is.EqualTo(VFXEventType.AbilityDashShockwave));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.DashStun), Is.EqualTo(VFXEventType.AbilityDashStagger));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.DashCooldownRefundOnKill), Is.EqualTo(VFXEventType.AbilityDashBuff));
            Assert.That(AbilityVFXEventResolver.ResolveSelectionEvent(AbilityEffectType.ProjectileSizeUp), Is.EqualTo(VFXEventType.AbilityProjectileSize));
        }

        [Test]
        public void Resolver_MapsElementalDamageTypesToAbilityHitAccents()
        {
            Assert.That(AbilityVFXEventResolver.TryResolveDamageTypeEvent(DamageType.Fire, out var fireEvent), Is.True);
            Assert.That(fireEvent, Is.EqualTo(VFXEventType.AbilityFireProc));

            Assert.That(AbilityVFXEventResolver.TryResolveDamageTypeEvent(DamageType.Poison, out var poisonEvent), Is.True);
            Assert.That(poisonEvent, Is.EqualTo(VFXEventType.AbilityPoisonProc));

            Assert.That(AbilityVFXEventResolver.TryResolveDamageTypeEvent(DamageType.Ice, out var iceEvent), Is.True);
            Assert.That(iceEvent, Is.EqualTo(VFXEventType.AbilityIceProc));

            Assert.That(AbilityVFXEventResolver.TryResolveDamageTypeEvent(DamageType.Lightning, out var lightningEvent), Is.True);
            Assert.That(lightningEvent, Is.EqualTo(VFXEventType.AbilityLightningProc));

            Assert.That(AbilityVFXEventResolver.TryResolveDamageTypeEvent(DamageType.Physical, out _), Is.False);
        }

        [Test]
        public void TrySpawnSelectionVFX_WithMissingServicePublishesRequestWithoutThrowing()
        {
            var controllerObject = new GameObject("AbilityVFXFeedbackController");
            var playerObject = new GameObject("PlayerRuntimeStats");
            var ability = CreateAbility(AbilityEffectType.BurnOnHit);

            try
            {
                var playerStats = playerObject.AddComponent<PlayerRuntimeStats>();
                var controller = controllerObject.AddComponent<AbilityVFXFeedbackController>();
                controller.SetPlayerRuntimeStats(playerStats);

                var requestCount = 0;
                var requestedEvent = VFXEventType.GenericBurst;
                controller.OnVFXRequested += request =>
                {
                    requestCount++;
                    requestedEvent = request.EventType;
                };

                Assert.That(controller.TrySpawnSelectionVFX(ability, 1), Is.False);
                Assert.That(requestCount, Is.EqualTo(1));
                Assert.That(requestedEvent, Is.EqualTo(VFXEventType.AbilityFireProc));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(ability);
                UnityEngine.Object.DestroyImmediate(playerObject);
                UnityEngine.Object.DestroyImmediate(controllerObject);
            }
        }

        private static AbilityDefinition CreateAbility(AbilityEffectType effectType)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            var serializedObject = new SerializedObject(ability);
            serializedObject.FindProperty("effectType").intValue = (int)effectType;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return ability;
        }
    }
}
