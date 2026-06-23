using UnityEngine;

namespace TapKnockout.Combat
{
    /// <summary>
    /// Contract for objects that can receive combat hits. Health behavior is implemented later.
    /// </summary>
    public interface IDamageable
    {
        bool IsAlive { get; }
        GameObject GameObject { get; }
        void ReceiveHit(HitContext hitContext);
    }
}
