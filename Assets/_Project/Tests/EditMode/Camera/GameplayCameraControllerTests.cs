using NUnit.Framework;
using TapKnockout.Camera;
using UnityEngine;

namespace TapKnockout.Camera.Tests
{
    public sealed class GameplayCameraControllerTests
    {
        [Test]
        public void SetFollowTarget_WithSnap_MovesCameraToFallbackFollowPosition()
        {
            var cameraObject = new GameObject("Gameplay Camera");
            var targetObject = new GameObject("Player Target");

            try
            {
                cameraObject.AddComponent<UnityEngine.Camera>();
                var controller = cameraObject.AddComponent<GameplayCameraController>();
                targetObject.transform.position = new Vector3(1f, 0f, 2f);

                controller.SetFollowTarget(targetObject.transform, true);

                Assert.That(cameraObject.transform.position, Is.EqualTo(new Vector3(1f, 12.5f, -6.75f)));
                Assert.That(controller.FollowTarget, Is.EqualTo(targetObject.transform));
            }
            finally
            {
                Object.DestroyImmediate(targetObject);
                Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
