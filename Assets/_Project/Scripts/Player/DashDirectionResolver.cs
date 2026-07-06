using TapKnockout.Input;
using UnityEngine;

namespace TapKnockout.Player
{
    public static class DashDirectionResolver
    {
        public static Vector3 Resolve(PlayerMovementController movementController, Transform fallbackTransform)
        {
            return Resolve(movementController, fallbackTransform, null);
        }

        public static Vector3 Resolve(
            PlayerMovementController movementController,
            Transform fallbackTransform,
            MouseAimController mouseAimController)
        {
            var transformForward = fallbackTransform != null ? fallbackTransform.forward : Vector3.forward;
            var currentMoveDirection = Vector3.zero;
            var lastFacingDirection = Vector3.zero;
            var mouseAimDirection = Vector3.zero;

            if (movementController != null)
            {
                currentMoveDirection = movementController.IsMoving
                    ? movementController.CurrentMoveDirection
                    : Vector3.zero;

                lastFacingDirection = movementController.LastFacingDirection;
            }

            if (mouseAimController != null &&
                mouseAimController.TryGetAimDirection(out var aimDirection))
            {
                mouseAimDirection = aimDirection;
            }

            return Resolve(currentMoveDirection, mouseAimDirection, lastFacingDirection, transformForward);
        }

        public static Vector3 Resolve(Vector3 currentMoveDirection, Vector3 lastFacingDirection, Vector3 transformForward)
        {
            return Resolve(currentMoveDirection, Vector3.zero, lastFacingDirection, transformForward);
        }

        public static Vector3 Resolve(
            Vector3 currentMoveDirection,
            Vector3 mouseAimDirection,
            Vector3 lastFacingDirection,
            Vector3 transformForward)
        {
            if (TryFlattenNormalize(currentMoveDirection, out var current))
            {
                return current;
            }

            if (TryFlattenNormalize(mouseAimDirection, out var aim))
            {
                return aim;
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