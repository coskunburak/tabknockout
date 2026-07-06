using NUnit.Framework;
using TapKnockout.Pickups;
using UnityEngine;

namespace TapKnockout.Pickups.Tests
{
    public sealed class XPOrbVisualFeedbackTests
    {
        [Test]
        public void ApplyGlow_WithNoRenderer_DoesNotThrow()
        {
            var orbObject = new GameObject("XPOrb");

            try
            {
                orbObject.AddComponent<XPOrb>();
                var visual = orbObject.AddComponent<XPOrbVisualFeedback>();

                Assert.That(visual.HasRenderer, Is.False);
                Assert.DoesNotThrow(() => visual.ApplyGlow());
                Assert.DoesNotThrow(() => visual.TickVisual(0.2f));
            }
            finally
            {
                Object.DestroyImmediate(orbObject);
            }
        }
    }
}
