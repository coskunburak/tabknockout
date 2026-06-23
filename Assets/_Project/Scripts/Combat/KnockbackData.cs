using System;
using UnityEngine;

namespace TapKnockout.Combat
{
    /// <summary>
    /// Lightweight data passed with a hit when knockback should be applied by a later movement system.
    /// </summary>
    [Serializable]
    public struct KnockbackData
    {
        public Vector3 Direction;
        public float Force;
        public float Duration;
        public bool IgnoreResistance;

        public KnockbackData(Vector3 direction, float force, float duration, bool ignoreResistance = false)
        {
            Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.zero;
            Force = Mathf.Max(0f, force);
            Duration = Mathf.Max(0f, duration);
            IgnoreResistance = ignoreResistance;
        }

        public static KnockbackData None => new KnockbackData(Vector3.zero, 0f, 0f);

        public readonly bool HasKnockback =>
            Direction.sqrMagnitude > 0f &&
            Force > 0f &&
            Duration > 0f;
    }
}
