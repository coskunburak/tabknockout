using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerTargetProvider : MonoBehaviour
    {
        [Header("Defaults")]
        [SerializeField, Min(0.1f)] private float defaultRange = 8f;
        [SerializeField] private LayerMask defaultTargetLayers;
        [SerializeField, Range(4, 128)] private int queryBufferSize = 32;

        private Collider[] overlapBuffer;

        public float DefaultRange => defaultRange;
        public LayerMask DefaultTargetLayers => defaultTargetLayers;

        private void Awake()
        {
            EnsureBuffer();
        }

        private void OnValidate()
        {
            defaultRange = Mathf.Max(0.1f, defaultRange);
            queryBufferSize = Mathf.Clamp(queryBufferSize, 4, 128);
        }

        public bool TryGetNearestTarget(out TargetingResult result)
        {
            return TryGetNearestTarget(defaultRange, defaultTargetLayers, out result);
        }

        public bool TryGetNearestTarget(float range, LayerMask targetLayers, out TargetingResult result)
        {
            result = TargetingResult.None;

            if (range <= 0f || targetLayers.value == 0)
            {
                return false;
            }

            EnsureBuffer();

            var origin = transform.position;
            var hitCount = Physics.OverlapSphereNonAlloc(
                origin,
                range,
                overlapBuffer,
                targetLayers,
                QueryTriggerInteraction.Collide);

            var bestSqrDistance = float.PositiveInfinity;
            var bestResult = TargetingResult.None;

            for (var i = 0; i < hitCount; i++)
            {
                var candidateCollider = overlapBuffer[i];
                if (!TryBuildCandidate(origin, candidateCollider, out var candidate))
                {
                    continue;
                }

                var candidateSqrDistance = candidate.Direction.sqrMagnitude > 0f
                    ? candidate.Distance * candidate.Distance
                    : 0f;

                if (candidateSqrDistance >= bestSqrDistance)
                {
                    continue;
                }

                bestSqrDistance = candidateSqrDistance;
                bestResult = candidate;
            }

            result = bestResult;
            return result.HasTarget;
        }

        private void EnsureBuffer()
        {
            if (overlapBuffer == null || overlapBuffer.Length != queryBufferSize)
            {
                overlapBuffer = new Collider[queryBufferSize];
            }
        }

        private bool TryBuildCandidate(Vector3 origin, Collider candidateCollider, out TargetingResult result)
        {
            result = TargetingResult.None;

            if (candidateCollider == null ||
                !candidateCollider.gameObject.activeInHierarchy ||
                IsSelf(candidateCollider.transform))
            {
                return false;
            }

            var targetable = candidateCollider.GetComponentInParent<ITargetable>();
            if (targetable != null && !targetable.IsTargetable)
            {
                return false;
            }

            var damageable = candidateCollider.GetComponentInParent<IDamageable>();
            if (damageable != null && !damageable.IsAlive)
            {
                return false;
            }

            var targetTransform = ResolveTargetTransform(candidateCollider, targetable, damageable);
            var targetGameObject = ResolveTargetGameObject(candidateCollider, targetable, damageable, targetTransform);

            if (targetTransform == null || targetGameObject == null)
            {
                return false;
            }

            if (!targetGameObject.activeInHierarchy)
            {
                return false;
            }
            
            var offset = targetTransform.position - origin;
            offset.y = 0f;
            var distance = offset.magnitude;

            result = new TargetingResult(
                targetTransform,
                targetGameObject,
                damageable,
                targetable,
                distance,
                offset);

            return result.HasTarget;
        }

        private bool IsSelf(Transform candidate)
        {
            return candidate == transform || candidate.IsChildOf(transform);
        }

        private static Transform ResolveTargetTransform(
            Collider candidateCollider,
            ITargetable targetable,
            IDamageable damageable)
        {
            if (targetable != null && targetable.TargetTransform != null)
            {
                return targetable.TargetTransform;
            }

            if (damageable != null && damageable.GameObject != null)
            {
                return damageable.GameObject.transform;
            }

            return candidateCollider.transform;
        }

        private static GameObject ResolveTargetGameObject(
            Collider candidateCollider,
            ITargetable targetable,
            IDamageable damageable,
            Transform targetTransform)
        {
            if (damageable != null && damageable.GameObject != null)
            {
                return damageable.GameObject;
            }

            if (targetable != null && targetable.GameObject != null)
            {
                return targetable.GameObject;
            }

            if (targetTransform != null)
            {
                return targetTransform.gameObject;
            }

            return candidateCollider.gameObject;
        }
    }
}
