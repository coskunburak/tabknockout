using NUnit.Framework;
using UnityEngine;

namespace TapKnockout.Room.Tests
{
    public sealed class RoomExitGateTests
    {
        [Test]
        public void LockUnlock_TogglesStateColliderAndVisuals()
        {
            var root = new GameObject("Gate");
            var lockedVisual = new GameObject("LockedVisual");
            var unlockedVisual = new GameObject("UnlockedVisual");

            try
            {
                lockedVisual.transform.SetParent(root.transform, false);
                unlockedVisual.transform.SetParent(root.transform, false);
                var collider = root.AddComponent<BoxCollider>();
                var gate = root.AddComponent<RoomExitGate>();
                gate.SetReferences(collider, lockedVisual, unlockedVisual);

                gate.Lock();

                Assert.That(gate.IsLocked, Is.True);
                Assert.That(collider.enabled, Is.True);
                Assert.That(lockedVisual.activeSelf, Is.True);
                Assert.That(unlockedVisual.activeSelf, Is.False);

                gate.Unlock();

                Assert.That(gate.IsLocked, Is.False);
                Assert.That(collider.enabled, Is.False);
                Assert.That(lockedVisual.activeSelf, Is.False);
                Assert.That(unlockedVisual.activeSelf, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void LockUnlock_RaisesEvents()
        {
            var root = new GameObject("Gate");

            try
            {
                var gate = root.AddComponent<RoomExitGate>();
                var lockedCount = 0;
                var unlockedCount = 0;
                gate.OnGateLocked += _ => lockedCount++;
                gate.OnGateUnlocked += _ => unlockedCount++;

                gate.Lock();
                gate.Unlock();

                Assert.That(lockedCount, Is.EqualTo(1));
                Assert.That(unlockedCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
