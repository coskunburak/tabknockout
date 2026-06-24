using NUnit.Framework;
using TapKnockout.Combat;
using TapKnockout.Projectile;
using UnityEngine;

namespace TapKnockout.Projectile.Tests
{
    public sealed class ProjectileControllerTests
    {
        [Test]
        public void Initialize_StoresRuntimeStateWithoutRequiringPrefabSetup()
        {
            var projectile = new GameObject("Projectile");

            try
            {
                projectile.AddComponent<SphereCollider>();
                var controller = projectile.AddComponent<ProjectileController>();
                var hitContext = new HitContext();

                controller.Initialize(hitContext, Vector3.forward, 12f, 3f, null);

                Assert.That(controller.IsInitialized, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(projectile);
            }
        }
    }
}
