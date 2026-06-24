using NUnit.Framework;
using TapKnockout.Input;
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
    }
}
