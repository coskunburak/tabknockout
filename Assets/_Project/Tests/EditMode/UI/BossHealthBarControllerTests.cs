using NUnit.Framework;
using TapKnockout.Boss;
using TapKnockout.Enemy;
using TapKnockout.UI;
using UnityEngine;

namespace TapKnockout.UI.Tests
{
    public sealed class BossHealthBarControllerTests
    {
        [Test]
        public void Refresh_WithMissingReferences_HandlesSafely()
        {
            var hudObject = new GameObject("BossHud");

            try
            {
                var controller = hudObject.AddComponent<BossHealthBarController>();
                controller.SetBoss(null, null, null);

                Assert.That(controller.CurrentFillAmount, Is.EqualTo(0f));
                Assert.That(controller.CurrentBossNameText, Is.EqualTo("Boss"));
            }
            finally
            {
                Object.DestroyImmediate(hudObject);
            }
        }

        [Test]
        public void Refresh_WithBossHealth_SetsFullFill()
        {
            var hudObject = new GameObject("BossHud");
            var bossObject = new GameObject("Boss");
            var config = ScriptableObject.CreateInstance<BossConfig>();

            try
            {
                var health = bossObject.AddComponent<EnemyHealth>();
                health.Initialize(null);

                var controller = hudObject.AddComponent<BossHealthBarController>();
                controller.SetBoss(config, health, null);

                Assert.That(controller.CurrentFillAmount, Is.EqualTo(1f).Within(0.001f));
                Assert.That(controller.CurrentBossNameText, Is.EqualTo(config.DisplayName));
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(bossObject);
                Object.DestroyImmediate(hudObject);
            }
        }
    }
}
