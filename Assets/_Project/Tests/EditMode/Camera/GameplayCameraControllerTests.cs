using NUnit.Framework;
using TapKnockout.Camera;
using UnityEngine;

namespace TapKnockout.Camera.Tests
{
    public sealed class GameplayCameraControllerTests
    {
        [Test]
        public void SetFollowTarget_WithSnap_MovesCameraToAnchoredPortraitPosition()
        {
            var cameraObject = new GameObject("Gameplay Camera");
            var targetObject = new GameObject("Player Target");

            try
            {
                var unityCamera = cameraObject.AddComponent<UnityEngine.Camera>();
                unityCamera.aspect = 9f / 19.5f;
                var controller = cameraObject.AddComponent<GameplayCameraController>();
                targetObject.transform.position = new Vector3(1f, 0f, 2f);

                controller.SetFollowTarget(targetObject.transform, true);

                var rotation = CameraFramingUtility.ResolveRigRotation(58f, 0f);
                var viewHeight = CameraFramingUtility.ResolveViewHeight(true, 16.5f, 42f, 24f);
                var center = CameraFramingUtility.ResolveAnchoredCenterPosition(
                    targetObject.transform.position,
                    rotation,
                    new Vector2(0.5f, 0.44f),
                    viewHeight,
                    9f / 19.5f,
                    new Vector3(0f, 0.85f, 0f),
                    0.25f,
                    Vector3.zero);
                var expectedPosition = CameraFramingUtility.ResolveCameraPosition(center, rotation, 24f);

                Assert.That(Vector3.Distance(cameraObject.transform.position, expectedPosition), Is.LessThan(0.001f));
                Assert.That(controller.FollowTarget, Is.EqualTo(targetObject.transform));
                Assert.That(cameraObject.transform.rotation.eulerAngles.x, Is.EqualTo(58f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(targetObject);
                Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
