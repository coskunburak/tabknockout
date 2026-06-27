using UnityEngine;

namespace TapKnockout.Combat
{
    public readonly struct DashStartedEventArgs
    {
        public DashStartedEventArgs(GameObject source, Vector3 direction, float distance, float duration, float cooldown)
        {
            Source = source;
            Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
            Distance = Mathf.Max(0f, distance);
            Duration = Mathf.Max(0f, duration);
            Cooldown = Mathf.Max(0f, cooldown);
        }

        public GameObject Source { get; }
        public Vector3 Direction { get; }
        public float Distance { get; }
        public float Duration { get; }
        public float Cooldown { get; }
    }

    public readonly struct DashHitEventArgs
    {
        public DashHitEventArgs(GameObject source, HitContext hitContext, Vector3 dashDirection, float dashDistance, float dashDuration)
        {
            Source = source;
            HitContext = hitContext;
            DashDirection = dashDirection.sqrMagnitude > 0f ? dashDirection.normalized : Vector3.forward;
            DashDistance = Mathf.Max(0f, dashDistance);
            DashDuration = Mathf.Max(0f, dashDuration);
        }

        public GameObject Source { get; }
        public HitContext HitContext { get; }
        public Vector3 DashDirection { get; }
        public float DashDistance { get; }
        public float DashDuration { get; }
    }

    public readonly struct DashEndedEventArgs
    {
        public DashEndedEventArgs(GameObject source, Vector3 direction, bool completed, float cooldownRemaining)
        {
            Source = source;
            Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
            Completed = completed;
            CooldownRemaining = Mathf.Max(0f, cooldownRemaining);
        }

        public GameObject Source { get; }
        public Vector3 Direction { get; }
        public bool Completed { get; }
        public float CooldownRemaining { get; }
    }

    public readonly struct DashIFrameEventArgs
    {
        public DashIFrameEventArgs(GameObject source, float duration)
        {
            Source = source;
            Duration = Mathf.Max(0f, duration);
        }

        public GameObject Source { get; }
        public float Duration { get; }
    }

    public readonly struct PerfectDashEventArgs
    {
        public PerfectDashEventArgs(
            GameObject source,
            GameObject incomingSource,
            HitContext avoidedHit,
            Vector3 position,
            float cooldownRefundSeconds)
        {
            Source = source;
            IncomingSource = incomingSource;
            AvoidedHit = avoidedHit;
            Position = position;
            CooldownRefundSeconds = Mathf.Max(0f, cooldownRefundSeconds);
        }

        public GameObject Source { get; }
        public GameObject IncomingSource { get; }
        public HitContext AvoidedHit { get; }
        public Vector3 Position { get; }
        public float CooldownRefundSeconds { get; }
    }

    public readonly struct ProjectileDodgeEventArgs
    {
        public ProjectileDodgeEventArgs(
            GameObject source,
            GameObject projectileSource,
            HitContext avoidedHit,
            Vector3 position)
        {
            Source = source;
            ProjectileSource = projectileSource;
            AvoidedHit = avoidedHit;
            Position = position;
        }

        public GameObject Source { get; }
        public GameObject ProjectileSource { get; }
        public HitContext AvoidedHit { get; }
        public Vector3 Position { get; }
    }
}
