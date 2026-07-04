using NUnit.Framework;
using TapKnockout.Player;
using TapKnockout.UI;
using UnityEngine;

namespace TapKnockout.UI.Tests
{
    public sealed class PlayerHealthHudControllerTests
    {
        [Test]
        public void Refresh_WithPlayerHealth_ShowsHpText()
        {
            var hudObject = new GameObject("HealthHud");
            var playerObject = new GameObject("Player");

            try
            {
                var health = playerObject.AddComponent<PlayerHealth>();
                health.ResetHealth();

                var controller = hudObject.AddComponent<PlayerHealthHudController>();
                controller.SetPlayerHealth(health);

                Assert.That(controller.CurrentText, Is.EqualTo("HP 100 / 100"));
            }
            finally
            {
                Object.DestroyImmediate(playerObject);
                Object.DestroyImmediate(hudObject);
            }
        }
    }
}
