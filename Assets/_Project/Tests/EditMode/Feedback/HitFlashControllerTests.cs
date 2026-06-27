using NUnit.Framework;
using TapKnockout.Feedback;
using UnityEngine;

namespace TapKnockout.Feedback.Tests
{
    public sealed class HitFlashControllerTests
    {
        [Test]
        public void Flash_WithNoRenderer_ReturnsFalseWithoutThrowing()
        {
            var target = new GameObject("Target");

            try
            {
                var hitFlash = target.AddComponent<HitFlashController>();
                hitFlash.CacheRenderers();

                Assert.That(hitFlash.RendererCount, Is.EqualTo(0));
                Assert.That(hitFlash.Flash(0.1f), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
