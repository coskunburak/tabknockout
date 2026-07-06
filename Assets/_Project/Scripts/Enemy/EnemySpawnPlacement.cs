using UnityEngine;

namespace TapKnockout.Enemy
{
    public static class EnemySpawnPlacement
    {
        public static Vector3 ResolveGroundedPosition(
            GameObject enemy,
            Vector3 desiredPosition,
            float fallbackGroundY,
            bool snapToGround,
            LayerMask groundLayers,
            float raycastHeight,
            float raycastDistance,
            float groundClearance,
            Transform ignoredTarget = null)
        {
            var groundY = snapToGround
                ? ResolveGroundY(enemy, desiredPosition, fallbackGroundY, groundLayers, raycastHeight, raycastDistance, ignoredTarget)
                : fallbackGroundY;

            var colliderBottomOffset = ResolveColliderBottomOffset(enemy);
            desiredPosition.y = groundY + Mathf.Max(0f, groundClearance) - colliderBottomOffset;
            return desiredPosition;
        }

        public static void PrepareRigidbodyForArenaSpawn(GameObject enemy, bool disableGravity)
        {
            if (enemy == null)
            {
                return;
            }

            var rigidbodies = enemy.GetComponentsInChildren<Rigidbody>(true);
            for (var i = 0; i < rigidbodies.Length; i++)
            {
                var body = rigidbodies[i];
                if (body == null)
                {
                    continue;
                }

                if (disableGravity)
                {
                    body.useGravity = false;
                }

                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }

        public static float ResolveColliderBottomOffset(GameObject enemy)
        {
            if (enemy == null)
            {
                return 0f;
            }

            var colliders = enemy.GetComponentsInChildren<Collider>(true);
            var lowestY = float.PositiveInfinity;
            for (var i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                if (collider == null || !collider.enabled || collider.isTrigger)
                {
                    continue;
                }

                var bounds = collider.bounds;
                if (bounds.size == Vector3.zero)
                {
                    continue;
                }

                lowestY = Mathf.Min(lowestY, bounds.min.y);
            }

            return float.IsPositiveInfinity(lowestY) ? 0f : lowestY - enemy.transform.position.y;
        }

        private static float ResolveGroundY(
            GameObject enemy,
            Vector3 desiredPosition,
            float fallbackGroundY,
            LayerMask groundLayers,
            float raycastHeight,
            float raycastDistance,
            Transform ignoredTarget)
        {
            if (groundLayers.value == 0)
            {
                return fallbackGroundY;
            }

            var safeHeight = Mathf.Max(0f, raycastHeight);
            var safeDistance = Mathf.Max(0f, raycastDistance);
            if (safeHeight <= 0f && safeDistance <= 0f)
            {
                return fallbackGroundY;
            }

            var originY = Mathf.Max(desiredPosition.y, fallbackGroundY) + safeHeight;
            var origin = new Vector3(desiredPosition.x, originY, desiredPosition.z);
            var hits = Physics.RaycastAll(
                origin,
                Vector3.down,
                safeHeight + safeDistance,
                groundLayers,
                QueryTriggerInteraction.Ignore);

            var bestDistance = float.PositiveInfinity;
            var bestY = fallbackGroundY;
            for (var i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (!IsValidGroundHit(hit.collider, enemy, ignoredTarget))
                {
                    continue;
                }

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestY = hit.point.y;
                }
            }

            return bestY;
        }

        private static bool IsValidGroundHit(Collider collider, GameObject enemy, Transform ignoredTarget)
        {
            if (collider == null || collider.isTrigger)
            {
                return false;
            }

            var hitTransform = collider.transform;
            if (enemy != null && hitTransform.IsChildOf(enemy.transform))
            {
                return false;
            }

            if (ignoredTarget != null && hitTransform.IsChildOf(ignoredTarget))
            {
                return false;
            }

            return collider.GetComponentInParent<EnemyController>() == null &&
                collider.GetComponentInParent<EnemyHealth>() == null &&
                collider.GetComponentInParent<EnemyMovement>() == null;
        }
    }
}
