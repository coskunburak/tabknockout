using System;
using UnityEngine;

namespace TapKnockout.Input
{
    /// <summary>
    /// Immutable movement input snapshot after dead-zone and magnitude normalization.
    /// </summary>
    [Serializable]
    public readonly struct PlayerInputState
    {
        public PlayerInputState(Vector2 rawMoveInput, Vector2 previousLastNonZeroMoveInput, float movementDeadZone)
        {
            MovementDeadZone = Mathf.Max(0f, movementDeadZone);
            var clampedInput = Vector2.ClampMagnitude(rawMoveInput, 1f);
            IsMovingAboveThreshold = clampedInput.sqrMagnitude > MovementDeadZone * MovementDeadZone;
            MoveInput = IsMovingAboveThreshold ? clampedInput : Vector2.zero;
            LastNonZeroMoveInput = IsMovingAboveThreshold
                ? MoveInput.normalized
                : NormalizeOrZero(previousLastNonZeroMoveInput);
        }

        public Vector2 MoveInput { get; }
        public Vector2 LastNonZeroMoveInput { get; }
        public float MovementDeadZone { get; }
        public bool IsMovePressed => MoveInput.sqrMagnitude > 0f;
        public bool IsMovingAboveThreshold { get; }

        public static PlayerInputState None(float movementDeadZone = 0f)
        {
            return new PlayerInputState(Vector2.zero, Vector2.zero, movementDeadZone);
        }

        private static Vector2 NormalizeOrZero(Vector2 value)
        {
            return value.sqrMagnitude > 0f ? value.normalized : Vector2.zero;
        }
    }
}
