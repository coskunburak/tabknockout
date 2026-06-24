using UnityEngine;

namespace TapKnockout.Combat
{
    public readonly struct TargetingResult
    {
        public TargetingResult(
            Transform targetTransform,
            GameObject targetGameObject,
            IDamageable damageable,
            ITargetable targetable,
            float distance,
            Vector3 direction)
        {
            TargetTransform = targetTransform;
            TargetGameObject = targetGameObject != null
                ? targetGameObject
                : targetTransform != null
                    ? targetTransform.gameObject
                    : null;
            Damageable = damageable;
            Targetable = targetable;
            Distance = Mathf.Max(0f, distance);
            Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.zero;
        }

        public static TargetingResult None => default;

        public bool HasTarget => TargetTransform != null && TargetGameObject != null;
        public Transform TargetTransform { get; }
        public GameObject TargetGameObject { get; }
        public IDamageable Damageable { get; }
        public ITargetable Targetable { get; }
        public float Distance { get; }
        public Vector3 Direction { get; }
    }
}
