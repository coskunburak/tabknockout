using UnityEngine;

namespace TapKnockout.Camera
{
    public static class CameraFramingUtility
    {
        public static Vector3 ResolveFollowPosition(Vector3 targetPosition, Vector3 followOffset)
        {
            return targetPosition + followOffset;
        }

        public static Vector3 ResolveLookAtPosition(Vector3 targetPosition, Vector3 lookAtOffset)
        {
            return targetPosition + lookAtOffset;
        }

        public static Vector3 ClampPositionToBounds(Vector3 position, Bounds bounds)
        {
            if (bounds.size == Vector3.zero)
            {
                return position;
            }

            return new Vector3(
                Mathf.Clamp(position.x, bounds.min.x, bounds.max.x),
                position.y,
                Mathf.Clamp(position.z, bounds.min.z, bounds.max.z));
        }

        public static Quaternion ResolveLookRotation(Vector3 cameraPosition, Vector3 lookAtPosition, Quaternion fallbackRotation)
        {
            var lookDirection = lookAtPosition - cameraPosition;
            return lookDirection.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(lookDirection.normalized, Vector3.up)
                : fallbackRotation;
        }

        public static float ResolveRotationInterpolation(float rotationSharpness, float deltaTime)
        {
            return 1f - Mathf.Exp(-Mathf.Max(0.01f, rotationSharpness) * Mathf.Max(0f, deltaTime));
        }
    }
}
