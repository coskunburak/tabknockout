using NUnit.Framework;
using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Combat.Tests
{
    public sealed class DashEventsTests
    {
        [Test]
        public void RaiseDashStarted_NotifiesSubscribers()
        {
            var received = false;

            void Handler(DashStartedEventArgs eventArgs)
            {
                received = true;
            }

            DashEvents.OnDashStarted += Handler;

            try
            {
                DashEvents.RaiseDashStarted(new DashStartedEventArgs(null, Vector3.forward, 3.5f, 0.18f, 4f));
            }
            finally
            {
                DashEvents.OnDashStarted -= Handler;
            }

            Assert.That(received, Is.True);
        }

        [Test]
        public void DashHitEventArgs_NormalizesDashDirection()
        {
            var hitContext = new HitContext();
            var eventArgs = new DashHitEventArgs(null, hitContext, new Vector3(3f, 0f, 4f), 3.5f, 0.18f);

            Assert.That(Vector3.Distance(eventArgs.DashDirection, new Vector3(0.6f, 0f, 0.8f)), Is.LessThan(0.0001f));
            Assert.That(eventArgs.HitContext, Is.SameAs(hitContext));
        }
    }
}
