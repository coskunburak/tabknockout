using NUnit.Framework;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Player.Tests
{
    public sealed class PlayerAttackControllerTests
    {
        [Test]
        public void SetWeaponConfig_AssignsWeaponAndResetsControllerState()
        {
            var player = new GameObject("Player");
            var weaponConfig = ScriptableObject.CreateInstance<WeaponConfig>();

            try
            {
                player.AddComponent<Rigidbody>();
                player.AddComponent<PlayerMovementController>();
                player.AddComponent<PlayerTargetProvider>();
                var attackController = player.AddComponent<PlayerAttackController>();

                attackController.SetWeaponConfig(weaponConfig);

                Assert.That(attackController.WeaponConfig, Is.EqualTo(weaponConfig));
                Assert.That(attackController.IsCooldownReady, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(weaponConfig);
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void EffectiveAttackValues_UseRuntimeStatsWhenAvailable()
        {
            var player = new GameObject("Player");
            var weaponConfig = ScriptableObject.CreateInstance<WeaponConfig>();

            try
            {
                player.AddComponent<Rigidbody>();
                player.AddComponent<PlayerRuntimeStats>();
                player.AddComponent<PlayerMovementController>();
                player.AddComponent<PlayerTargetProvider>();
                var attackController = player.AddComponent<PlayerAttackController>();
                var stats = player.GetComponent<PlayerRuntimeStats>();

                attackController.SetWeaponConfig(weaponConfig);
                stats.AddAttackDamageMultiplier(0.5f);
                stats.AddAttackCooldownReduction(0.25f);

                Assert.That(attackController.EffectiveAttackDamage, Is.EqualTo(15f).Within(0.0001f));
                Assert.That(attackController.EffectiveAttackCooldown, Is.EqualTo(0.6f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(weaponConfig);
                Object.DestroyImmediate(player);
            }
        }
    }
}
