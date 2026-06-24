using UnityEngine;

namespace TapKnockout.Input
{
    /// <summary>
    /// Abstraction used by gameplay systems so movement, dash, and attack logic do not depend on raw Unity input APIs.
    /// </summary>
    public interface IPlayerInputSource
    {
        PlayerInputState CurrentInput { get; }
        Vector2 MoveInput { get; }
        Vector2 LastNonZeroMoveInput { get; }
        bool IsMovePressed { get; }
        bool IsMovingAboveThreshold { get; }
        void SetMovementDeadZone(float movementDeadZone);
    }
}
