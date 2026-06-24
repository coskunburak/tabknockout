using UnityEngine;

namespace TapKnockout.Player
{
    public static class DashDirectionResolver
    {
        public static Vector3 Resolve(PlayerMovementController movementController, Transform fallbackTransform)
        {
            var transformForward = fallbackTransform != null ? fallbackTransform.forward : Vector3.forward;

            if (movementController == null)
            {
                return Resolve(Vector3.zero, Vector3.zero, transformForward);
            }

            var currentMoveDirection = movementController.IsMoving
                ? movementController.CurrentMoveDirection
                : Vector3.zero;

            return Resolve(currentMoveDirection, movementController.LastFacingDirection, transformForward);
        }

        public static Vector3 Resolve(Vector3 currentMoveDirection, Vector3 lastFacingDirection, Vector3 transformForward)
        {
            if (TryFlattenNormalize(currentMoveDirection, out var current))
            {
                return current;
            }

            if (TryFlattenNormalize(lastFacingDirection, out var lastFacing))
            {
                return lastFacing;
            }

            if (TryFlattenNormalize(transformForward, out var forward))
            {
                return forward;
            }

            return Vector3.forward;
        }

        private static bool TryFlattenNormalize(Vector3 direction, out Vector3 normalized)
        {
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                normalized = Vector3.zero;
                return false;
            }

            normalized = direction.normalized;
            return true;
        }
    }
}
