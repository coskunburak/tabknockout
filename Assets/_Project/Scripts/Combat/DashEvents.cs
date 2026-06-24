using System;

namespace TapKnockout.Combat
{
    public static class DashEvents
    {
        public static event Action<DashStartedEventArgs> OnDashStarted;
        public static event Action<DashHitEventArgs> OnDashHit;
        public static event Action<DashEndedEventArgs> OnDashEnded;
        public static event Action<DashIFrameEventArgs> OnDashIFrameStarted;
        public static event Action<DashIFrameEventArgs> OnDashIFrameEnded;

        public static void RaiseDashStarted(DashStartedEventArgs eventArgs)
        {
            OnDashStarted?.Invoke(eventArgs);
        }

        public static void RaiseDashHit(DashHitEventArgs eventArgs)
        {
            OnDashHit?.Invoke(eventArgs);
        }

        public static void RaiseDashEnded(DashEndedEventArgs eventArgs)
        {
            OnDashEnded?.Invoke(eventArgs);
        }

        public static void RaiseDashIFrameStarted(DashIFrameEventArgs eventArgs)
        {
            OnDashIFrameStarted?.Invoke(eventArgs);
        }

        public static void RaiseDashIFrameEnded(DashIFrameEventArgs eventArgs)
        {
            OnDashIFrameEnded?.Invoke(eventArgs);
        }
    }
}
