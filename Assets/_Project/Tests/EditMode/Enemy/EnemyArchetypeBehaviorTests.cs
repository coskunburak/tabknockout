using NUnit.Framework;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Enemy.Tests
{
    public sealed class EnemyArchetypeBehaviorTests
    {
        [Test]
        public void FastCharger_LocksDirectionAfterTelegraph()
        {
            var chargerObject = new GameObject("FastCharger");
            var targetObject = new GameObject("Player");

            try
            {
                chargerObject.AddComponent<Rigidbody>();
                chargerObject.transform.position = Vector3.zero;
                targetObject.transform.position = Vector3.forward * 5f;

                var charger = chargerObject.AddComponent<FastChargerController>();
                charger.Initialize(null, targetObject.transform);

                Assert.That(charger.TryBeginChargeWindup(), Is.True);
                charger.Advance(1f);
                Assert.That(charger.State, Is.EqualTo(FastChargerController.ChargeState.Charging));

                var lockedDirection = charger.LockedChargeDirection;
                targetObject.transform.position = Vector3.right * 5f;
                charger.Advance(0.01f);

                Assert.That(charger.LockedChargeDirection.x, Is.EqualTo(lockedDirection.x).Within(0.001f));
                Assert.That(charger.LockedChargeDirection.z, Is.EqualTo(lockedDirection.z).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(chargerObject);
                Object.DestroyImmediate(targetObject);
            }
        }

        [Test]
        public void RangedShooter_CreatesProjectileRequestSafely()
        {
            var shooterObject = new GameObject("RangedShooter");
            var targetObject = new GameObject("Player");

            try
            {
                shooterObject.transform.position = Vector3.zero;
                targetObject.transform.position = Vector3.forward * 4f;

                var shooter = shooterObject.AddComponent<RangedShooterController>();
                shooter.Initialize(null, targetObject.transform);

                var request = shooter.CreateProjectileRequest();
                Assert.That(request.CanSpawn, Is.True);
                Assert.That(request.Source, Is.EqualTo(shooterObject));
                Assert.That(request.Target, Is.EqualTo(targetObject));
                Assert.That(request.Speed, Is.GreaterThan(0f));
            }
            finally
            {
                Object.DestroyImmediate(shooterObject);
                Object.DestroyImmediate(targetObject);
            }
        }

        [Test]
        public void AreaBomber_DamageTimingWaitsForWindupCrossing()
        {
            Assert.That(AreaBomberController.ShouldResolveDamageAfterTick(0.5f, 0.25f), Is.False);
            Assert.That(AreaBomberController.ShouldResolveDamageAfterTick(0.5f, 0.5f), Is.True);
            Assert.That(AreaBomberController.ShouldResolveDamageAfterTick(0.5f, 0.75f), Is.True);
        }
    }
}
