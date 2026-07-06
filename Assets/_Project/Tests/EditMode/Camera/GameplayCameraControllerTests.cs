using NUnit.Framework;
using TapKnockout.Camera;
using UnityEngine;

namespace TapKnockout.Camera.Tests
{
    public sealed class GameplayCameraControllerTests
    {
        [Test]
        public void SetFollowTarget_WithSnap_MovesCameraToAnchoredDesktopPosition()
        {
            var cameraObject = new GameObject("Gameplay Camera");
            var targetObject = new GameObject("Player Target");

            try
            {
                var unityCamera = cameraObject.AddComponent<UnityEngine.Camera>();
                unityCamera.aspect = 16f / 9f;
                var controller = cameraObject.AddComponent<GameplayCameraController>();
                targetObject.transform.position = new Vector3(1f, 0f, 2f);

                controller.SetFollowTarget(targetObject.transform, true);

                var rotation = CameraFramingUtility.ResolveRigRotation(52f, 0f);
                var viewHeight = CameraFramingUtility.ResolveViewHeight(true, 11.25f, 40f, 18f);
                var center = CameraFramingUtility.ResolveAnchoredCenterPosition(
                    targetObject.transform.position,
                    rotation,
                    new Vector2(0.5f, 0.49f),
                    viewHeight,
                    16f / 9f,
                    new Vector3(0f, 0.65f, 0f),
                    0.65f,
                    Vector3.zero);
                var expectedPosition = CameraFramingUtility.ResolveCameraPosition(center, rotation, 18f);

                Assert.That(Vector3.Distance(cameraObject.transform.position, expectedPosition), Is.LessThan(0.001f));
                Assert.That(controller.FollowTarget, Is.EqualTo(targetObject.transform));
                Assert.That(cameraObject.transform.rotation.eulerAngles.x, Is.EqualTo(52f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(targetObject);
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void TargetOverride_WithSnap_UsesOverrideUntilCleared()
        {
            var cameraObject = new GameObject("Gameplay Camera");
            var playerTarget = new GameObject("Player Target");
            var bossTarget = new GameObject("Boss Target");

            try
            {
                cameraObject.AddComponent<UnityEngine.Camera>().aspect = 16f / 9f;
                var controller = cameraObject.AddComponent<GameplayCameraController>();
                playerTarget.transform.position = Vector3.zero;
                bossTarget.transform.position = new Vector3(6f, 0f, 4f);

                controller.SetFollowTarget(playerTarget.transform, true);
                controller.SetTargetOverride(bossTarget.transform, true);

                Assert.That(controller.ActiveFollowTarget, Is.EqualTo(bossTarget.transform));

                controller.ClearTargetOverride(true);

                Assert.That(controller.ActiveFollowTarget, Is.EqualTo(playerTarget.transform));
            }
            finally
            {
                Object.DestroyImmediate(bossTarget);
                Object.DestroyImmediate(playerTarget);
                Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
