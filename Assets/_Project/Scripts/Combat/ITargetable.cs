using UnityEngine;

namespace TapKnockout.Combat
{
    /// <summary>
    /// Optional targeting contract for objects that need an aim point separate from their root.
    /// </summary>
    public interface ITargetable
    {
        bool IsTargetable { get; }
        Transform TargetTransform { get; }
        GameObject GameObject { get; }
    }
}
