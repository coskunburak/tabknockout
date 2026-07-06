using NUnit.Framework;
using UnityEngine;

namespace TapKnockout.Enemy.Tests
{
    public sealed class EnemySpawnPlacementTests
    {
        [Test]
        public void ResolveGroundedPosition_OffsetsRootSoColliderBottomStartsAboveGround()
        {
            var enemy = new GameObject("Enemy");
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);

            try
            {
                floor.name = "Floor";
                floor.transform.position = new Vector3(0f, -0.05f, 0f);
                floor.transform.localScale = new Vector3(8f, 0.1f, 8f);

                var collider = enemy.AddComponent<CapsuleCollider>();
                collider.center = Vector3.zero;
                collider.height = 2f;
                collider.radius = 0.5f;

                Physics.SyncTransforms();

                var resolved = EnemySpawnPlacement.ResolveGroundedPosition(
                    enemy,
                    Vector3.zero,
                    0f,
                    true,
                    ~0,
                    4f,
                    8f,
                    0.03f);

                Assert.That(resolved.y, Is.EqualTo(1.03f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(enemy);
                Object.DestroyImmediate(floor);
            }
        }

        [Test]
        public void PrepareRigidbodyForArenaSpawn_DisablesGravityAndClearsVelocity()
        {
            var enemy = new GameObject("Enemy");

            try
            {
                var body = enemy.AddComponent<Rigidbody>();
                body.useGravity = true;
                body.linearVelocity = Vector3.forward * 3f;
                body.angularVelocity = Vector3.up * 2f;

                EnemySpawnPlacement.PrepareRigidbodyForArenaSpawn(enemy, true);

                Assert.That(body.useGravity, Is.False);
                Assert.That(body.linearVelocity, Is.EqualTo(Vector3.zero));
                Assert.That(body.angularVelocity, Is.EqualTo(Vector3.zero));
            }
            finally
            {
                Object.DestroyImmediate(enemy);
            }
        }
    }
}
