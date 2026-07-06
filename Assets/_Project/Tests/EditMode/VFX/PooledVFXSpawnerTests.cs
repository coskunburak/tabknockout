using NUnit.Framework;
using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.VFX.Tests
{
    public sealed class PooledVFXSpawnerTests
    {
        [Test]
        public void TrySpawn_WithMissingPrefab_ReturnsFalseWithoutThrowing()
        {
            var poolRoot = new GameObject("PoolRoot");
            var catalog = ScriptableObject.CreateInstance<VFXCatalog>();

            try
            {
                catalog.SetDefinitions(new[]
                {
                    new VFXDefinition(VFXEventType.DashImpact, null, 1, 0.1f)
                });

                var spawner = new PooledVFXSpawner(poolRoot.transform);
                var request = VFXSpawnRequest.Create(VFXEventType.DashImpact, Vector3.zero);

                Assert.That(spawner.TrySpawn(catalog, request), Is.False);
                Assert.That(spawner.ActiveCount, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(poolRoot);
            }
        }

        [Test]
        public void VFXService_WithMissingCatalog_ReturnsFalseWithoutThrowing()
        {
            var serviceObject = new GameObject("VFXService");

            try
            {
                var service = serviceObject.AddComponent<VFXService>();
                Assert.That(service.TrySpawn(VFXSpawnRequest.Create(VFXEventType.GenericBurst, Vector3.zero)), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(serviceObject);
            }
        }
    }
}
