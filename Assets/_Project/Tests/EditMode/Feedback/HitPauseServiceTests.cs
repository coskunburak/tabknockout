using NUnit.Framework;
using TapKnockout.Feedback;
using UnityEngine;

namespace TapKnockout.Feedback.Tests
{
    public sealed class HitPauseServiceTests
    {
        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
        }

        [Test]
        public void RequestHitPause_RestoresTimeScaleWhenTickCompletes()
        {
            var serviceObject = new GameObject("HitPauseService");

            try
            {
                Time.timeScale = 1f;
                var service = serviceObject.AddComponent<HitPauseService>();

                Assert.That(service.RequestHitPause(0.04f), Is.True);
                Assert.That(Time.timeScale, Is.EqualTo(0f));

                service.Tick(0.05f);

                Assert.That(service.IsPauseActive, Is.False);
                Assert.That(Time.timeScale, Is.EqualTo(1f));
            }
            finally
            {
                Object.DestroyImmediate(serviceObject);
            }
        }

        [Test]
        public void RequestHitPause_WhenAlreadyPausedExternally_ReturnsFalse()
        {
            var serviceObject = new GameObject("HitPauseService");

            try
            {
                Time.timeScale = 0f;
                var service = serviceObject.AddComponent<HitPauseService>();

                Assert.That(service.RequestHitPause(0.04f), Is.False);
                Assert.That(service.IsPauseActive, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(serviceObject);
            }
        }
    }
}
