using NUnit.Framework;
using TapKnockout.UI;
using UnityEngine;

namespace TapKnockout.UI.Tests
{
    public sealed class RunStatusHudControllerTests
    {
        [Test]
        public void Refresh_WithoutChapterRunner_ShowsReadyStatus()
        {
            var gameObject = new GameObject("RunStatusHud");

            try
            {
                var controller = gameObject.AddComponent<RunStatusHudController>();

                controller.Refresh();

                Assert.That(controller.CurrentStatusText, Is.EqualTo("Chapter Ready"));
                Assert.That(controller.CurrentRoomText, Is.EqualTo(string.Empty));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
