using System.Reflection;
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

        [Test]
        public void Initialize_ForcesColliderToTriggerToAvoidPhysicalProjectilePush()
        {
            var projectile = new GameObject("Projectile");

            try
            {
                var projectileCollider = projectile.AddComponent<SphereCollider>();
                projectileCollider.isTrigger = false;
                var rigidbody = projectile.AddComponent<Rigidbody>();
                rigidbody.useGravity = true;
                var controller = projectile.AddComponent<ProjectileController>();

                controller.Initialize(new HitContext(), Vector3.forward, 12f, 3f, null);

                Assert.That(projectileCollider.isTrigger, Is.True);
                Assert.That(rigidbody.useGravity, Is.False);
                Assert.That(rigidbody.collisionDetectionMode, Is.EqualTo(CollisionDetectionMode.ContinuousDynamic));
            }
            finally
            {
                Object.DestroyImmediate(projectile);
            }
        }

        [Test]
        public void Initialize_ResolvesDamageableAlreadyOverlappingProjectile()
        {
            var projectile = new GameObject("Projectile");
            var target = new GameObject("Target");

            try
            {
                projectile.transform.position = Vector3.zero;
                target.transform.position = Vector3.zero;

                var projectileCollider = projectile.AddComponent<SphereCollider>();
                projectileCollider.radius = 0.15f;
                var controller = projectile.AddComponent<ProjectileController>();
                SetDeactivateInsteadOfDestroy(controller);

                var targetCollider = target.AddComponent<SphereCollider>();
                targetCollider.radius = 0.5f;
                var damageable = target.AddComponent<TestDamageable>();

                Physics.SyncTransforms();

                controller.Initialize(new HitContext(null, null, 7f), Vector3.forward, 12f, 3f, null);

                Assert.That(damageable.HitCount, Is.EqualTo(1));
                Assert.That(damageable.LastHit.IsProjectileHit, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(projectile);
            }
        }

        [Test]
        public void Update_ResolvesDamageableCrossedBetweenFrames()
        {
            var projectile = new GameObject("Projectile");
            var target = new GameObject("Target");

            try
            {
                projectile.transform.position = Vector3.zero;
                target.transform.position = new Vector3(0f, 0f, 5f);

                var projectileCollider = projectile.AddComponent<SphereCollider>();
                projectileCollider.radius = 0.15f;
                var controller = projectile.AddComponent<ProjectileController>();
                SetDeactivateInsteadOfDestroy(controller);

                var targetCollider = target.AddComponent<SphereCollider>();
                targetCollider.radius = 0.5f;
                var damageable = target.AddComponent<TestDamageable>();

                Physics.SyncTransforms();

                controller.Initialize(new HitContext(null, null, 7f), Vector3.forward, 12f, 3f, null);
                projectile.transform.position = new Vector3(0f, 0f, 10f);
                Physics.SyncTransforms();

                InvokeUpdate(controller);

                Assert.That(damageable.HitCount, Is.EqualTo(1));
                Assert.That(damageable.LastHit.IsProjectileHit, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(projectile);
            }
        }

        [Test]
        public void Update_PiercingProjectileHitsMultipleTargetsInSameSweep()
        {
            var projectile = new GameObject("Projectile");
            var firstTarget = new GameObject("FirstTarget");
            var secondTarget = new GameObject("SecondTarget");

            try
            {
                projectile.transform.position = Vector3.zero;
                firstTarget.transform.position = new Vector3(0f, 0f, 2f);
                secondTarget.transform.position = new Vector3(0f, 0f, 4f);

                var projectileCollider = projectile.AddComponent<SphereCollider>();
                projectileCollider.radius = 0.1f;
                var controller = projectile.AddComponent<ProjectileController>();
                SetDeactivateInsteadOfDestroy(controller);

                firstTarget.AddComponent<SphereCollider>().radius = 0.35f;
                secondTarget.AddComponent<SphereCollider>().radius = 0.35f;
                var firstDamageable = firstTarget.AddComponent<TestDamageable>();
                var secondDamageable = secondTarget.AddComponent<TestDamageable>();

                Physics.SyncTransforms();

                var modifiers = new ProjectileModifierState(0, 0, 0, 0, 0, 1, 0, 0, 0f, 1f, 1f);
                controller.Initialize(new HitContext(null, null, 7f), Vector3.forward, 12f, 3f, null, modifiers);
                projectile.transform.position = new Vector3(0f, 0f, 5f);
                Physics.SyncTransforms();

                InvokeUpdate(controller);

                Assert.That(firstDamageable.HitCount, Is.EqualTo(1));
                Assert.That(secondDamageable.HitCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(secondTarget);
                Object.DestroyImmediate(firstTarget);
                Object.DestroyImmediate(projectile);
            }
        }

        [Test]
        public void FixedUpdate_RigidbodyProjectilePredictsHighSpeedHitBeforePhysicsStep()
        {
            var projectile = new GameObject("Projectile");
            var target = new GameObject("Target");

            try
            {
                projectile.transform.position = Vector3.zero;
                target.transform.position = new Vector3(0f, 0f, 5f);

                var projectileCollider = projectile.AddComponent<SphereCollider>();
                projectileCollider.radius = 0.15f;
                projectile.AddComponent<Rigidbody>();
                var controller = projectile.AddComponent<ProjectileController>();
                SetDeactivateInsteadOfDestroy(controller);

                target.AddComponent<SphereCollider>().radius = 0.5f;
                var damageable = target.AddComponent<TestDamageable>();

                Physics.SyncTransforms();

                controller.Initialize(new HitContext(null, null, 7f), Vector3.forward, 300f, 3f, null);
                InvokeFixedUpdate(controller);

                Assert.That(damageable.HitCount, Is.EqualTo(1));
                Assert.That(damageable.LastHit.IsProjectileHit, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(projectile);
            }
        }

        private static void SetDeactivateInsteadOfDestroy(ProjectileController controller)
        {
            typeof(ProjectileController)
                .GetField("deactivateInsteadOfDestroy", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(controller, true);
        }

        private static void InvokeUpdate(ProjectileController controller)
        {
            typeof(ProjectileController)
                .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(controller, null);
        }

        private static void InvokeFixedUpdate(ProjectileController controller)
        {
            typeof(ProjectileController)
                .GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(controller, null);
        }

        private sealed class TestDamageable : MonoBehaviour, IDamageable
        {
            public bool IsAlive { get; private set; } = true;
            public GameObject GameObject => gameObject;
            public int HitCount { get; private set; }
            public HitContext LastHit { get; private set; }

            public void ReceiveHit(HitContext hitContext)
            {
                HitCount++;
                LastHit = hitContext;
            }
        }
    }
}
