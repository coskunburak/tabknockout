using NUnit.Framework;
using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.VFX.Tests
{
    public sealed class VFXCatalogTests
    {
        [Test]
        public void VFXSpawnRequest_DefaultStructValuesResolveToSafeRuntimeDefaults()
        {
            var request = new VFXSpawnRequest
            {
                EventType = VFXEventType.DashImpact,
                Position = Vector3.one
            };

            Assert.That(request.EventType, Is.EqualTo(VFXEventType.DashImpact));
            Assert.That(request.EffectiveRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(request.EffectiveScale, Is.EqualTo(Vector3.one));
            Assert.That(request.EffectiveIntensity, Is.EqualTo(1f));
            Assert.That(request.HasLifetimeOverride, Is.False);
            Assert.That(request.HasColorOverride, Is.False);
        }

        [Test]
        public void TryGetDefinition_ReturnsDefinitionByEventType()
        {
            var catalog = ScriptableObject.CreateInstance<VFXCatalog>();
            try
            {
                var definition = new VFXDefinition(VFXEventType.ProjectileHit, null, 0, 0.5f);
                catalog.SetDefinitions(new[] { definition });

                Assert.That(catalog.TryGetDefinition(VFXEventType.ProjectileHit, out var resolvedDefinition), Is.True);
                Assert.That(resolvedDefinition, Is.SameAs(definition));
                Assert.That(catalog.TryGetDefinition(VFXEventType.RoomClear, out _), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }
    }
}
