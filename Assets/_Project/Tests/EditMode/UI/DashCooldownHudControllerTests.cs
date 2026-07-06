using NUnit.Framework;
using TapKnockout.Player;
using TapKnockout.UI;
using UnityEngine;

namespace TapKnockout.UI.Tests
{
    public sealed class DashCooldownHudControllerTests
    {
        [Test]
        public void Refresh_WithReadyDash_ShowsReadyText()
        {
            var hudObject = new GameObject("DashHud");
            var playerObject = new GameObject("Player");

            try
            {
                playerObject.AddComponent<Rigidbody>();
                playerObject.AddComponent<PlayerMovementController>();
                var dashController = playerObject.AddComponent<PlayerDashController>();

                var controller = hudObject.AddComponent<DashCooldownHudController>();
                controller.SetDashController(dashController);

                Assert.That(controller.CurrentText, Is.EqualTo("Dash Ready"));
                Assert.That(controller.CurrentFillAmount, Is.EqualTo(1f));
            }
            finally
            {
                Object.DestroyImmediate(playerObject);
                Object.DestroyImmediate(hudObject);
            }
        }
    }
}
