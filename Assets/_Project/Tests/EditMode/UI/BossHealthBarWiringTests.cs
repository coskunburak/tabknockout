using NUnit.Framework;
using TapKnockout.Boss;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.UI.Tests
{
    public sealed class BossHealthBarWiringTests
    {
        [Test]
        public void SetBossFromGameObject_BindsHealthAndShowsFullFill()
        {
            var uiRoot = new GameObject("BossHealthBar");
            var boss = new GameObject("Boss");

            try
            {
                var controller = uiRoot.AddComponent<BossHealthBarController>();
                boss.AddComponent<EnemyHealth>().Initialize(null);
                boss.AddComponent<BossPhaseController>();

                controller.SetBossFromGameObject(boss, null);

                Assert.That(controller.CurrentFillAmount, Is.EqualTo(1f).Within(0.001f));
                Assert.That(controller.IsVisible, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(uiRoot);
                Object.DestroyImmediate(boss);
            }
        }

        [Test]
        public void SetBossFromGameObject_WithNullBoss_DoesNotThrow()
        {
            var uiRoot = new GameObject("BossHealthBar");

            try
            {
                var controller = uiRoot.AddComponent<BossHealthBarController>();

                Assert.DoesNotThrow(() => controller.SetBossFromGameObject(null, null));
                Assert.That(controller.CurrentFillAmount, Is.EqualTo(0f));
            }
            finally
            {
                Object.DestroyImmediate(uiRoot);
            }
        }
    }
}
