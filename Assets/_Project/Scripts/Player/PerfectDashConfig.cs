using UnityEngine;

namespace TapKnockout.Player
{
    [CreateAssetMenu(fileName = "PerfectDashConfig", menuName = "Tap Knockout/Player/Perfect Dash Config")]
    public sealed class PerfectDashConfig : ScriptableObject
    {
        [SerializeField, Min(0f)] private float eventDebounceSeconds = 0.05f;
        [SerializeField, Min(0f)] private float cooldownRefundSeconds = 0.35f;
        [SerializeField] private bool raiseProjectileDodgeEvents = true;
        [SerializeField] private bool refundDashCooldown = true;

        public float EventDebounceSeconds => eventDebounceSeconds;
        public float CooldownRefundSeconds => cooldownRefundSeconds;
        public bool RaiseProjectileDodgeEvents => raiseProjectileDodgeEvents;
        public bool RefundDashCooldown => refundDashCooldown;

        private void OnValidate()
        {
            eventDebounceSeconds = Mathf.Max(0f, eventDebounceSeconds);
            cooldownRefundSeconds = Mathf.Max(0f, cooldownRefundSeconds);
        }
    }
}
