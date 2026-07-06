using UnityEngine;

namespace TapKnockout.Enemy
{
    public interface IEnemyRuntimeConfigReceiver
    {
        void Initialize(EnemyConfig enemyConfig, Transform target);
    }
}
