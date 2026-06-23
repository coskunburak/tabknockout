using NUnit.Framework;
using TapKnockout.Combat;

namespace TapKnockout.Combat.Tests
{
    public sealed class CombatEventsTests
    {
        [Test]
        public void DamageEvent_CanBeCreatedWithoutSceneObjects()
        {
            var damageEvent = new DamageEvent(null, null, 12f, DamageType.Physical);

            Assert.That(damageEvent.Source, Is.Null);
            Assert.That(damageEvent.Target, Is.Null);
            Assert.That(damageEvent.Amount, Is.EqualTo(12f));
            Assert.That(damageEvent.DamageType, Is.EqualTo(DamageType.Physical));
        }

        [Test]
        public void RaiseHitResolved_NotifiesSubscribers()
        {
            var hitContext = new HitContext();
            HitContext received = null;

            void Handler(HitContext context)
            {
                received = context;
            }

            CombatEvents.OnHitResolved += Handler;

            try
            {
                CombatEvents.RaiseHitResolved(hitContext);
            }
            finally
            {
                CombatEvents.OnHitResolved -= Handler;
            }

            Assert.That(received, Is.SameAs(hitContext));
        }
    }
}
