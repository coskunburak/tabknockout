using UnityEngine;

namespace TapKnockout.Enemy
{
    public interface IEnemyRuntimeTargetReceiver
    {
        void SetTarget(Transform target);
    }
}
