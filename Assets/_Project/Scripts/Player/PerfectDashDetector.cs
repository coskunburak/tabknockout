using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Player
{
    [DisallowMultipleComponent]
    public sealed class PerfectDashDetector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerDashController dashController;
        [SerializeField] private PerfectDashConfig config;

        [Header("Fallback")]
        [SerializeField, Min(0f)] private float fallbackEventDebounceSeconds = 0.05f;
        [SerializeField, Min(0f)] private float fallbackCooldownRefundSeconds = 0.35f;
        [SerializeField] private bool fallbackRaiseProjectileDodgeEvents = true;
        [SerializeField] private bool fallbackRefundDashCooldown = true;

        private float lastPerfectDashTime = -999f;

        public int PerfectDashCount { get; private set; }
        public float EventDebounceSeconds => config != null ? config.EventDebounceSeconds : fallbackEventDebounceSeconds;
        public float CooldownRefundSeconds => config != null ? config.CooldownRefundSeconds : fallbackCooldownRefundSeconds;

        private bool RaiseProjectileDodgeEvents => config != null ? config.RaiseProjectileDodgeEvents : fallbackRaiseProjectileDodgeEvents;
        private bool RefundDashCooldown => config != null ? config.RefundDashCooldown : fallbackRefundDashCooldown;

        private void Reset()
        {
            playerHealth = GetComponent<PlayerHealth>();
            dashController = GetComponent<PlayerDashController>();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (playerHealth != null)
            {
                playerHealth.OnDamageIgnored -= HandleDamageIgnored;
                playerHealth.OnDamageIgnored += HandleDamageIgnored;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDamageIgnored -= HandleDamageIgnored;
            }
        }

        private void OnValidate()
        {
            fallbackEventDebounceSeconds = Mathf.Max(0f, fallbackEventDebounceSeconds);
            fallbackCooldownRefundSeconds = Mathf.Max(0f, fallbackCooldownRefundSeconds);
        }

        private void HandleDamageIgnored(HitContext hitContext)
        {
            if (!ShouldTriggerPerfectDash(dashController != null && dashController.IsDashInvulnerable, lastPerfectDashTime, Time.unscaledTime, EventDebounceSeconds))
            {
                return;
            }

            lastPerfectDashTime = Time.unscaledTime;
            PerfectDashCount++;

            var position = ResolveEventPosition(hitContext);
            var refundSeconds = CooldownRefundSeconds;

            if (RefundDashCooldown && dashController != null)
            {
                dashController.RefundCooldown(refundSeconds);
            }

            DashEvents.RaisePerfectDash(new PerfectDashEventArgs(
                gameObject,
                hitContext != null ? hitContext.Source : null,
                hitContext,
                position,
                refundSeconds));

            if (RaiseProjectileDodgeEvents && hitContext != null && hitContext.IsProjectileHit)
            {
                DashEvents.RaiseProjectileDodged(new ProjectileDodgeEventArgs(
                    gameObject,
                    hitContext.Source,
                    hitContext,
                    position));
            }
        }

        public static bool ShouldTriggerPerfectDash(bool isDashInvulnerable, float lastEventTime, float currentTime, float debounceSeconds)
        {
            return isDashInvulnerable && currentTime - lastEventTime >= Mathf.Max(0f, debounceSeconds);
        }

        private void ResolveReferences()
        {
            if (playerHealth == null)
            {
                playerHealth = GetComponent<PlayerHealth>();
            }

            if (dashController == null)
            {
                dashController = GetComponent<PlayerDashController>();
            }
        }

        private Vector3 ResolveEventPosition(HitContext hitContext)
        {
            if (hitContext != null && hitContext.HitPoint != Vector3.zero)
            {
                return hitContext.HitPoint;
            }

            return transform.position;
        }
    }
}
