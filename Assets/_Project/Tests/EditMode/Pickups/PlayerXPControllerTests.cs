using NUnit.Framework;
using UnityEngine;

namespace TapKnockout.Pickups.Tests
{
    public sealed class PlayerXPControllerTests
    {
        [Test]
        public void AddXP_CrossingRequirement_RaisesLevelUpAndKeepsOverflow()
        {
            var player = new GameObject("Player");

            try
            {
                var xpController = player.AddComponent<PlayerXPController>();
                xpController.SetXPCurve(new[] { 5, 8 });
                xpController.ResetProgression();

                var levelUpCount = 0;
                xpController.OnLevelUp += _ => levelUpCount++;

                xpController.AddXP(6);

                Assert.That(levelUpCount, Is.EqualTo(1));
                Assert.That(xpController.Level, Is.EqualTo(2));
                Assert.That(xpController.CurrentXP, Is.EqualTo(1));
                Assert.That(xpController.XPForNextLevel, Is.EqualTo(8));
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }
    }
}
