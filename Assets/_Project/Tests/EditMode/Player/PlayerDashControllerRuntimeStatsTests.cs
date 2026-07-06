using System.Reflection;
using NUnit.Framework;
using TapKnockout.Combat;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Player.Tests
{
    public sealed class PlayerDashControllerRuntimeStatsTests
    {
        [Test]
        public void EffectiveDashValues_UseRuntimeStatsWhenAvailable()
        {
            var player = new GameObject("Player");

            try
            {
                player.AddComponent<Rigidbody>();
                var stats = player.AddComponent<PlayerRuntimeStats>();
                player.AddComponent<PlayerMovementController>();
                var dashController = player.AddComponent<PlayerDashController>();

                stats.AddDashCooldownReduction(0.5f);
                stats.AddDashDamageMultiplier(0.5f);
                stats.AddDashKnockbackMultiplier(0.25f);

                Assert.That(dashController.EffectiveDashCooldown, Is.EqualTo(2f).Within(0.0001f));
                Assert.That(dashController.EffectiveDashImpactDamage, Is.EqualTo(18f).Within(0.0001f));
                Assert.That(dashController.EffectiveDashKnockbackForce, Is.EqualTo(10f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void FixedUpdate_DashSegmentHitSettlesTargetVelocityAndSuppressesPhysicalPush()
        {
            var player = new GameObject("Player");
            var target = new GameObject("Target");

            try
            {
                var playerCollider = player.AddComponent<CapsuleCollider>();
                playerCollider.radius = 0.35f;
                playerCollider.height = 1.8f;
                playerCollider.center = new Vector3(0f, 0.9f, 0f);

                player.AddComponent<Rigidbody>();
                var movement = player.AddComponent<PlayerMovementController>();
                var dashController = player.AddComponent<PlayerDashController>();
                SetPrivate(dashController, "fallbackDashDistance", 10f);
                SetPrivate(dashController, "fallbackDashDuration", Time.fixedDeltaTime);
                SetPrivate(dashController, "fallbackDashCooldown", 1f);
                SetPrivate(dashController, "fallbackDashHitRadius", 0.45f);
                SetPrivate(dashController, "fallbackDashHitLayers", new LayerMask { value = Physics.AllLayers });
                SetPrivate(dashController, "dashCollisionSuppressionGrace", 1f);
                SetPrivate(dashController, "embeddedDashKnockbackForceMultiplier", 0.25f);
                SetPrivate(dashController, "embeddedDashKnockbackMaxDuration", 0.08f);
                SetPrivate(dashController, "logSetupWarnings", false);

                target.transform.position = new Vector3(0f, 0f, 5f);
                var targetCollider = target.AddComponent<SphereCollider>();
                targetCollider.radius = 0.5f;
                var targetRigidbody = target.AddComponent<Rigidbody>();
                targetRigidbody.useGravity = false;
                targetRigidbody.linearVelocity = Vector3.forward * 20f;
                var damageable = target.AddComponent<TestDamageable>();

                InvokePrivate(movement, "Awake");
                InvokePrivate(dashController, "Awake");
                Physics.SyncTransforms();

                Assert.That(dashController.TryDash(), Is.True);
                InvokePrivate(dashController, "FixedUpdate");

                Assert.That(damageable.HitCount, Is.EqualTo(1));
                Assert.That(targetRigidbody.linearVelocity, Is.EqualTo(Vector3.zero));
                Assert.That(Physics.GetIgnoreCollision(playerCollider, targetCollider), Is.True);

                InvokePrivate(dashController, "OnDisable");
                Assert.That(Physics.GetIgnoreCollision(playerCollider, targetCollider), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void FixedUpdate_DashHitWhileOverlappingLimitsKnockback()
        {
            var player = new GameObject("Player");
            var target = new GameObject("OverlappingTarget");

            try
            {
                var playerCollider = player.AddComponent<CapsuleCollider>();
                playerCollider.radius = 0.35f;
                playerCollider.height = 1.8f;
                playerCollider.center = new Vector3(0f, 0.9f, 0f);

                player.AddComponent<Rigidbody>();
                var movement = player.AddComponent<PlayerMovementController>();
                var dashController = player.AddComponent<PlayerDashController>();
                SetPrivate(dashController, "fallbackDashDistance", 3.5f);
                SetPrivate(dashController, "fallbackDashDuration", Time.fixedDeltaTime);
                SetPrivate(dashController, "fallbackDashCooldown", 1f);
                SetPrivate(dashController, "fallbackDashKnockbackForce", 8f);
                SetPrivate(dashController, "fallbackDashKnockbackDuration", 0.2f);
                SetPrivate(dashController, "fallbackDashHitRadius", 0.9f);
                SetPrivate(dashController, "fallbackDashHitLayers", new LayerMask { value = Physics.AllLayers });
                SetPrivate(dashController, "embeddedDashKnockbackForceMultiplier", 0.25f);
                SetPrivate(dashController, "embeddedDashKnockbackMaxDuration", 0.08f);
                SetPrivate(dashController, "dashCollisionSuppressionGrace", 1f);
                SetPrivate(dashController, "logSetupWarnings", false);

                target.transform.position = new Vector3(0f, 0f, 0.25f);
                var targetCollider = target.AddComponent<SphereCollider>();
                targetCollider.radius = 0.55f;
                var targetRigidbody = target.AddComponent<Rigidbody>();
                targetRigidbody.useGravity = false;
                var damageable = target.AddComponent<TestDamageable>();

                InvokePrivate(movement, "Awake");
                InvokePrivate(dashController, "Awake");
                Physics.SyncTransforms();

                Assert.That(dashController.TryDash(), Is.True);
                InvokePrivate(dashController, "FixedUpdate");

                Assert.That(damageable.HitCount, Is.EqualTo(1));
                Assert.That(damageable.LastHitContext, Is.Not.Null);
                Assert.That(damageable.LastHitContext.Knockback.Force, Is.InRange(0f, 2.001f));
                Assert.That(damageable.LastHitContext.Knockback.Duration, Is.InRange(0f, 0.081f));
                Assert.That(targetRigidbody.linearVelocity, Is.EqualTo(Vector3.zero));
                Assert.That(Physics.GetIgnoreCollision(playerCollider, targetCollider), Is.True);

                InvokePrivate(dashController, "OnDisable");
            }
            finally
            {
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(player);
            }
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            target.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(target, value);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            target.GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(target, null);
        }

        private sealed class TestDamageable : MonoBehaviour, IDamageable
        {
            public bool IsAlive { get; private set; } = true;
            public GameObject GameObject => gameObject;
            public int HitCount { get; private set; }
            public HitContext LastHitContext { get; private set; }

            public void ReceiveHit(HitContext hitContext)
            {
                HitCount++;
                LastHitContext = hitContext;
            }
        }
    }
}
