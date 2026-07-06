using System.Reflection;
using NUnit.Framework;
using TapKnockout.Input;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Player.Tests
{
    public sealed class PlayerInputStateTests
    {
        [Test]
        public void Constructor_ZeroesInputBelowDeadZone()
        {
            var state = new PlayerInputState(new Vector2(0.05f, 0f), Vector2.up, 0.1f);

            Assert.That(state.MoveInput, Is.EqualTo(Vector2.zero));
            Assert.That(state.IsMovePressed, Is.False);
            Assert.That(state.IsMovingAboveThreshold, Is.False);
            Assert.That(state.LastNonZeroMoveInput, Is.EqualTo(Vector2.up));
        }

        [Test]
        public void Constructor_ClampsInputMagnitude()
        {
            var state = new PlayerInputState(new Vector2(2f, 0f), Vector2.zero, 0.1f);

            Assert.That(state.MoveInput.magnitude, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(state.LastNonZeroMoveInput, Is.EqualTo(Vector2.right));
            Assert.That(state.IsMovePressed, Is.True);
        }

        [Test]
        public void Constructor_UpdatesLastNonZeroMoveInput()
        {
            var state = new PlayerInputState(new Vector2(0f, -0.75f), Vector2.right, 0.1f);

            Assert.That(state.LastNonZeroMoveInput.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(state.LastNonZeroMoveInput.y, Is.EqualTo(-1f).Within(0.0001f));
        }

        [Test]
        public void PlayerInputReader_DisablesMouseDragMovementByDefault()
        {
            var player = new GameObject("Player");

            try
            {
                var reader = player.AddComponent<PlayerInputReader>();
                var mouseDragField = typeof(PlayerInputReader)
                    .GetField("enableMouseDragInput", BindingFlags.Instance | BindingFlags.NonPublic);

                Assert.That(mouseDragField, Is.Not.Null);
                Assert.That(mouseDragField.GetValue(reader), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void MovementLock_ZeroesHorizontalRigidbodyVelocity()
        {
            var player = new GameObject("Player");

            try
            {
                var rigidbody = player.AddComponent<Rigidbody>();
                var movement = player.AddComponent<PlayerMovementController>();
                rigidbody.isKinematic = false;
                rigidbody.linearVelocity = new Vector3(4f, 2f, -3f);

                movement.SetMovementLocked(true);

                Assert.That(rigidbody.linearVelocity.x, Is.EqualTo(0f).Within(0.0001f));
                Assert.That(rigidbody.linearVelocity.y, Is.EqualTo(2f).Within(0.0001f));
                Assert.That(rigidbody.linearVelocity.z, Is.EqualTo(0f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void MovementController_ConfiguresKinematicTopDownMotorByDefault()
        {
            var player = new GameObject("Player");

            try
            {
                var rigidbody = player.AddComponent<Rigidbody>();
                var movement = player.AddComponent<PlayerMovementController>();

                InvokePrivate(movement, "Awake");

                Assert.That(rigidbody.isKinematic, Is.True);
                Assert.That(rigidbody.useGravity, Is.False);
                Assert.That(rigidbody.collisionDetectionMode, Is.EqualTo(CollisionDetectionMode.ContinuousSpeculative));
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        private static void InvokePrivate(object target, string methodName)
        {
            target.GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(target, null);
        }
    }
}
