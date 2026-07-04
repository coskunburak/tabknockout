using UnityEngine;

namespace TapKnockout.Camera
{
    public static class CameraFramingUtility
    {
        private const float DefaultPortraitAspect = 9f / 19.5f;

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

        public static Quaternion ResolveRigRotation(float pitchDegrees, float yawDegrees)
        {
            return Quaternion.Euler(
                Mathf.Clamp(pitchDegrees, 1f, 89f),
                yawDegrees,
                0f);
        }

        public static Vector3 ResolvePlanarForward(Quaternion rotation)
        {
            var forward = Vector3.ProjectOnPlane(rotation * Vector3.forward, Vector3.up);
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        public static float ResolveAspectSafeOrthographicSize(
            float orthographicSize,
            float aspect,
            float minimumSupportedAspect,
            float maximumSupportedAspect)
        {
            var safeAspect = aspect > 0f ? aspect : DefaultPortraitAspect;
            var minAspect = Mathf.Max(0.01f, minimumSupportedAspect);
            var maxAspect = Mathf.Max(minAspect, maximumSupportedAspect);
            var clampedAspect = Mathf.Clamp(safeAspect, minAspect, maxAspect);

            if (safeAspect < clampedAspect)
            {
                return Mathf.Max(1f, orthographicSize) * clampedAspect / safeAspect;
            }

            return Mathf.Max(1f, orthographicSize);
        }

        public static float ResolveViewHeight(
            bool useOrthographic,
            float orthographicSize,
            float fieldOfView,
            float cameraDistance)
        {
            if (useOrthographic)
            {
                return Mathf.Max(1f, orthographicSize) * 2f;
            }

            var safeDistance = Mathf.Max(0.01f, cameraDistance);
            var safeFov = Mathf.Clamp(fieldOfView, 1f, 179f);
            return 2f * safeDistance * Mathf.Tan(safeFov * 0.5f * Mathf.Deg2Rad);
        }

        public static Vector3 ResolveAnchoredCenterPosition(
            Vector3 targetPosition,
            Quaternion rotation,
            Vector2 playerViewportAnchor,
            float viewHeight,
            float aspect,
            Vector3 targetOffset,
            float forwardLookAhead,
            Vector3 worldOffset)
        {
            var safeAspect = aspect > 0f ? aspect : DefaultPortraitAspect;
            var safeViewHeight = Mathf.Max(0.01f, viewHeight);
            var anchor = new Vector2(
                Mathf.Clamp(playerViewportAnchor.x, 0f, 1f),
                Mathf.Clamp(playerViewportAnchor.y, 0f, 1f));
            var viewWidth = safeViewHeight * safeAspect;
            var right = rotation * Vector3.right;
            var up = rotation * Vector3.up;
            var planarForward = ResolvePlanarForward(rotation);

            return targetPosition
                + targetOffset
                + worldOffset
                + planarForward * Mathf.Max(0f, forwardLookAhead)
                + right * ((0.5f - anchor.x) * viewWidth)
                + up * ((0.5f - anchor.y) * safeViewHeight);
        }

        public static Vector3 ResolveCameraPosition(Vector3 centerPosition, Quaternion rotation, float cameraDistance)
        {
            return centerPosition - (rotation * Vector3.forward) * Mathf.Max(0.01f, cameraDistance);
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
