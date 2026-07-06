using NUnit.Framework;
using TapKnockout.UI;
using UnityEngine;

namespace TapKnockout.UI.Tests
{
    public sealed class DamageNumberSpawnerTests
    {
        [Test]
        public void ShowDamage_WithMissingCanvasAndPrefab_ReturnsFalse()
        {
            var spawnerObject = new GameObject("DamageNumberSpawner");

            try
            {
                var spawner = spawnerObject.AddComponent<DamageNumberSpawner>();

                Assert.That(spawner.ShowDamage(10f, Vector3.zero), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(spawnerObject);
            }
        }
    }
}
