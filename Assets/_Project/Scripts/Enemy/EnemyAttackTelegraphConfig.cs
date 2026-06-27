using UnityEngine;

namespace TapKnockout.Enemy
{
    [CreateAssetMenu(fileName = "EnemyAttackTelegraphConfig", menuName = "Tap Knockout/Enemies/Attack Telegraph Config")]
    public sealed class EnemyAttackTelegraphConfig : ScriptableObject
    {
        [SerializeField] private bool enabledByDefault;
        [SerializeField, Min(0f)] private float windupDuration = 0.25f;
        [SerializeField, Min(0f)] private float cancelledRetryDelay = 0.15f;

        public bool EnabledByDefault => enabledByDefault;
        public float WindupDuration => windupDuration;
        public float CancelledRetryDelay => cancelledRetryDelay;

        private void OnValidate()
        {
            windupDuration = Mathf.Max(0f, windupDuration);
            cancelledRetryDelay = Mathf.Max(0f, cancelledRetryDelay);
        }
    }
}
