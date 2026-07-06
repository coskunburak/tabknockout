using NUnit.Framework;
using TapKnockout.Camera;
using UnityEngine;

namespace TapKnockout.Camera.Tests
{
    public sealed class CameraShakeReceiverTests
    {
        [Test]
        public void Shake_DecaysAndRestoresLocalPosition()
        {
            var cameraObject = new GameObject("Camera");

            try
            {
                var receiver = cameraObject.AddComponent<CameraShakeReceiver>();

                receiver.Shake(0.05f, 0.08f);
                Assert.That(receiver.IsShaking, Is.True);

                receiver.Tick(0.1f);

                Assert.That(receiver.IsShaking, Is.False);
                Assert.That(cameraObject.transform.localPosition, Is.EqualTo(Vector3.zero));
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
